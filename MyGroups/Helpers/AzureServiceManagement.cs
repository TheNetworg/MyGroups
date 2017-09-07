using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MyGroups.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MyGroups.Helpers
{
    public class AzureServiceManagement
    {
        private string _userObjectId;
        private string _tenantId;
        private IHttpContextAccessor _httpContextAccessor;
        private AuthenticationContext _authContext;
        private ClientCredential _clientCredential;
        private IMemoryCache _memoryCache;
        public AzureServiceManagement(IHttpContextAccessor httpContextAccessor, IConfigurationRoot configuration, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _userObjectId = _httpContextAccessor.HttpContext.User.FindFirst(AzureAdClaimTypes.ObjectId)?.Value;
            _tenantId = _httpContextAccessor.HttpContext.User.FindFirst(AzureAdClaimTypes.TenantId)?.Value;
            _authContext = new AuthenticationContext($"https://login.microsoftonline.com/{_tenantId}", new NaiveSessionCache(_tenantId, _httpContextAccessor.HttpContext.Session));
            _clientCredential = new ClientCredential(configuration["Authentication:AzureAd:ClientId"], configuration["Authentication:AzureAd:ClientSecret"]);
            _memoryCache = memoryCache;
        }
        public async Task<TenantsList> GetTenantsListCachedAsync()
        {
            TenantsList tenants;
            if (!_memoryCache.TryGetValue($"{_userObjectId}-tenants", out tenants))
            {
                tenants = await GetTenantsListAsync();
                _memoryCache.Set($"{_userObjectId}-tenants", tenants, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15)));
            }
            return tenants;
        }
        public async Task<TenantsList> GetTenantsListFromManagementCachedAsync()
        {
            TenantsList tenants;
            if (!_memoryCache.TryGetValue($"{_userObjectId}-tenants", out tenants))
            {
                tenants = await GetTenantsListFromManagementAsync();
                _memoryCache.Set($"{_userObjectId}-tenants", tenants, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15)));
            }
            return tenants;
        }
        public async Task<TenantsList> GetTenantsListAsync()
        {
            var authResult = await _authContext.AcquireTokenSilentAsync("https://management.core.windows.net/", _clientCredential, new UserIdentifier(_userObjectId, UserIdentifierType.UniqueId));

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

                var response = httpClient.PostAsync("https://portal.azure.com/AzureHubs/api/tenants/List", null).Result; 
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                else
                {
                    TenantsList tenants = JsonConvert.DeserializeObject<TenantsList>(responseContent);
                    return tenants;
                }
            }
        }
        public async Task<TenantsList> GetTenantsListFromManagementAsync()
        {
            var authResult = await _authContext.AcquireTokenSilentAsync("https://management.core.windows.net/", _clientCredential, new UserIdentifier(_userObjectId, UserIdentifierType.UniqueId));

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

                var response = httpClient.GetAsync("https://management.azure.com/tenants?api-version=2016-02-01").Result;
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                else
                {
                    var tenantIds = JsonConvert.DeserializeObject<ManagementTenantList>(responseContent);
                    List<TenantsListTenant> tenants = KnownTenants.Tenants.Where(x => tenantIds.Value.Where(y => x.Id == y.TenantId).FirstOrDefault() != null).ToList();

                    return new TenantsList { Tenants = tenants };
                }
            }
        }
    }
}
