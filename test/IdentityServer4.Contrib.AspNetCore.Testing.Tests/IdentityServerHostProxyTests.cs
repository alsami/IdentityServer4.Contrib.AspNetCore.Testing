using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using IdentityServer4.Contrib.AspNetCore.Testing.Builder;
using IdentityServer4.Contrib.AspNetCore.Testing.Configuration;
using IdentityServer4.Contrib.AspNetCore.Testing.Services;
using IdentityServer4.Models;
using Xunit;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Tests
{
    public class IdentityServerHostProxyTests
    {
        [Fact]
        public async Task GetDiscoverResponseAsync_ValidConfiguration_Succeeds()
        {
            var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[] {"api1"},
                AllowedGrantTypes = new[] {GrantType.ClientCredentials},
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200
            };

            var hostBuilder = new IdentityServerTestHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .AddApiScopes(new ApiScope("api1"))
                .CreateHostBuilder(new AutofacServiceProviderFactory(), ContainerBuilderConfiguration.ConfigureContainer);

            var identityServerProxy = new IdentityServerHostProxy(hostBuilder);

            var response = await identityServerProxy.GetDiscoverResponseAsync();

            Assert.NotNull(response);
        }
    }
}