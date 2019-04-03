using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace IdentityServer4.Testing.Infrastructure.Validators
{
    public class SimpleResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (context.UserName != "user" || context.Password != "password")
            {
                context.Result =
                    new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Username or password is wrong!");
                return Task.CompletedTask;
            }

            context.Result = new GrantValidationResult("user", OidcConstants.AuthenticationMethods.Password,
                new List<Claim>
                {
                    new Claim(JwtClaimTypes.Subject, "user")
                });

            return Task.CompletedTask;
        }
    }
}