using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using BlazorApp.Shared;

namespace BlazorApp.Api.Utils
{
    public static class UserDetails
    {
        public static ClientPrincipal GetClientPrincipal(HttpRequest req)
        {
            ClientPrincipal user = new ClientPrincipal()
            {
                IdentityProvider = "devtest",
                UserId = "test123",
                UserDetails = "rbrands",
                UserRoles = new String[] { "anonymous", "authenticated", "admin" }
            };

            string header = req.Headers["x-ms-client-principal"];
            if (!String.IsNullOrEmpty(header))
            {
                var decoded = System.Convert.FromBase64String(header);
                var json = System.Text.ASCIIEncoding.ASCII.GetString(decoded);
                user = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return user;
        }
    }
}
