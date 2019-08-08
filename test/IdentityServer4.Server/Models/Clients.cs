using System.Collections.Generic;
using IdentityServer4.Models;

namespace IdentityServer4.Server.Models
{
    public static class Clients
    {
        public const string Id = "sampleclient";
        public const string Secret = "samplesecret";

        public static IEnumerable<Client> GetClients
            => new List<Client>
            {
                new Client
                {
                    ClientId = Clients.Id,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret(Clients.Secret.Sha256())
                    },
                    AllowedScopes = new List<string>
                    {
                        "api1", IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    },
                    AllowedGrantTypes = new List<string>
                    {
                        GrantType.ResourceOwnerPassword
                    },
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 60 * 60,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Absolute
                }
            };
    }
}