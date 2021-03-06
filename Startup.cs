﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using SampleMvcApp.Options;
using SampleMvcApp.Utils;

namespace SampleMvcApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);
            Configuration = configurationBuilder.Build();

        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => HostingEnvironment.IsProduction();
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthenticationCore(options=>
            {
                options.AddScheme<SimpleAuthenticationHandler>("SimpleScheme", "demo scheme");
            });

            // Add authentication services
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "SimpleScheme";
            })
            .AddCookie()
            .AddOpenIdConnect("TestAuth", options => {
                // Set the authority
                options.Authority = $"{Configuration["TestAuth:Domain"]}";

                // Configure the Client ID and Client Secret
                options.ClientId = Configuration["TestAuth:ClientId"];
                options.ClientSecret = Configuration["TestAuth:ClientSecret"];
                options.RequireHttpsMetadata = false;
                // Set response type to code
                options.ResponseType = "code";
                
                // Configure the scope
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("offline_access");
                options.Scope.Add("sysId");
                options.Scope.Add("api1");
                options.Scope.Add("profile");
                options.Scope.Add("https://quickstarts/api");
                options.CallbackPath = new PathString("/callback");
                options.GetClaimsFromUserInfoEndpoint = false;
                options.ClaimsIssuer = "TestAuth";
                
                // Saves tokens to the AuthenticationProperties
                options.SaveTokens = true;

                options.Events = new OpenIdConnectEvents
                {
                    // handle the logout redirection 
                    OnRedirectToIdentityProviderForSignOut = (context) =>
                    {
                        var logoutUri = $"{Configuration["TestAuth:Domain"]}/Account/Logout?logoutId={Configuration["TestAuth:ClientId"]}";

                        var postLogoutUri = context.Properties.RedirectUri;
                        if (!string.IsNullOrEmpty(postLogoutUri))
                        {
                            if (postLogoutUri.StartsWith("/"))
                            {
                                // transform to absolute
                                var request = context.Request;
                                postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                            }
                            logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                        }

                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
