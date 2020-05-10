using System;
using IdentityServer4.Contrib.AspNetCore.Testing.Misc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Builder
{
    public class IdentityServerTestWebHostBuilder : AbstractIdentityServerHostBuilder<IdentityServerTestWebHostBuilder>
    {
        private IWebHostBuilder internalHostBuilder;

        private Action<WebHostBuilderContext, ILoggingBuilder> internalLoggingBuilder;
        private Action<WebHostBuilderContext, IConfigurationBuilder> internalConfigurationBuilder;
        private Action<WebHostBuilderContext, IServiceCollection> internalServicesBuilder;

        public IdentityServerTestWebHostBuilder()
        {
            this.internalConfigurationBuilder = (context, configurationBuilder) => { };

            this.internalServicesBuilder = (builder, services) => { };

            this.internalLoggingBuilder = (context, loggingBuilder) =>
                loggingBuilder.AddProvider(new DefaultLoggerProvider());
        }

        public IdentityServerTestWebHostBuilder UseConfigurationBuilder(
            Action<WebHostBuilderContext, IConfigurationBuilder> configurationBuilder)
        {
            this.internalConfigurationBuilder = configurationBuilder;

            return this;
        }

        public IdentityServerTestWebHostBuilder UseServices(
            Action<WebHostBuilderContext, IServiceCollection> servicesBuilder)
        {
            this.internalServicesBuilder = servicesBuilder;

            return this;
        }

        public IdentityServerTestWebHostBuilder UseLoggingBuilder(
            Action<WebHostBuilderContext, ILoggingBuilder> loggingBuilder)
        {
            this.internalLoggingBuilder = loggingBuilder ??
                                          throw new ArgumentNullException(nameof(loggingBuilder),
                                              "loggingBuilder must not be null");

            return this;
        }

        public IdentityServerTestWebHostBuilder UseWebHostBuilder(IWebHostBuilder webHostBuilder)
        {
            this.internalHostBuilder = webHostBuilder ?? throw new ArgumentNullException(nameof(webHostBuilder));

            return this;
        }

        public IWebHostBuilder CreateWebHostBuider()
        {
            if (this.internalHostBuilder != null) return this.internalHostBuilder;

            this.Validate();

            return new WebHostBuilder()
                .ConfigureLogging(this.internalLoggingBuilder)
                .Configure(builder =>
                {
                    builder.UseIdentityServer();
                    this.InternalApplicationBuilder(builder);
                })
                .ConfigureServices((context, services) =>
                {
                    this.internalServicesBuilder(context, services);

                    this.ConfigureIdentityServerServices(services);

                    var identityServerBuilder = this.CreatePreconfiguredIdentityServerBuilder(services);

                    this.ConfigureIdentityServerResources(identityServerBuilder);
                })
                .ConfigureAppConfiguration(this.internalConfigurationBuilder);
        }
    }
}