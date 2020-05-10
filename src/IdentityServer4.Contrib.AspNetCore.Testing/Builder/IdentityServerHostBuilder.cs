using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Configuration;
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
    public class IdentityServerHostBuilder
    {
        private readonly List<ApiResource> internalApiResources;
        private readonly List<ApiScope> internalApiScopes;
        private readonly List<Client> internalClients;
        private readonly List<IdentityResource> internalIdentityResources;

        private Action<IApplicationBuilder> internalApplicationBuilder;
        private Action<WebHostBuilderContext, IConfigurationBuilder> internalConfigurationBuilder;
        private IWebHostBuilder internalHostBuilder;
        private Func<IServiceCollection, IIdentityServerBuilder> internalIdentityServerBuilder;
        private Action<IdentityServerOptions> internalIdentityServerOptionsBuilder;
        private Action<WebHostBuilderContext, ILoggingBuilder> internalLoggingBuilder;
        private IProfileService internalProfileService;
        private Type internalProfileServiceType;
        private IResourceOwnerPasswordValidator internalResourceOwnerPasswordValidator;
        private Type internalResourceOwnerPasswordValidatorType;
        private Action<WebHostBuilderContext, IServiceCollection> internalServicesBuilder;

        public IdentityServerHostBuilder()
        {
            this.internalApiResources = new List<ApiResource>();

            this.internalApiScopes = new List<ApiScope>();

            this.internalClients = new List<Client>();

            this.internalIdentityResources = new List<IdentityResource>();

            this.internalConfigurationBuilder = (context, configurationBuilder) => { };

            this.internalApplicationBuilder = app => { };

            this.internalServicesBuilder = (builder, services) => { };

            this.internalLoggingBuilder = (context, loggingBuilder) =>
                loggingBuilder.AddProvider(new DefaultLoggerProvider());

            this.internalIdentityServerOptionsBuilder = options =>
            {
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;
            };
        }

        public IdentityServerHostBuilder AddApiResources(params ApiResource[] apiResources)
        {
            if (!apiResources?.Any() ?? true)
                throw new ArgumentException("ApiResources must not be null or empty", nameof(apiResources));

            this.internalApiResources.AddRange(apiResources);

            return this;
        }

        public IdentityServerHostBuilder AddApiScopes(params ApiScope[] apiScopes)
        {
            if (!apiScopes?.Any() ?? true)
                throw new ArgumentException("ApiScopes must not be null or empty", nameof(apiScopes));

            this.internalApiScopes.AddRange(apiScopes);

            return this;
        }

        public IdentityServerHostBuilder AddClients(params Client[] clients)
        {
            if (!clients?.Any() ?? true)
                throw new ArgumentException("Clients must not be null or empty", nameof(clients));

            this.internalClients.AddRange(clients);

            return this;
        }

        public IdentityServerHostBuilder AddIdentityResources(params IdentityResource[] identityResources)
        {
            if (!identityResources?.Any() ?? true)
                throw new ArgumentException("Clients must not be null or empty", nameof(identityResources));

            this.internalIdentityResources.AddRange(identityResources);

            return this;
        }

        public IdentityServerHostBuilder UseApplicationBuilder(Action<IApplicationBuilder> applicationBuilder)
        {
            this.internalApplicationBuilder =
                applicationBuilder ?? throw new ArgumentNullException(nameof(applicationBuilder));

            return this;
        }

        public IdentityServerHostBuilder UseConfigurationBuilder(
            Action<WebHostBuilderContext, IConfigurationBuilder> configurationBuilder)
        {
            this.internalConfigurationBuilder = configurationBuilder;

            return this;
        }

        public IdentityServerHostBuilder UseServices(
            Action<WebHostBuilderContext, IServiceCollection> servicesBuilder)
        {
            this.internalServicesBuilder = servicesBuilder;

            return this;
        }

        public IdentityServerHostBuilder UseLoggingBuilder(
            Action<WebHostBuilderContext, ILoggingBuilder> loggingBuilder)
        {
            this.internalLoggingBuilder = loggingBuilder ??
                                          throw new ArgumentNullException(nameof(loggingBuilder),
                                              "loggingBuilder must not be null");

            return this;
        }

        public IdentityServerHostBuilder UseResourceOwnerPasswordValidator(Type type)
        {
            if (!typeof(IResourceOwnerPasswordValidator).IsAssignableFrom(type))
                throw new ArgumentException($"Type must be assignable to {nameof(IResourceOwnerPasswordValidator)}",
                    nameof(type));

            this.internalResourceOwnerPasswordValidatorType = type;

            return this;
        }

        public IdentityServerHostBuilder UseResourceOwnerPasswordValidator<TResourceOwnerPasswordValidator>(
            TResourceOwnerPasswordValidator resourceOwnerPasswordValidator)
            where TResourceOwnerPasswordValidator : class, IResourceOwnerPasswordValidator
        {
            this.internalResourceOwnerPasswordValidator = resourceOwnerPasswordValidator;
            return this;
        }

        public IdentityServerHostBuilder UseProfileService(Type type)
        {
            if (!typeof(IProfileService).IsAssignableFrom(type))
                throw new ArgumentException($"Type must be assignable to {nameof(IProfileService)}",
                    nameof(type));

            this.internalProfileServiceType = type;

            return this;
        }

        public IdentityServerHostBuilder UseProfileService<TProfileService>(
            TProfileService profileService) where TProfileService : class, IProfileService
        {
            this.internalProfileService = profileService;
            return this;
        }

        public IdentityServerHostBuilder UseWebHostBuilder(IWebHostBuilder webHostBuilder)
        {
            this.internalHostBuilder = webHostBuilder ?? throw new ArgumentNullException(nameof(webHostBuilder));

            return this;
        }

        public IdentityServerHostBuilder UseIdentityServerOptionsBuilder(
            Action<IdentityServerOptions> identityServerOptionsBuilder)
        {
            this.internalIdentityServerOptionsBuilder = identityServerOptionsBuilder ??
                                                        throw new ArgumentNullException(
                                                            nameof(identityServerOptionsBuilder),
                                                            $"{nameof(identityServerOptionsBuilder)} must not be null!");
            return this;
        }

        public IdentityServerHostBuilder UseIdentityServerBuilder(
            Func<IServiceCollection, IIdentityServerBuilder> identityServerBuilder)
        {
            this.internalIdentityServerBuilder = identityServerBuilder ??
                                                 throw new ArgumentNullException(nameof(identityServerBuilder),
                                                     $"{nameof(identityServerBuilder)} must not be null!");

            return this;
        }

        public IWebHostBuilder CreateWebHostBuider()
        {
            if (this.internalHostBuilder != null) return this.internalHostBuilder;

            this.Validate();

            return new WebHostBuilder()
                .Configure(builder =>
                {
                    builder.UseIdentityServer();
                    this.internalApplicationBuilder(builder);
                })
                .ConfigureServices((context, services) =>
                {
                    this.internalServicesBuilder(context, services);

                    this.ConfigureIdentityServerServices(services);

                    var identityServerBuilder = this.GetIdentityServerBuilder(services);

                    this.ConfigureIdentityServerResources(identityServerBuilder);
                })
                .ConfigureAppConfiguration(this.internalConfigurationBuilder);
        }

        private void Validate()
        {
            if (this.internalApiResources.Any() && this.internalApiResources.Count != this.internalApiScopes.Count)
                throw new InvalidOperationException(
                    "IdentityServer4 version 4 requires API scopes for each API resource!");
        }

        private void ConfigureIdentityServerServices(IServiceCollection services)
        {
            services.AddSingleton<IEventSink>(new EventCaptureSink(new IdentityServerEventCaptureStore()));

            if (this.internalResourceOwnerPasswordValidator != null)
                services.AddSingleton(sp => this.internalResourceOwnerPasswordValidator);

            if (this.internalResourceOwnerPasswordValidatorType != null)
                services.AddSingleton(typeof(IResourceOwnerPasswordValidator),
                    this.internalResourceOwnerPasswordValidatorType);

            if (this.internalProfileService != null) services.AddSingleton(sp => this.internalProfileService);

            if (this.internalProfileServiceType != null)
                services.AddSingleton(typeof(IProfileService), this.internalProfileServiceType);
        }

        private IIdentityServerBuilder GetIdentityServerBuilder(IServiceCollection services)
        {
            if (this.internalIdentityServerBuilder != null) return this.internalIdentityServerBuilder(services);

            return services.AddIdentityServer(this.internalIdentityServerOptionsBuilder)
                .AddDefaultEndpoints()
                .AddDefaultSecretParsers()
                .AddDeveloperSigningCredential();
        }

        private void ConfigureIdentityServerResources(IIdentityServerBuilder identityServerBuilder)
        {
            if (this.internalClients.Any()) identityServerBuilder.AddInMemoryClients(this.internalClients);

            if (this.internalApiResources.Any())
                identityServerBuilder.AddInMemoryApiResources(this.internalApiResources);

            if (this.internalApiScopes.Any())
                identityServerBuilder.AddInMemoryApiScopes(this.internalApiScopes);

            if (this.internalIdentityResources.Any())
                identityServerBuilder.AddInMemoryIdentityResources(this.internalIdentityResources);
        }
    }
}