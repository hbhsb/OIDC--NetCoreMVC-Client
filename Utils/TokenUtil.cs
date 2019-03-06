using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace SampleMvcApp.Utils
{
    public static class TokenUtil
    {
        public static async Task RenewTokenAsync(this HttpContext httpContext, IConfiguration configuration)
        {
            var client = new HttpClient();
            DiscoveryResponse discoveryResponse = await client.GetDiscoveryDocumentAsync("http://localhost:5000");
            if (discoveryResponse.IsError)
            {
                throw new Exception(discoveryResponse.Error);
            }

            var refreshToken = await httpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
            TokenResponse tokenResponse = await client.RequestRefreshTokenAsync(new RefreshTokenRequest()
            {
                Address = discoveryResponse.TokenEndpoint,
                ClientId = configuration["TestAuth:ClientId"],
                ClientSecret = configuration["TestAuth:ClientSecret"],
                Scope = "api1 openid profile offline_access",
                GrantType = OpenIdConnectGrantTypes.RefreshToken,
                RefreshToken = refreshToken
            });
            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error);
            }

            var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResponse.ExpiresIn);

            var tokens = new[]
            {
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.IdToken,
                    Value = tokenResponse.IdentityToken
                },
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.AccessToken,
                    Value = tokenResponse.AccessToken
                },
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.RefreshToken,
                    Value = tokenResponse.RefreshToken
                },
                new AuthenticationToken
                {
                    Name = "expires_at",
                    Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                }
            };

            // 获取身份认证的结果，包含当前的principal和properties
            var currentAuthenticateResult =
                await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //把新token保存起来
            currentAuthenticateResult.Properties.StoreTokens(tokens);

            //登录
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                currentAuthenticateResult.Principal, currentAuthenticateResult.Properties);
        }
    }
}
