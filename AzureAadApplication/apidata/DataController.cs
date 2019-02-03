using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureAadApplication.Controllers
{
    [Route("api/data")]
    public class DataController : Controller
    {
        public DataController()
        {

        }

        [Authorize]
        [HttpGet]
        public string GetName()
        {
            return "Hi! Mohit. How are you? I am fine.";
        }
    }
}
