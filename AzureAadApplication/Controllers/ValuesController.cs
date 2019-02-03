using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using AzureAadApplication.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureAadApplication.Controllers
{
    [Route("api/values")]
    public class ValuesController : Controller
    {
        private readonly AzureAd azureAd;
        public ValuesController(IOptions<AzureAd> azureAdSettings)
        {
            this.azureAd = azureAdSettings.Value;
        }

        //[Authorize]
        //[HttpGet]
        //public string GetName()
        //{
        //    return "Mohit Dhingra server1";
        //    HttpClient client = new HttpClient();
        //    string token = null;
        //    Task.Run(async () => { token = await GetOnBehalfToken(); }).Wait();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //    var path = "http://localhost:58788/api/Data/";
        //    var prodPath = "https://azureaadapi.azurewebsites.net/api/values/";


        //    HttpResponseMessage response = null;
        //    Task.Run(async () => { response = await client.GetAsync(path); }).Wait();
        //    if (response.IsSuccessStatusCode)
        //    {
        //        string result = null;
        //        Task.Run(async () => { result = await response.Content.ReadAsStringAsync(); }).Wait();
        //        return result;
        //    }
        //    return "Hi! Mohit. How are you (From Server1)?";
        //}

        private async Task<string> GetOnBehalfToken()
        {
            // Get User Assertion
            var authenticateInfo = await HttpContext.Authentication.GetAuthenticateInfoAsync("Bearer");
            string incomingClientToken = authenticateInfo.Properties.Items[".Token.access_token"];
            UserAssertion userAssertion = new UserAssertion(incomingClientToken, "urn:ietf:params:oauth:grant-type:jwt-bearer");

            // Get oauth token using client credentials
            string token = string.Empty;
            string authString = string.Format(this.azureAd.AadInstance, this.azureAd.TenantId);
            AuthenticationContext authenticationContext = new AuthenticationContext(authString, false);

            //config for oauth client credentials
            //string resource = "https://graph.windows.net";
            ClientCredential clientCredential = new ClientCredential(this.azureAd.ClientId, this.azureAd.AppKey);

            var syncAudience = "https://enterprisedirectory.onmicrosoft.com/45854ee0-608a-4dff-bc39-cec937958b2f";
            var synaudience12 = "https://azureaadapi.azurewebsites.net/api/values/";
            AuthenticationResult result = await authenticationContext.AcquireTokenAsync(synaudience12, clientCredential);
            return result.AccessToken;
        }

        [HttpGet]
        [Authorize]
        public string GetLatestData()
        {
            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;
            if(claims==null || !claims.Any())
                return "Hi! Mohit. Sorry this request is not authenticated.";

            return "Hi! Mohit. You are successfully authenticated";
        }

        ////[HttpGet]
        ////[Authorize]
        ////public string GetDataFromApp2()
        ////{
        ////    HttpClient client = new HttpClient();
        ////    string token = null;
        ////    Task.Run(async () => { token = await GetBearerToken(); }).Wait();
        ////    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        ////    var path = "http://localhost:58788/api/Data/";
        ////    var prodPath = "https://azureaadapi.azurewebsites.net/api/values/";

        ////    ////var newPath = "http://localhost:58788/home/getdata";
        ////    var newPath = "http://azureaadapi.azurewebsites.net/home/getdata";


        ////    HttpResponseMessage response = null;
        ////    Task.Run(async () => { response = await client.GetAsync(newPath); }).Wait();
        ////    if (response.IsSuccessStatusCode)
        ////    {
        ////        string result = null;
        ////        Task.Run(async () => { result = await response.Content.ReadAsStringAsync(); }).Wait();
        ////        return result;
        ////    }
        ////    return "Hi! Mohit. How are you (From Server1)?";
        ////}

        private async Task<string> GetBearerToken()
        {
            string token = string.Empty;

            // Get oauth token using client credentials
            string authString = "https://login.microsoftonline.com/" + this.azureAd.TenantId;

            AuthenticationContext authenticationContext = new AuthenticationContext(authString, false);

            //config for oauth client credentials
            ClientCredential clientCredential = new ClientCredential(this.azureAd.ClientId, this.azureAd.AppKey);
            //string resource = "https://graph.windows.net";
            //string audience = "https://enterprisedirectory.onmicrosoft.com/d64fb064-9628-466d-a2e3-304dba885b0e";
            string audience = "https://enterprisedirectory.onmicrosoft.com/078b34e6-3ae7-47fb-8b48-604832353071";

            AuthenticationResult result = await authenticationContext.AcquireTokenAsync(audience, clientCredential);
            return result.AccessToken;
        }
    }
}
