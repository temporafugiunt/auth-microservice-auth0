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
  public class HomeController : Controller
  {
    public HomeController(IOptions<SandboxAppOptions> sandboxAppOptions) => SandboxAppOptions = sandboxAppOptions.Value;

    private SandboxAppOptions SandboxAppOptions { get; }

    public IActionResult Index()
    {
      var environment = Request.Cookies["environment"];
      return View(new HomeViewModel() { ExternalDNS = SandboxAppOptions.ExternalDNSName, href = $"{SandboxAppOptions.ExternalDNSName}/app1/", environment = environment });
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
