using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using BlazorApp.Shared;
using System.Web.Http;
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;

namespace BlazorApp.Api
{
    public class GetExportLog
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<ExportLogItem> _cosmosRepository;
        private CosmosDBRepository<TenantSettings> _tenantRepository;

        public GetExportLog(ILogger<GetExportLog> logger,
                                 CosmosDBRepository<TenantSettings> tenantRepository,
                                 CosmosDBRepository<ExportLogItem> cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
            _tenantRepository = tenantRepository;
        }
        /// <summary>
        /// Gets the current version of ServerSettings.         /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetExportLog")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetExportLog/{id}")] HttpRequest req,
            string id)
        {
            // Get tenant settings to check authorization
            TenantSettings tenant = await _tenantRepository.GetItem(id);
            if (null == tenant)
            {
                return new BadRequestErrorMessageResult($"Tenant with id {id} not found.");
            }
            ClientPrincipal user = UserDetails.GetClientPrincipal(req);
            _logger.LogInformation($"GetExportLog for {tenant.TenantName} called from {user.UserDetails}");
            if (!user.IsInRole(tenant.AdminRole) && !user.IsInRole(Constants.ROLE_ADMIN))
            {
                _logger.LogError($"User {user.UserDetails} not authorized for tenant {tenant.TenantName}");
                return new BadRequestErrorMessageResult($"User not authorized for GetExportLog.");
            }

            IEnumerable<ExportLogItem> exportLog;
            if (null == tenant.TenantKey)
            {
                exportLog = await _cosmosRepository.GetItems(l => (l.Tenant ?? String.Empty) == String.Empty);
            }
            else
            {
                exportLog = await _cosmosRepository.GetItems(l => l.Tenant.Equals(tenant.TenantKey));
            }
            IEnumerable<ExportLogItem> orderedList = exportLog.OrderByDescending(l => l.RequestDate);
            return new OkObjectResult(orderedList);
        }
    }
}
