using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using MyGroups.Helpers;
using MyGroups.Models;

namespace MyGroups.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private AzureServiceManagement _asm;
        private MicrosoftGraph _graph;
        public HomeController(AzureServiceManagement asm, MicrosoftGraph graph)
        {
            _asm = asm;
            _graph = graph;
        }
        public async Task<IActionResult> Index()
        {
            var graphClient = _graph.GetClient();
            List<GroupDTO> groups = new List<GroupDTO>();

            var currentGroups = (await graphClient.Me.MemberOf.Request().GetAsync()).CurrentPage.OfType<Group>().Where(x => x.GroupTypes.Contains("Unified"));

            var results = new ConcurrentBag<Site>();

            await currentGroups.ParallelForEachAsync(
                async currentGroup =>
                {
                    var site = await graphClient.Groups[currentGroup.Id].Sites["root"].Request().GetAsync();
                    results.Add(site);
                }, 10);

            foreach(var currentSite in results)
            {
                GroupDTO group = new GroupDTO();
                group.Name = currentSite.DisplayName;
                group.SharePointUrl = currentSite.WebUrl;
                groups.Add(group);
            }

            groups = groups.OrderBy(x => x.Name).ToList();

            var currentTenantId = HttpContext.User.FindFirst(AzureAdClaimTypes.TenantId)?.Value;
            ViewData["tenant"] = (await _asm.GetTenantsListCachedAsync()).Tenants.Where(x => x.Id == currentTenantId).First();

            return View(groups);
        }
        public async Task TenantSelect(Guid id, string redirectUrl = "/")
        {
            var state = new Dictionary<string, string> { { "tenantId", id.ToString() } };
            await HttpContext.Authentication.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties(state)
            {
                RedirectUri = redirectUrl
            }, ChallengeBehavior.Unauthorized);
        }
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
