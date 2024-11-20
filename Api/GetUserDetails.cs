using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BlazorApp.Shared;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;

namespace BlazorApp.Api
{
    public static class GetUserDetails
    {
        [Function("GetUserDetails")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetUserDetails called");
            ClientPrincipal user = Utils.UserDetails.GetClientPrincipal(req);

            return new OkObjectResult(user);
        }
    }
}
