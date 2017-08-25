﻿using System;
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
            var tenantList = await _asm.GetTenantsListCachedAsync();
            var currentTenantId = HttpContext.User.FindFirst(AzureAdClaimTypes.TenantId)?.Value;
            var currentTenant = tenantList.Tenants.Where(x => x.Id == currentTenantId).First();

            var graphClient = _graph.GetClient();
            List<GroupDTO> groups = new List<GroupDTO>();

            var currentGroups = (await graphClient.Me.MemberOf.Request().GetAsync()).CurrentPage.OfType<Group>().Where(x => x.GroupTypes.Contains("Unified"));

            var results = new ConcurrentBag<(Group group, Site site, IOnenoteNotebooksCollectionPage notebooks, IPlannerGroupPlansCollectionPage plans)>();

            await currentGroups.ParallelForEachAsync(
                async currentGroup =>
                {
                    var site = await graphClient.Groups[currentGroup.Id].Sites["root"].Request().GetAsync();
                    var onenote = await graphClient.Groups[currentGroup.Id].Onenote.Notebooks.Request().GetAsync();
                    var planner = await graphClient.Groups[currentGroup.Id].Planner.Plans.Request().GetAsync();
                    results.Add((currentGroup, site, onenote, planner));
                }, 10);

            foreach(var result in results)
            {
                GroupDTO group = new GroupDTO();
                group.Name = result.group.DisplayName;
                group.SharePointUrl = result.site.WebUrl;
                foreach(var nb in result.notebooks)
                {
                    group.Notebooks.Add(new OneNoteDTO
                    {
                        Name = nb.DisplayName,
                        Url = nb.Links.OneNoteWebUrl.Href
                    });
                }
                foreach (var p in result.plans)
                {
                    group.Plans.Add(new PlanDTO
                    {
                        Name = p.Title,
                        Url = $"https://tasks.office.com/{currentTenant.DomainName}/Home/Planner#/plantaskboard?groupId={result.group.Id}&planId={p.Id}"
                    });
                }
                groups.Add(group);
            }

            groups = groups.OrderBy(x => x.Name).ToList();

            ViewData["tenant"] = currentTenant;

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
