using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace IdentityServer4.Testing.Infrastructure.Services
{
    public class SimpleProfileService : IProfileService

    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subject = context.Subject.Claims.First(claim => claim.Type == JwtClaimTypes.Subject).Value;

            context.IssuedClaims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, subject)
            };

            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.CompletedTask;
        }
    }
}