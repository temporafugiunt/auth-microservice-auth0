using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Options;
using Serilog;

namespace auth_microservice_auth0
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            HostingEnvironment = hostingEnvironment;
        }

        public IHostingEnvironment HostingEnvironment { get; }
        public Auth0Options Auth0Options { get; private set; }
        public SandboxAppOptions SandboxAppOptions { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => HostingEnvironment.IsProduction();
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<Auth0Options>(Program.Configuration.GetSection("AUTH0"));
            services.Configure<SandboxAppOptions>(Program.Configuration.GetSection("SANDBOXAPP"));

            var sp = services.BuildServiceProvider();
            Auth0Options = sp.GetService<IOptions<Auth0Options>>().Value;
            SandboxAppOptions = sp.GetService<IOptions<SandboxAppOptions>>().Value;

            ConfigureOpenIDConnectServices(services);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        private void ConfigureOpenIDConnectServices(IServiceCollection services)
        {
            Log.Information($"Application running with an STS Domain of {Auth0Options.Domain} for the external domain {SandboxAppOptions.ExternalDNSName}");

            // Add authentication services
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect("Auth0", options =>
            {
                // Set the authority to your Auth0 domain
                options.Authority = $"https://{Auth0Options.Domain}";

                // Configure the Auth0 Client ID and Client Secret
                options.ClientId = Auth0Options.ClientId;
                options.ClientSecret = Auth0Options.ClientSecret;

                // Set response type to code
                options.ResponseType = "code";

                // Configure the scope
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                // Set the callback path, so Auth0 will call back to http://localhost:3000/callback
                // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard
                options.CallbackPath = new PathString("/callback");

                // Configure the Claims Issuer to be Auth0
                options.ClaimsIssuer = "Auth0";

                options.Events = new OpenIdConnectEvents
                {
                    // handle the logout redirection
                    OnRedirectToIdentityProviderForSignOut = (context) =>
                    {
                        var logoutUri = $"https://{Auth0Options.Domain}/v2/logout?client_id={Auth0Options.ClientId}";

                        var postLogoutUri = context.Properties.RedirectUri;
                        if (!string.IsNullOrEmpty(postLogoutUri))
                        {
                            if (postLogoutUri.StartsWith("/"))
                            {
                                // transform to absolute
                                var request = context.Request;
                                postLogoutUri = request.Scheme + "://" + SandboxAppOptions.ExternalDNSName + request.PathBase + postLogoutUri;
                            }
                            logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                        }

                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = (notification) =>
                    {
                        var emailAddress = notification.Principal.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                        if (emailAddress != null)
                        {
                            Log.Information($"OnTokenValidated for email {emailAddress.Value}.");
                            if (emailAddress.Value.ToLower().Contains("mavenwave.com"))
                            {
                                Log.Information("User is a mavenwave account, adding token for beta environment");
                                notification.Response.Cookies.Append("environment", "beta");
                            }
                            else
                            {
                                Log.Information("User is NOT a mavenwave account, adding token for production environment");
                                notification.Response.Cookies.Append("environment", "production");
                            }
                        }
                        else
                        {
                            Log.Information($"OnTokenValidated but email address not found, adding token for production environment");
                            notification.Response.Cookies.Append("environment", "production");
                        }
                        return Task.CompletedTask;
                    },
                    OnRemoteSignOut = (remoteSignOutContext) =>
                    {
                        // Expire environment cookie since user has logged out.
                        var cookieOptions = new CookieOptions();
                        cookieOptions.Expires = DateTime.Now.AddMonths(-1);
                        cookieOptions.Domain = SandboxAppOptions.ExternalDNSName;
                        remoteSignOutContext.Response.Cookies.Delete("environment");
                        remoteSignOutContext.Response.Cookies.Append("environment", "", cookieOptions);

                        return Task.CompletedTask;
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }

            // app.UsePathBase("/auth");
            //app.UseHttpsRedirection();                
            if (!SandboxAppOptions.ExternalDNSName.Contains("localhost"))
            {
                app.Use((context, next) =>
                {
                    // Force https scheme behind Istio gateway to stop cookie correlation failures with Auth0:
                    // https://github.com/aspnet/Security/issues/1755
                    // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-2.1#when-it-isnt-possible-to-add-forwarded-headers-and-all-requests-are-secure
                    context.Request.Scheme = "https";
                    return next();
                });
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
