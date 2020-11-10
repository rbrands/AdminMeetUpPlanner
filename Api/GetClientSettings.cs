using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BlazorApp.Shared;
using System.Web.Http;
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;

namespace BlazorApp.Api
{
    public class GetClientSettings
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosDBRepository<ClientSettings> _cosmosRepository;
        private CosmosDBRepository<TenantSettings> _tenantRepository;

        public GetClientSettings(ILogger<GetClientSettings> logger, 
                                 IConfiguration config,
                                 CosmosDBRepository<TenantSettings> tenantRepository,
                                 CosmosDBRepository<ClientSettings> cosmosRepository)
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
            _tenantRepository = tenantRepository;
        }

        /// <summary>
        /// Get the current ClientSettings
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [FunctionName(nameof(GetClientSettings))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetClientSettings/{id}")] HttpRequest req, string id)
        {
            // Get tenant settings to check authorization
            TenantSettings tenant = await _tenantRepository.GetItem(id);
            if (null == tenant)
            {
                return new BadRequestErrorMessageResult($"Tenant with id {id} not found.");
            }
            ClientPrincipal user = UserDetails.GetClientPrincipal(req);
            _logger.LogInformation($"GetClientSettings for {tenant.TenantName} called from {user.UserDetails}");
            if (!user.IsInRole(tenant.AdminRole) && !user.IsInRole(Constants.ROLE_ADMIN))
            {
                _logger.LogError($"User {user.UserDetails} not authorized for tenant {tenant.TenantName}");
                return new BadRequestErrorMessageResult($"User not authorized for GetClientSettings.");
            }

            // Read settings by assembling key            
            string key = Constants.KEY_CLIENT_SETTINGS;
            if (null != tenant.TenantKey)
            {
                key += "-" + tenant.TenantKey;
            }
            ClientSettings clientSettings = await _cosmosRepository.GetItemByKey(key);
            if (null == clientSettings)
            {
                // If a new tenant is created there are no ClientSettings ==> create defaults
                clientSettings = new ClientSettings()
                {
                    Title = Constants.DEFAULT_TITLE,
                    FurtherInfoLink = Constants.DEFAULT_LINK,
                    FurtherInfoTitle = Constants.DEFAULT_LINK_TITLE,
                    Disclaimer = Constants.DEFAULT_DISCLAIMER,
                    GuestDisclaimer = Constants.DEFAULT_GUEST_DISCLAIMER

                };
            }
            return new OkObjectResult(clientSettings);
        }
    }
}
