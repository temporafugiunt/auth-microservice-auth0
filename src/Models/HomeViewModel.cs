using System;

namespace auth_microservice_auth0.Models
{
    /// <summary>
    /// The model for the Home Index View
    /// </summary>
    public class HomeViewModel
    {
        /// <summary>
        /// The external DNS name that any application
        /// </summary>
        public string ExternalDNS { get; set; }
    }
}