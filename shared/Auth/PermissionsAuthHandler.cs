using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using Serilog;

namespace shared.Auth
{
    public class HasPermissionRequirement : IAuthorizationRequirement
    {
        public string Issuer { get; }
        public string Permission { get; }

        public HasPermissionRequirement(string permission, string issuer)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        }
    }

    public class HasPermissionHandler : AuthorizationHandler<HasPermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasPermissionRequirement requirement)
        {
            // Log.Information(String.Format("Testing for permission: {0}", requirement.Permission));
            // string message = "";
            // foreach (var claim in context.User.Claims) {
            //     message += claim.Type + ": " + claim.Value + ", ";
            // }
            // Log.Information(message);
            
            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == "permissions" && c.Issuer == requirement.Issuer)) {
                if (context.User.Claims.Count() == 0) {
                    Log.Information("Request missing access token.");
                } else {
                    Log.Information("Token missing permissions or wrong issuer");
                }

                return Task.CompletedTask;
            }
  
            // Succeed if the scope array contains the required scope
            if (context.User.Claims
                    .Where(c => c.Type == "permissions")
                    .Select(c => c.Value)
                    .Any(x => x == requirement.Permission)
            ) 
            {
                //Log.Information("Auth Succeded");
                context.Succeed(requirement);
            } else {
                //Log.Information("auth failed");
            }
            return Task.CompletedTask;
        }
    }
}