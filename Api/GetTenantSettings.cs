using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BlazorApp.Shared;
using BlazorApp.Api.Repositories;
using Microsoft.Azure.Functions.Worker;

namespace BlazorApp.Api
{
    public class GetTenantSettings
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<TenantSettings> _cosmosRepository;

        public GetTenantSettings(ILogger<GetTenantSettings> logger, CosmosDBRepository<TenantSettings> cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }
        [Function("GetTenantSettings")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("GetTenantSettings");

            IEnumerable<TenantSettings> tenantSettings = (await _cosmosRepository.GetItems()).OrderBy(t => t.TenantKey); 

            return new OkObjectResult(tenantSettings);
        }
    }
}
