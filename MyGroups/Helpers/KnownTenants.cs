using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyGroups.Models;

namespace MyGroups.Helpers
{
    public static class KnownTenants
    {
        public static List<TenantsListTenant> Tenants = new List<TenantsListTenant>()
        {
            new TenantsListTenant
            {
                DisplayName = "TheNetw.org s.r.o.",
                DomainName = "thenetw.org",
                Id = "67266d43-8de7-494d-9ed8-3d1bd3b3a764"
            },
            new TenantsListTenant
            {
                DisplayName = "RIGANTI s.r.o.",
                DomainName = "riganti.cz",
                Id = "98245c15-c348-45a7-8be1-25afcc783931"
            }
        };
    }
}
