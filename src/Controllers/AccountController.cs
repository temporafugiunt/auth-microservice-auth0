﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Options;

namespace auth_microservice_auth0.Controllers
{
    public class AccountController : Controller
    {
        public AccountController(IOptions<SandboxAppOptions> sandboxAppOptions) => SandboxAppOptions = sandboxAppOptions.Value;

        private SandboxAppOptions SandboxAppOptions { get; }

        public async Task Login(string returnUrl = "/")
        {
            await HttpContext.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        [Authorize]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties
            {
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be whitelisted in the
                // **Allowed Logout URLs** settings for the app.
                RedirectUri = Url.Action("Index", "Auth")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Expire environment cookie since user has logged out.
            var cookieOptions = new CookieOptions();
            cookieOptions.Expires = DateTime.Now.AddMonths(-1);
            cookieOptions.Domain = SandboxAppOptions.ExternalDNSName;
            Response.Cookies.Delete("environment");
            Response.Cookies.Append("environment", "", cookieOptions);
        }
    }
}
