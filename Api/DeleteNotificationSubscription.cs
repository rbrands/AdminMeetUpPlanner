using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BlazorApp.Shared;
using BlazorApp.Api.Repositories;
using Microsoft.Azure.Functions.Worker;

namespace BlazorApp.Api
{
    public class DeleteNotificationSubscription
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<NotificationSubscription> _cosmosRepository;

        public DeleteNotificationSubscription(ILogger<DeleteNotificationSubscription> logger, CosmosDBRepository<NotificationSubscription> cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [Function("DeleteNotificationSubscription")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("DeleteNotificationSubscription");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            NotificationSubscription notificationSubscription = JsonConvert.DeserializeObject<NotificationSubscription>(requestBody);
            await _cosmosRepository.DeleteItemAsync(notificationSubscription.Id);

            return new OkResult();
        }
    }
}
