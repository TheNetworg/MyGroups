using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MyGroups.Models
{
    public class ManagementTenantList
    {
        [JsonProperty("value")]
        public List<ManagementTenant> Value;
    }
    public class ManagementTenant
    {
        [JsonProperty("id")]
        public string Id;
        [JsonProperty("tenantId")]
        public string TenantId;
    }
}
