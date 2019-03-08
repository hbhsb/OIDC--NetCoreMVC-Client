using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace SampleMvcApp.Utils
{
    public class SimpleAuthenticationHandler : IAuthenticationHandler, IAuthenticationSignInHandler, IAuthenticationSignOutHandler
    {

        public AuthenticationScheme Scheme { get; private set; }
        protected HttpContext Context { get; private set; }
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            Scheme = scheme;
            Context = context;
            return Task.CompletedTask;
        }

        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            var cookie = Context.Request.Cookies["myAuthenticateCookie"];
            if (string.IsNullOrEmpty(cookie))
            {
                return AuthenticateResult.NoResult();
            }
            byte[] ticketBytes = Convert.FromBase64String(cookie);
            AuthenticationTicket authenticationTicket = new TicketSerializer().Deserialize(ticketBytes);
            return AuthenticateResult.Success(authenticationTicket);
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            Context.Response.Redirect("/simple/login");
            return Task.CompletedTask;
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            Context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }

        public Task SignOutAsync(AuthenticationProperties properties)
        {
            Context.Response.Cookies.Delete("myAuthenticateCookie");
            return Task.CompletedTask;
        }

        public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            var ticket = new AuthenticationTicket(user, properties, Scheme.Name);

            Context.Response.Cookies.Append("myAuthenticateCookie", Convert.ToBase64String(new TicketSerializer().Serialize(ticket)));
            return Task.CompletedTask;
        }
    }
}
