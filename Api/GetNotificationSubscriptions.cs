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
using System.Collections.Generic;
using System.Linq;

namespace BlazorApp.Api
{
    public class GetNotificationSubscriptions
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<NotificationSubscription> _cosmosRepository;

        public GetNotificationSubscriptions(ILogger<GetNotificationSubscriptions> logger, CosmosDBRepository<NotificationSubscription> cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }
        [FunctionName("GetNotificationSubscriptions")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("GetTenantSettings");

            IEnumerable<NotificationSubscription> notifications = (await _cosmosRepository.GetItems()).OrderBy(t => t.PlannerUrl);

            return new OkObjectResult(notifications);
        }
    }
}
