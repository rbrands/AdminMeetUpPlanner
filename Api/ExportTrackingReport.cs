using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BlazorApp.Shared;
using Newtonsoft.Json;
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;
using Microsoft.Azure.Functions.Worker;

namespace BlazorApp.Api
{
    public class ExportTrackingReport
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<CalendarItem> _cosmosRepository;
        private CosmosDBRepository<Participant> _participantRepository;
        private CosmosDBRepository<ExportLogItem> _logRepository;
        private CosmosDBRepository<TenantSettings> _tenantRepository;

        public ExportTrackingReport(ILogger<ExportTrackingReport> logger,
                                   CosmosDBRepository<TenantSettings> tenantRepository,
                                   CosmosDBRepository<CalendarItem> cosmosRepository,
                                   CosmosDBRepository<Participant> participantRepository,
                                   CosmosDBRepository<ExportLogItem> logRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
            _participantRepository = participantRepository;
            _logRepository = logRepository;
            _tenantRepository = tenantRepository;
        }
        /// <summary>
        /// Gets the current version of ServerSettings.         /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [Function("ExportTrackingReport")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ExportTrackingReport")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TrackingReportRequest trackingRequest = JsonConvert.DeserializeObject<TrackingReportRequest>(requestBody);
            // Get tenant settings to check authorization
            TenantSettings tenant = await _tenantRepository.GetItem(trackingRequest.TenantId);
            if (null == tenant)
            {
                return new BadRequestObjectResult($"Tenant with id {trackingRequest.TenantId} not found.");
            }
            ClientPrincipal user = UserDetails.GetClientPrincipal(req);
            _logger.LogInformation($"ExportTrackingReport for {tenant.TenantName} called from {user.UserDetails}");
            if (!user.IsInRole(tenant.AdminRole) && !user.IsInRole(Constants.ROLE_ADMIN))
            {
                _logger.LogError($"User {user.UserDetails} not authorized for tenant {tenant.TenantName}");
                return new BadRequestObjectResult($"User not authorized for ExportTrackingReport.");
            }
            // Add caller to request
            trackingRequest.RequestorFirstName = user.IdentityProvider;
            trackingRequest.RequestorLastName = user.UserDetails;
            _logger.LogInformation($"ExportTrackingReport called from {trackingRequest.RequestorFirstName}/{trackingRequest.RequestorLastName} of requestor.");
            // Check request
            if (String.IsNullOrEmpty(trackingRequest.TrackFirstName) || String.IsNullOrEmpty(trackingRequest.TrackLastName))
            {
                _logger.LogWarning("ExportTrackingReport called without name of person to track.");
                return new BadRequestObjectResult("Track name missing.");
            }
            // Get a list of all CalendarItems
            IEnumerable<CalendarItem> rawListOfCalendarItems;
            if (null == tenant.TenantKey)
            {
                rawListOfCalendarItems = await _cosmosRepository.GetItems(d => (d.Tenant ?? String.Empty) == String.Empty && !d.IsCanceled);
            }
            else
            {
                rawListOfCalendarItems = await _cosmosRepository.GetItems(d => d.Tenant.Equals(tenant.TenantKey) && !d.IsCanceled);
            }
            List<ExtendedCalendarItem> resultCalendarItems = new List<ExtendedCalendarItem>(50);
            // Filter the CalendarItems that are relevant
            foreach (CalendarItem item in rawListOfCalendarItems)
            {
                // Read all participants for this calendar item
                IEnumerable<Participant> participants = await _participantRepository.GetItems(p => p.CalendarItemId.Equals(item.Id));
                // Only events where the person was part of will be used.
                if (!item.WithoutHost && item.EqualsHost(trackingRequest.TrackFirstName, trackingRequest.TrackLastName) || null != participants.Find(trackingRequest.TrackFirstName, trackingRequest.TrackLastName))
                {
                    ExtendedCalendarItem extendedItem = new ExtendedCalendarItem(item);
                    extendedItem.ParticipantsList = participants;
                    resultCalendarItems.Add(extendedItem);
                }

            }
            IEnumerable<ExtendedCalendarItem> orderedList = resultCalendarItems.OrderBy(d => d.StartDate);
            // Build template for marker list corresponding to orderedList above
            List<CompanionCalendarInfo> relevantCalendarList = new List<CompanionCalendarInfo>(50);
            int calendarSize = 0;
            foreach (ExtendedCalendarItem e in orderedList)
            {
                relevantCalendarList.Add(new CompanionCalendarInfo(e));
                ++calendarSize;
            }

            // Assemble report
            TrackingReport report = new TrackingReport(trackingRequest);
            report.CompanionList = new List<Companion>(50);
            report.CalendarList = relevantCalendarList;
            int calendarIndex = 0;
            foreach (ExtendedCalendarItem calendarItem in orderedList)
            {
                if (!calendarItem.WithoutHost)
                {
                    report.CompanionList.AddCompanion(calendarItem.HostFirstName, calendarItem.HostLastName, calendarItem.HostAdressInfo, calendarSize, calendarIndex);
                }
                foreach (Participant p in calendarItem.ParticipantsList)
                {
                    report.CompanionList.AddCompanion(p.ParticipantFirstName, p.ParticipantLastName, p.ParticipantAdressInfo, calendarSize, calendarIndex);
                }
                ++calendarIndex;
            }
            report.CreationDate = DateTime.Now;
            ExportLogItem log = new ExportLogItem(trackingRequest);
            log.TimeToLive = Constants.LOG_TTL;
            if (null != tenant.TenantKey)
            {
                log.Tenant = tenant.TenantKey;
            }
            await _logRepository.CreateItem(log);

            return new OkObjectResult(report);

        }
    }
}
