using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker;
using BlazorApp.Api.Repositories;

namespace BlazorApp.Api
{
    public class GetUserDetails
    {
        private readonly ILogger _logger;
        public GetUserDetails(ILogger<GetUserDetails> logger)
        {
            _logger = logger;
        }

        [Function("GetUserDetails")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("GetUserDetails called");
            ClientPrincipal user = Utils.UserDetails.GetClientPrincipal(req);

            return new OkObjectResult(user);
        }
    }
}
