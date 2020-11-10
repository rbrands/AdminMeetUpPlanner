using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using BlazorApp.Shared;
using Newtonsoft.Json;

namespace BlazorApp.Client.Utils
{
    public class BackendApiRepository
    {
        HttpClient _http;

        public BackendApiRepository(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> GetConfig(string name)
        {
            return await _http.GetStringAsync($"/api/GetConfigValue?name={name}");
        }
        public async Task<ClientPrincipal> GetUserDetails()
        {
            return await _http.GetFromJsonAsync<ClientPrincipal>($"/api/GetUserDetails");
        }
        public async Task<IEnumerable<TenantSettings>> GetTenants()
        {
            return await _http.GetFromJsonAsync<IEnumerable<TenantSettings>>($"/api/GetTenantSettings");
        }
        public async Task<TenantSettings> WriteTenant(TenantSettings tenant)
        {
            HttpResponseMessage response = await _http.PostAsJsonAsync<TenantSettings>($"/api/WriteTenantSettings", tenant);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TenantSettings>(); 
        }
        public async Task DeleteTenant(TenantSettings tenant)
        {
            await _http.PostAsJsonAsync<TenantSettings>($"/api/DeleteTenantSettings", tenant);
        }
        public async Task<ServerSettings> GetServerSettings(string tenantId)
        {
            return await _http.GetFromJsonAsync<ServerSettings>($"/api/GetServerSettings/{tenantId}");
        }
        public async Task<ClientSettings> GetClientSettings(string tenantId)
        {
            return await _http.GetFromJsonAsync<ClientSettings>($"/api/GetClientSettings/{tenantId}");
        }
        public async Task<ServerSettings> WriteServerSettings(string tenantId, ServerSettings serverSettings)
        {
            HttpResponseMessage response = await _http.PostAsJsonAsync<ServerSettings>($"/api/WriteServerSettings/{tenantId}", serverSettings);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ServerSettings>();
        }
        public async Task<ClientSettings> WriteClientSettings(string tenantId, ClientSettings clientSettings)
        {
            HttpResponseMessage response = await _http.PostAsJsonAsync<ClientSettings>($"/api/WriteClientSettings/{tenantId}", clientSettings);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ClientSettings>();
        }
        public async Task<TrackingReport> ExportTrackingReport(TrackingReportRequest request)
        {
            HttpResponseMessage response = await _http.PostAsJsonAsync<TrackingReportRequest>($"/api/ExportTrackingReport", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TrackingReport>();
        }
        public async Task<IEnumerable<ExportLogItem>> GetExportLog(string tenantId)
        {
            return await _http.GetFromJsonAsync<IEnumerable<ExportLogItem>>($"/api/GetExportLog/{tenantId}");
        }
    }
}
