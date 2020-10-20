using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BlazorApp.Shared;
using System.Collections.Generic;

namespace BlazorApp.Api
{
    public static class GetUserDetails
    {
        [FunctionName("GetUserDetails")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            ClientPrincipal user = new ClientPrincipal()
            {
                IdentityProvider = "devtest",
                UserId = "test123",
                UserDetails = "rbrands",
                UserRoles = new String[] { "anonymous", "authenticated" }
            };

            string header = req.Headers["x-ms-client-principal"];
            if (!String.IsNullOrEmpty(header))
            {
                var decoded = System.Convert.FromBase64String(header);
                var json = System.Text.ASCIIEncoding.ASCII.GetString(decoded);
                user = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return new OkObjectResult(user);
        }
    }
}
