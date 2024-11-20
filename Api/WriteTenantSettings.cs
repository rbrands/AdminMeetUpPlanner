using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BlazorApp.Shared;
using BlazorApp.Api.Repositories;
using Microsoft.Azure.Functions.Worker;

namespace BlazorApp.Api
{
    public class WriteTenantSettings
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<TenantSettings> _cosmosRepository;

        public WriteTenantSettings(ILogger<WriteTenantSettings> logger, CosmosDBRepository<TenantSettings> cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [Function("WriteTenantSettings")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("WriteTenantSettings");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TenantSettings tenantSettings = JsonConvert.DeserializeObject<TenantSettings>(requestBody);
            tenantSettings = await _cosmosRepository.UpsertItem(tenantSettings);

            return new OkObjectResult(tenantSettings);
        }
    }
}
