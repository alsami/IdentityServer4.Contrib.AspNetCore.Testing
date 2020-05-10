using System;
using System.IO;
using System.Reflection;
using IdentityServer4.Contrib.AspNetCore.Testing.Builder;
using IdentityServer4.Services;
using IdentityServer4.Testing.Infrastructure.Services;
using IdentityServer4.Testing.Infrastructure.Validators;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Xunit;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Tests
{
    public class IdentityServerWebHostBuilderTests
    {
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

        [Fact]
        public void IdentityServerWebHostBuilder_UseConfigurationBuilder_HasSettings()
        {
            var webHost = new IdentityServerTestWebHostBuilder()
                .UseConfigurationBuilder((context, builder) =>
                {
                    context.HostingEnvironment.WebRootPath = AppContext.BaseDirectory;
                    builder.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "testappsettings.json"), false);
                })
                .CreateWebHostBuider()
                .Build();

            var configuration = webHost.Services.GetRequiredService<IConfiguration>();
            var hostingEnvironment =
                webHost.Services.GetRequiredService<IWebHostEnvironment>();

            Assert.Equal("PropValue", configuration["Prop"]);
            Assert.Equal(AppContext.BaseDirectory, hostingEnvironment.ContentRootPath);
        }

        [Fact]
        public void IdentityServerWebHostBuilder_UseLoggingBuilder_Serilog_Expect_Logger_And_Provider()
        {
            InitializeSerilog();

            var webHost = new IdentityServerTestWebHostBuilder()
                .UseLoggingBuilder((context, builder) => builder.AddSerilog())
                .CreateWebHostBuider()
                .UseContentRoot(AppContext.BaseDirectory)
                .Build();

            var logger = webHost
                .Services
                .GetRequiredService<ILogger<IdentityServerWebHostBuilderTests>>();

            var path = Path.Combine(AppContext.BaseDirectory, "Logs",
                $"{Assembly.GetExecutingAssembly().GetName().Name}-{DateTime.UtcNow:yyyyMMdd}.log");

            logger.LogError($"Logging to path {path} works!");
        }

        [Fact]
        public void IdentityServerWebHostBuilder_UseProfileService_Resolveable()
        {
            var webHost = new IdentityServerTestWebHostBuilder()
                .UseProfileService(new SimpleProfileService())
                .CreateWebHostBuider()
                .Build();

            webHost.Services.GetRequiredService<IProfileService>();
        }

        [Fact]
        public void IdentityServerWebHostBuilder_UseProfileService_Typed_Invalid_Throws()
        {
            Assert.Throws<ArgumentException>(() => new IdentityServerTestWebHostBuilder()
                .UseProfileService(typeof(ExtensionsGrantValidator)));
        }

        [Fact]
        public void IdentityServerWebHostBuilder_UseProfileService_Typed_Resolveable()
        {
            var webHost = new IdentityServerTestWebHostBuilder()
                .UseProfileService(typeof(SimpleProfileService))
                .CreateWebHostBuider()
                .Build();

            webHost.Services.GetRequiredService<IProfileService>();
        }

        [Fact]
        public void IdentityServerWebHostBuilder_UseResourceOwnerPasswordValidator_Resolveable()
        {
            var webHost = new IdentityServerTestWebHostBuilder()
                .UseResourceOwnerPasswordValidator(new SimpleResourceOwnerPasswordValidator())
                .CreateWebHostBuider()
                .Build();

            webHost.Services.GetRequiredService<IResourceOwnerPasswordValidator>();
        }

        [Fact]
        public void IdentityServerWebHostBuilder_UseResourceOwnerPasswordValidator_Typed_Invalid_Throws()
        {
            Assert.Throws<ArgumentException>(() => new IdentityServerTestWebHostBuilder()
                .UseResourceOwnerPasswordValidator(typeof(ExtensionsGrantValidator)));
        }

        [Fact]
        public void IdentityServerWebHostBuilder_UseResourceOwnerPasswordValidator_Typed_Resolveable()
        {
            var webHost = new IdentityServerTestWebHostBuilder()
                .UseResourceOwnerPasswordValidator(typeof(SimpleResourceOwnerPasswordValidator))
                .CreateWebHostBuider()
                .Build();

            webHost.Services.GetRequiredService<IResourceOwnerPasswordValidator>();
        }

        [Fact]
        public void IdentityServerWebHostBuilder_UseResourceOwnerPasswordValidator_With_Dependencies_Resolveable()
        {
            InitializeSerilog();

            var webHost = new IdentityServerTestWebHostBuilder()
                .UseLoggingBuilder((context, builder) => builder.AddSerilog())
                .UseResourceOwnerPasswordValidator(typeof(ResourceOwnerValidatorWithDependencies))
                .CreateWebHostBuider()
                .Build();

            webHost.Services.GetRequiredService<IResourceOwnerPasswordValidator>();
        }
    }
}