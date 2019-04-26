using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Contrib.AspNetCore.Testing.Builder;
using IdentityServer4.Contrib.AspNetCore.Testing.Configuration;
using IdentityServer4.Contrib.AspNetCore.Testing.Services;
using IdentityServer4.Models;
using IdentityServer4.Server;
using IdentityServer4.Server.Models;
using IdentityServer4.Testing.Infrastructure.Services;
using IdentityServer4.Testing.Infrastructure.Validators;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using IdentityResources = IdentityServer4.Models.IdentityResources;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Tests
{
    public class IdentityServerProxyTests
    {
        [Fact]
        public async Task IdentityServerProxy_GetDiscoverDocumentAsync_Succeeds()
        {
            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(new Client
                {
                    ClientId = "MyClient",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("MySecret".Sha256())
                    }
                })
                .AddApiResources(new ApiResource())
                .CreateWebHostBuilder();

            var identityServerClient = new IdentityServerProxy(webHostBuilder);
            var discoveryResponse = await identityServerClient.GetDiscoverResponseAsync();

            Assert.NotNull(discoveryResponse);
            Assert.False(discoveryResponse.IsError, discoveryResponse.Error);
        }

        [Fact]
        public async Task IdentityServerProxy_GetClientCredentialsAsync_Succeeds()
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

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .CreateWebHostBuilder();

            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            var tokenResponse = await identityServerProxy.GetClientAccessTokenAsync(clientConfiguration, "api1");

            Assert.NotNull(tokenResponse);
            Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
            Assert.NotNull(tokenResponse.AccessToken);
            Assert.Equal(7200, tokenResponse.ExpiresIn);
            Assert.Equal("Bearer", tokenResponse.TokenType);
        }
        
        [Fact]
        public async Task IdentityServerProxy_GetClientCredentialsAsync_Authorize_Api_Succeeds()
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

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .CreateWebHostBuilder();
            
            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            var tokenResponse = await identityServerProxy.GetClientAccessTokenAsync(clientConfiguration, "api1");

            var apiServer = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                    services.AddSingleton(identityServerProxy.IdentityServer.CreateHandler()))
                .UseStartup<IdentityServer4.Api.Startup>());

            var apiClient = apiServer.CreateClient();
            
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var apiResponse = await apiClient.GetAsync("api/auth");
            
            Assert.True(apiResponse.IsSuccessStatusCode, "should have been authenticated!");
        }

        [Fact]
        public async Task IdentityServerProxy_GetClientCredentialsAsync_WithScope_In_Parameters_Succeeds()
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

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .CreateWebHostBuilder();

            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            var tokenResponse = await identityServerProxy.GetClientAccessTokenAsync(clientConfiguration,
                new Dictionary<string, string>
                {
                    {"Scope", "api1"}
                });

            Assert.NotNull(tokenResponse);
            Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
            Assert.NotNull(tokenResponse.AccessToken);
            Assert.Equal(7200, tokenResponse.ExpiresIn);
            Assert.Equal("Bearer", tokenResponse.TokenType);
        }

        [Fact]
        public async Task
            IdentityServerProxy_GetResourceOwnerTokenAsync_Valid_User_Custom_IdentityServerBuilder_Succeeds()
        {
            var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[] {"api1", IdentityServerConstants.StandardScopes.OfflineAccess},
                AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200,
                AllowOfflineAccess = true
            };

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .UseResourceOwnerPasswordValidator(typeof(SimpleResourceOwnerPasswordValidator))
                .UseIdentityServerBuilder(services => services
                    .AddIdentityServer()
                    .AddDefaultEndpoints()
                    .AddDeveloperSigningCredential()
                )
                .CreateWebHostBuilder();

            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            var tokenResponse = await identityServerProxy.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
                new UserLoginConfiguration("user", "password"),
                "api1", "offline_access");

            Assert.NotNull(tokenResponse);
            Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
            Assert.NotNull(tokenResponse.AccessToken);
            Assert.NotNull(tokenResponse.RefreshToken);
            Assert.Equal(7200, tokenResponse.ExpiresIn);
            Assert.Equal("Bearer", tokenResponse.TokenType);
        }

        [Fact]
        public async Task
            IdentityServerProxy_GetResourceOwnerTokenAsync_Valid_User_Custom_IdentityServerBuilderOptions_Token_Endpoint_Disabled_Fails()
        {
            var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[] {"api1", IdentityServerConstants.StandardScopes.OfflineAccess},
                AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200,
                AllowOfflineAccess = true
            };

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .UseResourceOwnerPasswordValidator(typeof(SimpleResourceOwnerPasswordValidator))
                .UseIdentityServerOptionsBuilder(options => options.Endpoints.EnableTokenEndpoint = false)
                .CreateWebHostBuilder();

            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            var tokenResponse = await identityServerProxy.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
                new UserLoginConfiguration("user", "password"),
                "api1", "offline_access");

            Assert.NotNull(tokenResponse);
            Assert.True(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
        }

        [Fact]
        public async Task IdentityServerProxy_GetResourceOwnerTokenAsync_Valid_User_Typed_Validator_Succeeds()
        {
            var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[] {"api1", IdentityServerConstants.StandardScopes.OfflineAccess},
                AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200,
                AllowOfflineAccess = true
            };

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .UseResourceOwnerPasswordValidator(typeof(SimpleResourceOwnerPasswordValidator))
                .CreateWebHostBuilder();

            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            var tokenResponse = await identityServerProxy.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
                new UserLoginConfiguration("user", "password"),
                "api1", "offline_access");

            Assert.NotNull(tokenResponse);
            Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
            Assert.NotNull(tokenResponse.AccessToken);
            Assert.NotNull(tokenResponse.RefreshToken);
            Assert.Equal(7200, tokenResponse.ExpiresIn);
            Assert.Equal("Bearer", tokenResponse.TokenType);
        }

        [Fact]
        public async Task IdentityServerProxy_GetResourceOwnerTokenAsync_Valid_User_Succeeds()
        {
            var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[] {"api1", IdentityServerConstants.StandardScopes.OfflineAccess},
                AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200,
                AllowOfflineAccess = true
            };

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .UseResourceOwnerPasswordValidator(new SimpleResourceOwnerPasswordValidator())
                .CreateWebHostBuilder();

            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            var tokenResponse = await identityServerProxy.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
                new UserLoginConfiguration("user", "password"),
                "api1", "offline_access");

            Assert.NotNull(tokenResponse);
            Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
            Assert.NotNull(tokenResponse.AccessToken);
            Assert.NotNull(tokenResponse.RefreshToken);
            Assert.Equal(7200, tokenResponse.ExpiresIn);
            Assert.Equal("Bearer", tokenResponse.TokenType);
        }

        [Fact]
        public async Task IdentityServerProxy_GetResourceOwnerTokenAsync_WithScope_In_Parameters_Valid_User_Succeeds()
        {
            var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[] {"api1", IdentityServerConstants.StandardScopes.OfflineAccess},
                AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200,
                AllowOfflineAccess = true
            };

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .UseResourceOwnerPasswordValidator(new SimpleResourceOwnerPasswordValidator())
                .CreateWebHostBuilder();

            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            var tokenResponse = await identityServerProxy.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
                new UserLoginConfiguration("user", "password"),
                new Dictionary<string, string>
                {
                    {"Scope", "api1 offline_access"}
                });

            Assert.NotNull(tokenResponse);
            Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
            Assert.NotNull(tokenResponse.AccessToken);
            Assert.NotNull(tokenResponse.RefreshToken);
            Assert.Equal(7200, tokenResponse.ExpiresIn);
            Assert.Equal("Bearer", tokenResponse.TokenType);
        }

        [Fact]
        public async Task IdentityServerProxy_GetResourceOwnerTokenAsync_Invalid_User_Succeeds()
        {
            var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[] {"api1", IdentityServerConstants.StandardScopes.OfflineAccess},
                AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200,
                AllowOfflineAccess = true
            };

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .UseResourceOwnerPasswordValidator(new SimpleResourceOwnerPasswordValidator())
                .CreateWebHostBuilder();

            var identityServerClient = new IdentityServerProxy(webHostBuilder);

            var tokenResponse = await identityServerClient.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
                new UserLoginConfiguration("user", "password1"),
                "api1", "offline_access");

            Assert.NotNull(tokenResponse);
            Assert.True(tokenResponse.IsError);
        }

        [Fact]
        public async Task IdentityServerProxy_GetRefreshTokenAsync_Valid_Token_Succeeds()
        {
            var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[] {"api1", IdentityServerConstants.StandardScopes.OfflineAccess},
                AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200,
                AllowOfflineAccess = true
            };

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .UseResourceOwnerPasswordValidator(new SimpleResourceOwnerPasswordValidator())
                .CreateWebHostBuilder();

            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            var scopes = new[] {"api1", "offline_access"};

            var tokenResponse = await identityServerProxy.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
                new UserLoginConfiguration("user", "password"),
                scopes);

            // We are breaking the pattern arrange / act / assert here but we need to make sure token requested successfully first
            Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);


            var refreshTokenResponse = await identityServerProxy
                .GetRefreshTokenAsync(clientConfiguration, tokenResponse.RefreshToken,
                    scopes);

            Assert.NotNull(refreshTokenResponse);
            Assert.False(refreshTokenResponse.IsError,
                refreshTokenResponse.Error ?? refreshTokenResponse.ErrorDescription);
            Assert.NotNull(refreshTokenResponse.AccessToken);
            Assert.NotNull(refreshTokenResponse.RefreshToken);
            Assert.Equal(7200, refreshTokenResponse.ExpiresIn);
            Assert.Equal("Bearer", refreshTokenResponse.TokenType);
        }

        [Fact]
        public async Task IdentityServerProxy_GetRefreshTokenAsync_WithScope_In_Parameters_Valid_User_Succeeds()
        {
            var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[] {"api1", IdentityServerConstants.StandardScopes.OfflineAccess},
                AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200,
                AllowOfflineAccess = true
            };

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .UseResourceOwnerPasswordValidator(new SimpleResourceOwnerPasswordValidator())
                .CreateWebHostBuilder();

            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            const string scopes = "api1 offline_access";

            var tokenResponse = await identityServerProxy.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
                new UserLoginConfiguration("user", "password"),
                new Dictionary<string, string>
                {
                    {"Scope", scopes}
                });

            // We are breaking the pattern arrange / act / assert here but we need to make sure token requested successfully first
            Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);

            var refreshTokenResponse = await identityServerProxy
                .GetRefreshTokenAsync(clientConfiguration, tokenResponse.RefreshToken,
                    new Dictionary<string, string>
                    {
                        {"Scope", scopes}
                    });

            Assert.NotNull(refreshTokenResponse);
            Assert.False(refreshTokenResponse.IsError,
                refreshTokenResponse.Error ?? refreshTokenResponse.ErrorDescription);
            Assert.NotNull(refreshTokenResponse.AccessToken);
            Assert.NotNull(refreshTokenResponse.RefreshToken);
            Assert.Equal(7200, refreshTokenResponse.ExpiresIn);
            Assert.Equal("Bearer", refreshTokenResponse.TokenType);
        }

        [Fact]
        public async Task IdentityServerProxy_GetUserInfoAsync_Valid_Token_Succeeds()
        {
            var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[]
                {
                    "api1", IdentityServerConstants.StandardScopes.OfflineAccess,
                    IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile
                },
                AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200,
                AllowOfflineAccess = true
            };

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .AddIdentityResources(new IdentityResources.OpenId(), new IdentityResources.Profile())
                .UseResourceOwnerPasswordValidator(new SimpleResourceOwnerPasswordValidator())
                .UseProfileService(new SimpleProfileService())
                .CreateWebHostBuilder();

            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            var scopes = new[] {"api1", "offline_access", "openid", "profile"};

            var tokenResponse = await identityServerProxy.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
                new UserLoginConfiguration("user", "password"),
                scopes);

            // We are breaking the pattern arrange / act / assert here but we need to make sure token requested successfully first
            Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);


            var userInfoResponse = await identityServerProxy
                .GetUserInfoAsync(tokenResponse.AccessToken);

            Assert.NotNull(userInfoResponse);
            Assert.False(userInfoResponse.IsError);
            Assert.NotNull(userInfoResponse.Claims);

            var subjectClaim = userInfoResponse.Claims.First(claim => claim.Type == JwtClaimTypes.Subject);
            Assert.NotNull(subjectClaim);
            Assert.Equal("user", subjectClaim.Value);
        }

        [Fact]
        public async Task IdentityServerProxy_GetTokenAsync_Extension_Grant_Valid_User_Succeeds()
        {
            var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[]
                {
                    "api1", IdentityServerConstants.StandardScopes.OfflineAccess,
                    IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile
                },
                AllowedGrantTypes = new[] {"Custom"},
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200,
                AllowOfflineAccess = true
            };

            var webHostBuilder = new IdentityServerWebHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource("api1", "api1name"))
                .AddIdentityResources(new IdentityResources.OpenId(), new IdentityResources.Profile())
                .UseServices((context, collection) =>
                    collection.AddScoped<IExtensionGrantValidator, ExtensionsGrantValidator>())
                .CreateWebHostBuilder();

            var identityServerProxy = new IdentityServerProxy(webHostBuilder);

            var scopes = new[] {"api1", "offline_access", "openid", "profile"};

            var tokenResponse = await identityServerProxy.GetTokenAsync(clientConfiguration, "Custom",
                new Dictionary<string, string>
                {
                    {"scope", string.Join(" ", scopes)},
                    {"username", "user"},
                    {"password", "password"},
                });

            Assert.NotNull(tokenResponse);
            Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
            Assert.Equal(7200, tokenResponse.ExpiresIn);
            Assert.NotNull(tokenResponse.AccessToken);
            Assert.NotNull(tokenResponse.RefreshToken);
        }

        [Fact]
        public async Task IdentityServerProxy_GetResourceOwnerTokenAsync_Custom_WebHost_Succeeds()
        {
            var webHostBuilder = new IdentityServerWebHostBuilder()
                .UseWebHostBuilder(Program.CreateWebHostBuilder(new string[] { }))
                .CreateWebHostBuilder();

            var proxy = new IdentityServerProxy(webHostBuilder);

            var scopes = new[] {"api1", "offline_access", "openid", "profile"};

            var tokenResponse = await proxy.GetResourceOwnerPasswordAccessTokenAsync(
                new ClientConfiguration(Clients.Id, Clients.Secret),
                new UserLoginConfiguration("user1", "password1"), scopes);

            Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
        }
    }
}