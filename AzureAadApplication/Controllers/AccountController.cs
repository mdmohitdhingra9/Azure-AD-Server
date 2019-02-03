using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Options;
using AzureAadApplication.Settings;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AzureAadApplication.Controllers
{
    public class MyResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
    public class AccountController : Controller
    {
        private readonly AzureAd azureAd;

        public AccountController(IOptions<AzureAd> options)
        {
            this.azureAd = options.Value;
        }
        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult SignOut()
        {
            string returnUrl = Url.Action(
                action: nameof(SignedOut),
                controller: "Account",
                values: null,
                protocol: Request.Scheme);
            return SignOut(
                new AuthenticationProperties { RedirectUri = returnUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult SignedOut()
        {
            return RedirectToAction(actionName: "Index", controllerName: "Home");
        }

        public async Task<IActionResult> GetToken()
        {
            string token = await GetBearerToken();
            return RedirectToAction(actionName: "About", controllerName: "Home");
        }

        private async Task<string> GetBearerToken()
        {
            try
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
                //var data = JsonConvert.SerializeObject(result.AccessToken);

                //var jObject = JObject.Parse(result.AccessToken);
                //var jToken = jObject.GetValue("exp");

                var usertokenonbehalf = GenerateUserTokenonBehalf(result, authenticationContext);
                return result.AccessToken;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> GenerateUserTokenonBehalf(AuthenticationResult result, AuthenticationContext authenticationContext)
        {
            try
            {
                var assertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
                UserAssertion userAssertion = new UserAssertion(result.AccessToken, assertionType);
                //var apiclientid = "c68f2f66-2c0c-476f-8476-4663e8a8f2ee";
                //var apisecret = "vYFwxt07F+MqERat399jg4ILpmZvV8oqMF+vpXNwBCQ=";
                var apiaudience = "https://enterprisedirectory.onmicrosoft.com/45854ee0-608a-4dff-bc39-cec937958b2f";

                Uri uri = new Uri(apiaudience);
                string audiencenew = uri.Scheme + "://" + uri.Authority;

                ClientCredential apiclientCredential = new ClientCredential(this.azureAd.ClientId, this.azureAd.AppKey);
                AuthenticationResult apiresult = await authenticationContext.AcquireTokenAsync(apiaudience, apiclientCredential, userAssertion);
                return apiresult.AccessToken;
            }
            catch 
            {
                return null;
            }
        }
    }
}
