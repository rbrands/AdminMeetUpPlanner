using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazorApp.Shared
{
    public class ClientPrincipal
    {
        public string IdentityProvider { get; set; }
        public string UserId { get; set; }
        public string UserDetails { get; set; }
        public IEnumerable<string> UserRoles { get; set; }
        public bool IsInRole(string role)
        {
            return UserRoles.Contains(role);
        }
    }
}
