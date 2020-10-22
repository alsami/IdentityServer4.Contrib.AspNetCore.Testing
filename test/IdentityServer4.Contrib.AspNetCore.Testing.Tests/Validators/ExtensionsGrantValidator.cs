using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace IdentityServer4.Testing.Infrastructure.Validators
{
    public class ExtensionsGrantValidator : IExtensionGrantValidator
    {
        public Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            if (context.Request.Raw["username"] != "user" || context.Request.Raw["password"] != "password")
            {
                context.Result =
                    new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Username or password is wrong!");
                return Task.CompletedTask;
            }

            context.Result = new GrantValidationResult("user", "custom", new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, "user")
            });

            return Task.CompletedTask;
        }

        public string GrantType => "Custom";
    }
}