using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureAadApplication.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureAadApplication.Controllers
{
    public class Actionfilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string value = string.Empty;
            Microsoft.Extensions.Primitives.StringValues stringValues = new Microsoft.Extensions.Primitives.StringValues();
            var result = context.HttpContext.Request.Headers.TryGetValue("WorkPoint365Url", out stringValues);
            base.OnActionExecuting(context);
        }
    }
    public class HomeController : Controller
    {
        private readonly AzureAd azureAd;

        public HomeController(IOptions<AzureAd> options)
        {
            this.azureAd = options.Value;

        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public async Task<IActionResult> GetToken()
        {
            string token = await GetBearerToken();
            return RedirectToAction(actionName: "About", controllerName: "Home");
        }
        private async Task<string> GetBearerToken()
        {
            string token = string.Empty;

            // Get oauth token using client credentials
            string authString = "https://login.microsoftonline.com/" + this.azureAd.TenantId;

            AuthenticationContext authenticationContext = new AuthenticationContext(authString, false);

            //config for oauth client credentials
            ClientCredential clientCredential = new ClientCredential(this.azureAd.ClientId, this.azureAd.AppKey);
            //string resource = "https://graph.windows.net";
            string audience = "https://enterprisedirectory.onmicrosoft.com/d64fb064-9628-466d-a2e3-304dba885b0e";

            AuthenticationResult result = await authenticationContext.AcquireTokenAsync(audience, clientCredential);
            return result.AccessToken;
        }
    }
}
