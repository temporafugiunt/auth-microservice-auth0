using System;

namespace auth_microservice_auth0.Models
{
  /// <summary>
  /// The model for the Home Index View
  /// </summary>
  public class HomeViewModel
  {
    /// <summary>
    /// The external DNS name that any application is logged in under.
    /// </summary>
    public string ExternalDNS { get; set; }

    /// <summary>
    /// The href for redirection.
    /// </summary>
    public string href { get; set; }

    /// <summary>
    /// The environment you will be redirected to.
    /// </summary>
    public string environment { get; set; }
  }
}