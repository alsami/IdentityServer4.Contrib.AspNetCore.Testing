using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autofac.Extensions.DependencyInjection;
using IdentityServer4.Contrib.AspNetCore.Testing.Builder;
using IdentityServer4.Contrib.AspNetCore.Testing.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Testing.Infrastructure.Services;
using IdentityServer4.Testing.Infrastructure.Validators;
using IdentityServer4.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Xunit;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Tests
{
    public class IdentityServerTestHostBuilderTests
    {
        [Fact]
        public void CreateHostBuilder_UseProfileServiceTypedButOfWrongType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new IdentityServerTestHostBuilder().UseProfileService(typeof(ExtensionsGrantValidator)));
        }

        [Fact]
        public void CreateHostBuilder_UseProfileServiceTypes_Resolveable()
        {
            var (client, apiResource, apiScope) = CreateTestData();

            var builder = new IdentityServerTestHostBuilder()
                .UseProfileService(typeof(SimpleProfileService))
                .AddClients(client)
                .AddApiResources(apiResource)
                .AddApiScopes(apiScope)
                .CreateHostBuilder(new AutofacServiceProviderFactory(), ContainerBuilderConfiguration.ConfigureContainer);

            var host = builder.Start();
            host.Services.GetRequiredService<IProfileService>();
        }

        [Fact]
        public void CreateHostBuilder_UseResourceOwnerPasswordValidatorTypedButOfWrongType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new IdentityServerTestHostBuilder()
                .UseResourceOwnerPasswordValidator(typeof(ExtensionsGrantValidator)));
        }

        [Fact]
        public void CreateHostBuilder_UseResourceOwnerPasswordValidatorTyped_Resolveable()
        {
            var (client, apiResource, apiScope) = CreateTestData();

            var builder = new IdentityServerTestHostBuilder()
                .UseResourceOwnerPasswordValidator(typeof(SimpleResourceOwnerPasswordValidator))
                .AddClients(client)
                .AddApiResources(apiResource)
                .AddApiScopes(apiScope)
                .CreateHostBuilder(new AutofacServiceProviderFactory(), ContainerBuilderConfiguration.ConfigureContainer);

            var host = builder.Start();
            host.Services.GetRequiredService<IResourceOwnerPasswordValidator>();
        }


        [Fact]
        public void CreateHostBuilder_UseResourceOwnerPasswordValidator_Resolveable()
        {
            InitializeSerilog();

            var (client, apiResource, apiScope) = CreateTestData();

            var builder = new IdentityServerTestHostBuilder()
                .UseLoggingBuilder((context, loggingBuilder) => loggingBuilder.AddSerilog())
                .UseResourceOwnerPasswordValidator(typeof(ResourceOwnerValidatorWithDependencies))
                .AddClients(client)
                .AddApiResources(apiResource)
                .AddApiScopes(apiScope)
                .CreateHostBuilder(new AutofacServiceProviderFactory(), ContainerBuilderConfiguration.ConfigureContainer);

            var host = builder.Start();
            host.Services.GetRequiredService<IResourceOwnerPasswordValidator>();
        }

        [Fact]
        public void CreateHostBuilder_ServiceRegisteredWithAutofacContainer_ServiceResolveable()
        {
            var (client, apiResource, apiScope) = CreateTestData();

            var builder = new IdentityServerTestHostBuilder()
                .AddClients(client)
                .AddApiResources(apiResource)
                .AddApiScopes(apiScope)
                .CreateHostBuilder(new AutofacServiceProviderFactory(), ContainerBuilderConfiguration.ConfigureContainer);

            var host = builder.Start();

            host.Services.GetRequiredService<Dependency>();
        }

        [Fact]
        public void CreateHostBuilder_ValidateScopes()
        {
            var apiResource = new ApiResource("res1");
            apiResource.Scopes = new List<string> { "scope1", "scope3" };
            // Bad scope specs
            Assert.Throws<InvalidOperationException>(() => new IdentityServerTestHostBuilder()
                .AddApiResources(apiResource)
                .AddApiScopes(new ApiScope("scope3"))
                .CreateHostBuilder());
            Assert.Throws<InvalidOperationException>(() => new IdentityServerTestHostBuilder()
                .AddApiResources(apiResource)
                .AddApiScopes(new ApiScope("scope1"), new ApiScope("scope2"))
                .CreateHostBuilder());
            Assert.Throws<InvalidOperationException>(() => new IdentityServerTestHostBuilder()
                .AddApiResources(apiResource)
                .AddApiScopes(new ApiScope("scope2"), new ApiScope("scope3"))
                .CreateHostBuilder());
            // Good scope specs
            new IdentityServerTestHostBuilder()
                .AddApiResources(apiResource)
                .AddApiScopes(new ApiScope("scope1"), new ApiScope("scope2"), new ApiScope("scope3"))
                .CreateHostBuilder();
            new IdentityServerTestHostBuilder()
                .AddApiResources(apiResource)
                .AddApiScopes(new ApiScope("scope1"), new ApiScope("scope3"))
                .CreateHostBuilder();
        }

        private static (Client client, ApiResource apiResource, ApiScope apiScope) CreateTestData()
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

            return (client, new ApiResource("api1", "api1name"), new ApiScope("api1"));
        }

        private static void InitializeSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine(AppContext.BaseDirectory, "Logs",
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.log"))
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                    theme: AnsiConsoleTheme.Literate, restrictedToMinimumLevel: LogEventLevel.Error)
                .CreateLogger();
        }
    }
}