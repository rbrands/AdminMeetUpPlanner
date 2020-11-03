using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace BlazorApp.Shared
{
    public class TenantSettings : CosmosDBEntity
    {
        [Required(ErrorMessage = "Bitte Bezeichnung für den Tenant eingeben"), MaxLength(180, ErrorMessage = "Tenantbezeichnung bitte kürzer als 180 Zeichen.")]
        public string TenantName { get; set; }
        [MaxLength(80, ErrorMessage = "Tenant-Key bitte kürzer als 80 Zeichen.")]
        public string TenantKey { get; set; }
        [Required(ErrorMessage = "Primary URL erforderlich."), MaxLength(180, ErrorMessage = "Primary URL bitte kürzer als 180 Zeichen.")]
        public string PrimaryUrl { get; set; }
        public List<string> AlternativeUrls { get; set; }
        public Boolean ClubMemberShipAllow { get; set; } = true;
        public Boolean GuestNameShown { get; set; } = true;
    }
}
