using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleMvcApp.Controllers
{
    public class SimpleController : Controller
    {
        [Authorize(AuthenticationSchemes = "SimpleScheme")]
        public IActionResult Index()
        {
            return View();
        }

        public async Task Login()
        {
            ClaimsIdentity claimsIdentity = new ClaimsIdentity("SimpleScheme");
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, "ThisIsName"));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, "bob@gmail.com"));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.MobilePhone, "12345678901"));
            await HttpContext.SignInAsync("SimpleScheme", new ClaimsPrincipal(claimsIdentity));
        }
    }
}