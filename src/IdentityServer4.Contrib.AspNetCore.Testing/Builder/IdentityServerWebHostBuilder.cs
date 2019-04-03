using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Contrib.AspNetCore.Testing.Misc;
using IdentityServer4.Contrib.AspNetCore.Testing.Sinks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Builder
{
    public class IdentityServerWebHostBuilder
    {
        private readonly List<ApiResource> internalApiResources;
        private readonly List<Client> internalClients;
        private readonly List<IdentityResource> internalIdentityResources;

        private IWebHostBuilder internalWebHostBuilder;

        private Action<WebHostBuilderContext, IConfigurationBuilder> internalConfigurationBuilder;
        private Action<IApplicationBuilder> internalApplicationBuilder;
        private Action<WebHostBuilderContext, IServiceCollection> internalServicesBuilder;
        private Action<WebHostBuilderContext, ILoggingBuilder> internalLoggingBuilder;
        private IResourceOwnerPasswordValidator internalResourceOwnerPasswordValidator;
        private Type internalResourceOwnerPasswordValidatorType;
        private IProfileService internalProfileService;
        private Type internalProfileServiceType;

        public IdentityServerWebHostBuilder()
        {
            this.internalApiResources = new List<ApiResource>();
            this.internalClients = new List<Client>();
            this.internalIdentityResources = new List<IdentityResource>();

            this.internalConfigurationBuilder = (context, configurationBuilder) => { };
            this.internalApplicationBuilder = app => { };
            this.internalServicesBuilder = (builder, services) => { };
            this.internalLoggingBuilder = (context, loggingBuilder) =>
                loggingBuilder.AddProvider(new DefaultLoggerProvider());
        }

        public IdentityServerWebHostBuilder AddApiResources(params ApiResource[] apiResources)
        {
            if (!apiResources?.Any() ?? true)
            {
                throw new ArgumentException("ApiResources must not be null or empty", nameof(apiResources));
            }

            this.internalApiResources.AddRange(apiResources);

            return this;
        }

        public IdentityServerWebHostBuilder AddClients(params Client[] clients)
        {
            if (!clients?.Any() ?? true)
            {
                throw new ArgumentException("Clients must not be null or empty", nameof(clients));
            }

            this.internalClients.AddRange(clients);

            return this;
        }

        public IdentityServerWebHostBuilder AddIdentityResources(params IdentityResource[] identityResources)
        {
            if (!identityResources?.Any() ?? true)
            {
                throw new ArgumentException("Clients must not be null or empty", nameof(identityResources));
            }

            this.internalIdentityResources.AddRange(identityResources);

            return this;
        }

        public IdentityServerWebHostBuilder UseApplicationBuilder(Action<IApplicationBuilder> applicationBuilder)
        {
            this.internalApplicationBuilder = applicationBuilder;

            return this;
        }

        public IdentityServerWebHostBuilder UseConfigurationBuilder(
            Action<WebHostBuilderContext, IConfigurationBuilder> configurationBuilder)
        {
            this.internalConfigurationBuilder = configurationBuilder;

            return this;
        }

        public IdentityServerWebHostBuilder UseServices(
            Action<WebHostBuilderContext, IServiceCollection> servicesBuilder)
        {
            this.internalServicesBuilder = servicesBuilder;

            return this;
        }

        public IdentityServerWebHostBuilder UseLoggingBuilder(
            Action<WebHostBuilderContext, ILoggingBuilder> loggingBuilder)
        {
            this.internalLoggingBuilder = loggingBuilder ??
                                          throw new ArgumentNullException(nameof(loggingBuilder),
                                              "loggingBuilder must not be null");

            return this;
        }

        public IdentityServerWebHostBuilder UseResourceOwnerPasswordValidator(Type type)
        {
            if (!typeof(IResourceOwnerPasswordValidator).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type must be assignable to {nameof(IResourceOwnerPasswordValidator)}",
                    nameof(type));
            }

            this.internalResourceOwnerPasswordValidatorType = type;

            return this;
        }

        public IdentityServerWebHostBuilder UseResourceOwnerPasswordValidator<TResourceOwnerPasswordValidator>(
            TResourceOwnerPasswordValidator resourceOwnerPasswordValidator)
            where TResourceOwnerPasswordValidator : class, IResourceOwnerPasswordValidator
        {
            this.internalResourceOwnerPasswordValidator = resourceOwnerPasswordValidator;
            return this;
        }

        public IdentityServerWebHostBuilder UseProfileService(Type type)
        {
            if (!typeof(IProfileService).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type must be assignable to {nameof(IProfileService)}",
                    nameof(type));
            }

            this.internalProfileServiceType = type;

            return this;
        }

        public IdentityServerWebHostBuilder UseProfileService<TProfileService>(
            TProfileService profileService) where TProfileService : class, IProfileService
        {
            this.internalProfileService = profileService;
            return this;
        }

        public IdentityServerWebHostBuilder UseWebHostBuilder(IWebHostBuilder webHostBuilder)
        {
            this.internalWebHostBuilder = webHostBuilder ?? throw new ArgumentNullException(nameof(webHostBuilder));

            return this;
        }

        public IWebHostBuilder CreateWebHostBuilder()
        {
            if (this.internalWebHostBuilder != null)
            {
                return this.internalWebHostBuilder;
            }

            return new WebHostBuilder()
                .ConfigureLogging(this.internalLoggingBuilder)
                .Configure(builder =>
                {
                    builder.UseIdentityServer();
                    this.internalApplicationBuilder(builder);
                })
                .ConfigureServices((context, services) =>
                {
                    this.internalServicesBuilder(context, services);

                    services.AddSingleton<IEventSink>(new EventCaptureSink(new IdentityServerEventCaptureStore()));

                    if (this.internalResourceOwnerPasswordValidator != null)
                    {
                        services.AddSingleton(sp => this.internalResourceOwnerPasswordValidator);
                    }

                    if (this.internalResourceOwnerPasswordValidatorType != null)
                    {
                        services.AddSingleton(typeof(IResourceOwnerPasswordValidator),
                            this.internalResourceOwnerPasswordValidatorType);
                    }

                    if (this.internalProfileService != null)
                    {
                        services.AddSingleton(sp => this.internalProfileService);
                    }

                    if (this.internalProfileServiceType != null)
                    {
                        services.AddSingleton(typeof(IProfileService),
                            this.internalProfileServiceType);
                    }

                    var identityServerConfig = services
                        .AddIdentityServer(options =>
                        {
                            options.Events.RaiseInformationEvents = true;
                            options.Events.RaiseSuccessEvents = true;
                            options.Events.RaiseFailureEvents = true;
                            options.Events.RaiseErrorEvents = true;
                        })
                        .AddDefaultEndpoints()
                        .AddDefaultSecretParsers()
                        .AddDeveloperSigningCredential();

                    if (this.internalClients.Any())
                    {
                        identityServerConfig.AddInMemoryClients(this.internalClients);
                    }

                    if (this.internalApiResources.Any())
                    {
                        identityServerConfig.AddInMemoryApiResources(this.internalApiResources);
                    }

                    if (this.internalIdentityResources.Any())
                    {
                        identityServerConfig.AddInMemoryIdentityResources(this.internalIdentityResources);
                    }
                })
                .ConfigureAppConfiguration(this.internalConfigurationBuilder);
        }
    }
}