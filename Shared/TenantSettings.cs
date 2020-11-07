﻿using System;
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
        [RegularExpression(@"^https\:\/\/[0-9a-z]([-.\w]*[0-9a-z])*(:(0-9)*)*(\/?)([a-z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$", ErrorMessage = "Bitte eine gültige URL https://ausfahrten ... eingeben")]
        public string PrimaryUrl { get; set; }
        public Boolean ClubMemberShipAllow { get; set; } = true;
        public Boolean GuestNameShown { get; set; } = true;
        public Boolean IsLocked { get; set; } = false;
        public string LockMessage { get; set; }
        public Boolean TracksEnabled { get; set; } = true;
    }
}
