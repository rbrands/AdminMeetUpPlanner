using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using BlazorApp.Shared;


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

    }
}
