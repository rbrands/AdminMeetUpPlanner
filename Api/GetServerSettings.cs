using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BlazorApp.Shared;
using System.Web.Http;
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;

namespace BlazorApp.Api
{
    public class GetServerSettings
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<ServerSettings> _cosmosRepository;
        private CosmosDBRepository<TenantSettings> _tenantRepository;

        public GetServerSettings(ILogger<GetServerSettings> logger, 
                                 CosmosDBRepository<TenantSettings> tenantRepository,
                                 CosmosDBRepository<ServerSettings> cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
            _tenantRepository = tenantRepository;
        }
        /// <summary>
        /// Gets the current version of ServerSettings.         /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetServerSettings")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetServerSettings/{id}")] HttpRequest req,
            string id)
        {
            // Get tenant settings to check authorization
            TenantSettings tenant = await _tenantRepository.GetItem(id);
            if (null == tenant)
            {
                return new BadRequestErrorMessageResult($"Tenant with id {id} not found.");
            }
            ClientPrincipal user = UserDetails.GetClientPrincipal(req);
            _logger.LogInformation($"GetServerSettings for {tenant.TenantName} called from {user.UserDetails}");
            if (!user.IsInRole(tenant.AdminRole) && !user.IsInRole(Constants.ROLE_ADMIN))
            {
                _logger.LogError($"User {user.UserDetails} not authorized for tenant {tenant.TenantName}");
                return new BadRequestErrorMessageResult($"User not authorized for GetServerSettings.");
            }

            // Read settings by assembling unique key
            string settingsKey = Constants.KEY_SERVER_SETTINGS;
            if (!String.IsNullOrWhiteSpace(tenant.TenantKey))
            {
                settingsKey += "-" + tenant.TenantKey;
            }
            ServerSettings serverSettings = await _cosmosRepository.GetItemByKey(settingsKey);
            if (null == serverSettings)
            {
                // If a new tenant is created there are no ServerSettings ==> create defaults
                serverSettings = new ServerSettings()
                {
                    UserKeyword = Constants.DEFAULT_KEYWORD_USER,
                    AdminKeyword = Constants.DEFAULT_KEYWORD_ADMIN,
                    AutoDeleteAfterDays = Constants.DEFAULT_AUTO_DELETE_DAYS,
                    Tenant = tenant.TenantKey
                };
            }

            return new OkObjectResult(serverSettings);
        }
    }
}
