using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using auth_microservice_auth0.Models;
using Microsoft.Extensions.Options;
using Options;

namespace auth_microservice_auth0.Controllers
{
    public class AuthController : Controller
    {
        public AuthController(IOptions<SandboxAppOptions> sandboxAppOptions)
        {
            SandboxAppOptions = sandboxAppOptions.Value;
        }

        private SandboxAppOptions SandboxAppOptions { get; }

        public IActionResult Index()
        {
            return View(new AuthViewModel() { ExternalDNS = SandboxAppOptions.ExternalDNSName });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
