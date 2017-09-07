using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyGroups.Helpers;

namespace MyGroups.ViewComponents
{
    public class TenantSelection : ViewComponent
    {
        private AzureServiceManagement _asm;
        public TenantSelection(AzureServiceManagement asm)
        {
            _asm = asm;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tenants = await _asm.GetTenantsListFromManagementCachedAsync();
            return View(tenants);
        }
    }
}
