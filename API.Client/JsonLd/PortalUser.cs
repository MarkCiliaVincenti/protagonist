﻿using System;

namespace API.Client.JsonLd
{
    /// <summary>
    /// Hydra entity representing User of Portal UI.
    /// </summary>
    public class PortalUser : JsonLdBase
    {
        public string Email { get; set; }
        public DateTime? Created { get; set; }
        public bool Enabled { get; set; }
        public string? Password { get; set; }
    }
}