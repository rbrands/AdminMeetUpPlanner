using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BlazorApp.Shared;
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;
using Microsoft.Azure.Functions.Worker;


namespace BlazorApp.Api
{
    public class WriteServerSettings
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<ServerSettings> _cosmosRepository;
        private CosmosDBRepository<TenantSettings> _tenantRepository;

        public WriteServerSettings(ILogger<WriteServerSettings> logger,
                                  CosmosDBRepository<TenantSettings> tenantRepository,
                                  CosmosDBRepository<ServerSettings> cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
            _tenantRepository = tenantRepository;
        }

        /// <summary>
        /// Writes a new version of ServerSettings to the database. 
        /// Header "x-meetup-keyword" with the valid administrator keyword required.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [Function("WriteServerSettings")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WriteServerSettings/{id}")] HttpRequest req,
            string id)
        {
            // Get tenant settings to check authorization
            TenantSettings tenant = await _tenantRepository.GetItem(id);
            if (null == tenant)
            {
                return new BadRequestObjectResult($"Tenant with id {id} not found.");
            }
            ClientPrincipal user = UserDetails.GetClientPrincipal(req);
            _logger.LogInformation($"WriteServerSettings for {tenant.TenantName} called from {user.UserDetails}");
            if (!user.IsInRole(tenant.AdminRole) && !user.IsInRole(Constants.ROLE_ADMIN))
            {
                _logger.LogError($"User {user.UserDetails} not authorized for tenant {tenant.TenantName}");
                return new BadRequestObjectResult($"User not authorized for WriteServerSettings.");
            }

            // Write settings by assembling unique key
            string settingsKey = Constants.KEY_SERVER_SETTINGS;
            if (!String.IsNullOrWhiteSpace(tenant.TenantKey))
            {
                settingsKey += "-" + tenant.TenantKey;
            }
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ServerSettings newServerSettings = JsonConvert.DeserializeObject<ServerSettings>(requestBody);
            if (null != tenant.TenantKey)
            {
                newServerSettings.Tenant = tenant.TenantKey;
            }
            newServerSettings.LogicalKey = settingsKey;
            newServerSettings = await _cosmosRepository.UpsertItem(newServerSettings);

            return new OkObjectResult(newServerSettings);
        }
    }
}
