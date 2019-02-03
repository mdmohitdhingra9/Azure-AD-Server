using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureAadApplication.Settings
{
    public class AzureAd
    {
        public string ClientId { get; set; }

        public string TenantId { get; set; }

        public string AadInstance { get; set; }

        public string AuthCallback { get; set; }

        public string AppKey { get; set; }

        public string Audience { get; set; }

        public string Domain { get; set; }
    }
}
