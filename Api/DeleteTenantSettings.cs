using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BlazorApp.Shared;
using BlazorApp.Api.Repositories;

namespace BlazorApp.Api
{
    public class DeleteTenantSettings
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<TenantSettings> _cosmosRepository;

        public DeleteTenantSettings(ILogger<DeleteTenantSettings> logger, CosmosDBRepository<TenantSettings> cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("DeleteTenantSettings")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("DeleteTenantSettings");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TenantSettings tenantSettings = JsonConvert.DeserializeObject<TenantSettings>(requestBody);
            await _cosmosRepository.DeleteItemAsync(tenantSettings.Id);

            return new OkResult();
        }
    }
}
