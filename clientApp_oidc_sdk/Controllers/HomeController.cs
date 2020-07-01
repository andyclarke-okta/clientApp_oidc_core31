using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Okta.AspNetCore;
using okta_aspnetcore_mvc_example.Models;

namespace okta_aspnetcore_mvc_example.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }


        //[Authorize]
        public ActionResult Login()
        {
            ViewBag.Message = "Your Login Page.";

            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                var properties = new AuthenticationProperties();
                //without this, the redirect defaults to entry point of initialization
                //properties.RedirectUri = "/Home/PostLogOut";
                return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);

            }

            return RedirectToAction("PostLogin", "Home");
        }

        //[HttpPost]
        //public async Task<ActionResult> Logout()
        //{
        //    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
           
        //    return View();
        //}


        [HttpPost]
        public ActionResult Logout()
        {

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return new SignOutResult(
                new[]
                {
                         OpenIdConnectDefaults.AuthenticationScheme,
                         CookieAuthenticationDefaults.AuthenticationScheme,
                },
                new AuthenticationProperties { RedirectUri = "https://localhost:44305/Home/PostLogOut" });
            }
            return RedirectToAction("PostLogout", "Home");

        }



        public ActionResult PostLogin()
        {
            GetToken();
            return View();
        }

        public ActionResult PostLogOut()
        {

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View(HttpContext.User.Claims);
        }


        public async Task GetToken()
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");
            string idToken = await HttpContext.GetTokenAsync("id_token");
            string refreshToken = await HttpContext.GetTokenAsync("refresh_token");

            return ;
        }

    }
}
