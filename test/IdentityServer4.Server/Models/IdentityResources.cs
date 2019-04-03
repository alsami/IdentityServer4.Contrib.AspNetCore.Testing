using System.Collections.Generic;
using IdentityServer4.Models;

namespace IdentityServer4.Server.Models
{
    public static class IdentityResources
    {
        public static IEnumerable<IdentityResource> GetIdentityResources
            => new List<IdentityResource>
            {
                new IdentityServer4.Models.IdentityResources.OpenId(),
                new IdentityServer4.Models.IdentityResources.Profile()
            };
    }
}