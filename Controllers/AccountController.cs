using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SampleMvcApp.Utils;

namespace SampleMvcApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly IConfiguration configuration;

        public AccountController(IConfiguration iConfiguration)
        {
            configuration = iConfiguration;
        }

        public async Task Login(string returnUrl = "/")
        {
            await HttpContext.ChallengeAsync("TestAuth", new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        [Authorize]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("TestAuth", new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// This is just a helper action to enable you to easily see all claims related to a user. It helps when debugging your
        /// application to see the in claims populated from the Auth0 ID Token
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Claims()
        {
            string accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            string idToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
            //IAuthenticationService authenticationService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
            //AuthenticateResult authenticateResult = authenticationService.AuthenticateAsync(HttpContext, CookieAuthenticationDefaults.AuthenticationScheme).Result;
            //authenticateResult.Properties
            //    .UpdateTokenValue("access_token", accessToken + "1");
            string refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            HttpClient httpClient = new HttpClient();
            httpClient.SetBearerToken(accessToken);
            HttpResponseMessage responseMessage = await httpClient.GetAsync("http://localhost:4000/api/values");
            if (!responseMessage.IsSuccessStatusCode)
            {
                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await HttpContext.RenewTokenAsync(configuration);
                    return RedirectToAction();
                }

                throw new Exception(responseMessage.ReasonPhrase);
            }

            ViewBag.claims = await responseMessage.Content.ReadAsStringAsync();
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
