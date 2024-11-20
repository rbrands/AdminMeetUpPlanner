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
    public class WriteClientSettings
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<ClientSettings> _cosmosRepository;
        private CosmosDBRepository<TenantSettings> _tenantRepository;

        public WriteClientSettings(ILogger<WriteClientSettings> logger,
                                  CosmosDBRepository<TenantSettings> tenantRepository,
                                  CosmosDBRepository<ClientSettings> cosmosRepository)
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
        [Function("WriteClientSettings")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WriteClientSettings/{id}")] HttpRequest req,
            string id)
        {
            // Get tenant settings to check authorization
            TenantSettings tenant = await _tenantRepository.GetItem(id);
            if (null == tenant)
            {
                return new BadRequestObjectResult($"Tenant with id {id} not found.");
            }
            ClientPrincipal user = UserDetails.GetClientPrincipal(req);
            _logger.LogInformation($"WriteClientSettings for {tenant.TenantName} called from {user.UserDetails}");
            if (!user.IsInRole(tenant.AdminRole) && !user.IsInRole(Constants.ROLE_ADMIN))
            {
                _logger.LogError($"User {user.UserDetails} not authorized for tenant {tenant.TenantName}");
                return new BadRequestObjectResult($"User not authorized for WriteClientSettings.");
            }

            // Write settings by assembling unique key
            string settingsKey = Constants.KEY_CLIENT_SETTINGS;
            if (!String.IsNullOrWhiteSpace(tenant.TenantKey))
            {
                settingsKey += "-" + tenant.TenantKey;
            }
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ClientSettings newClientSettings = JsonConvert.DeserializeObject<ClientSettings>(requestBody);
            if (null != tenant.TenantKey)
            {
                newClientSettings.Tenant = tenant.TenantKey;
            }
            newClientSettings.LogicalKey = settingsKey;
            newClientSettings = await _cosmosRepository.UpsertItem(newClientSettings);

            return new OkObjectResult(newClientSettings);
        }
    }
}
