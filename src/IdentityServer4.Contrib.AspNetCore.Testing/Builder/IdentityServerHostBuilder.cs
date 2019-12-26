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
            internalApiResources = new List<ApiResource>();

            internalClients = new List<Client>();

            internalIdentityResources = new List<IdentityResource>();

            internalConfigurationBuilder = (context, configurationBuilder) => { };

            internalApplicationBuilder = app => { };

            internalServicesBuilder = (builder, services) => { };

            internalLoggingBuilder = (context, loggingBuilder) =>
                loggingBuilder.AddProvider(new DefaultLoggerProvider());

            internalIdentityServerOptionsBuilder = options =>
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

            internalApiResources.AddRange(apiResources);

            return this;
        }

        public IdentityServerHostBuilder AddClients(params Client[] clients)
        {
            if (!clients?.Any() ?? true)
                throw new ArgumentException("Clients must not be null or empty", nameof(clients));

            internalClients.AddRange(clients);

            return this;
        }

        public IdentityServerHostBuilder AddIdentityResources(params IdentityResource[] identityResources)
        {
            if (!identityResources?.Any() ?? true)
                throw new ArgumentException("Clients must not be null or empty", nameof(identityResources));

            internalIdentityResources.AddRange(identityResources);

            return this;
        }

        public IdentityServerHostBuilder UseApplicationBuilder(Action<IApplicationBuilder> applicationBuilder)
        {
            internalApplicationBuilder = applicationBuilder;

            return this;
        }

        public IdentityServerHostBuilder UseConfigurationBuilder(
            Action<WebHostBuilderContext, IConfigurationBuilder> configurationBuilder)
        {
            internalConfigurationBuilder = configurationBuilder;

            return this;
        }

        public IdentityServerHostBuilder UseServices(
            Action<WebHostBuilderContext, IServiceCollection> servicesBuilder)
        {
            internalServicesBuilder = servicesBuilder;

            return this;
        }

        public IdentityServerHostBuilder UseLoggingBuilder(
            Action<WebHostBuilderContext, ILoggingBuilder> loggingBuilder)
        {
            internalLoggingBuilder = loggingBuilder ??
                                     throw new ArgumentNullException(nameof(loggingBuilder),
                                         "loggingBuilder must not be null");

            return this;
        }

        public IdentityServerHostBuilder UseResourceOwnerPasswordValidator(Type type)
        {
            if (!typeof(IResourceOwnerPasswordValidator).IsAssignableFrom(type))
                throw new ArgumentException($"Type must be assignable to {nameof(IResourceOwnerPasswordValidator)}",
                    nameof(type));

            internalResourceOwnerPasswordValidatorType = type;

            return this;
        }

        public IdentityServerHostBuilder UseResourceOwnerPasswordValidator<TResourceOwnerPasswordValidator>(
            TResourceOwnerPasswordValidator resourceOwnerPasswordValidator)
            where TResourceOwnerPasswordValidator : class, IResourceOwnerPasswordValidator
        {
            internalResourceOwnerPasswordValidator = resourceOwnerPasswordValidator;
            return this;
        }

        public IdentityServerHostBuilder UseProfileService(Type type)
        {
            if (!typeof(IProfileService).IsAssignableFrom(type))
                throw new ArgumentException($"Type must be assignable to {nameof(IProfileService)}",
                    nameof(type));

            internalProfileServiceType = type;

            return this;
        }

        public IdentityServerHostBuilder UseProfileService<TProfileService>(
            TProfileService profileService) where TProfileService : class, IProfileService
        {
            internalProfileService = profileService;
            return this;
        }

        public IdentityServerHostBuilder UseWebHostBuilder(IWebHostBuilder webHostBuilder)
        {
            internalHostBuilder = webHostBuilder ?? throw new ArgumentNullException(nameof(webHostBuilder));

            return this;
        }

        public IdentityServerHostBuilder UseIdentityServerOptionsBuilder(
            Action<IdentityServerOptions> identityServerOptionsBuilder)
        {
            internalIdentityServerOptionsBuilder = identityServerOptionsBuilder ??
                                                   throw new ArgumentNullException(
                                                       nameof(identityServerOptionsBuilder),
                                                       $"{nameof(identityServerOptionsBuilder)} must not be null!");
            return this;
        }

        public IdentityServerHostBuilder UseIdentityServerBuilder(
            Func<IServiceCollection, IIdentityServerBuilder> identityServerBuilder)
        {
            internalIdentityServerBuilder = identityServerBuilder ??
                                            throw new ArgumentNullException(nameof(identityServerBuilder),
                                                $"{nameof(identityServerBuilder)} must not be null!");

            return this;
        }

        public IWebHostBuilder CreateWebHostBuider()
        {
            if (internalHostBuilder != null) return internalHostBuilder;

            return new WebHostBuilder()
                .Configure(builder =>
                {
                    builder.UseIdentityServer();
                    internalApplicationBuilder(builder);
                })
                .ConfigureServices((context, services) =>
                {
                    internalServicesBuilder(context, services);

                    ConfigureIdentityServerServices(services);

                    var identityServerBuilder = GetIdentityServerBuilder(services);

                    ConfigureIdentityServerResources(identityServerBuilder);
                })
                .ConfigureAppConfiguration(internalConfigurationBuilder);
        }

        private void ConfigureIdentityServerServices(IServiceCollection services)
        {
            services.AddSingleton<IEventSink>(new EventCaptureSink(new IdentityServerEventCaptureStore()));

            if (internalResourceOwnerPasswordValidator != null)
                services.AddSingleton(sp => internalResourceOwnerPasswordValidator);

            if (internalResourceOwnerPasswordValidatorType != null)
                services.AddSingleton(typeof(IResourceOwnerPasswordValidator),
                    internalResourceOwnerPasswordValidatorType);

            if (internalProfileService != null) services.AddSingleton(sp => internalProfileService);

            if (internalProfileServiceType != null)
                services.AddSingleton(typeof(IProfileService),
                    internalProfileServiceType);
        }

        private IIdentityServerBuilder GetIdentityServerBuilder(IServiceCollection services)
        {
            if (internalIdentityServerBuilder != null) return internalIdentityServerBuilder(services);

            return services.AddIdentityServer(internalIdentityServerOptionsBuilder)
                .AddDefaultEndpoints()
                .AddDefaultSecretParsers()
                .AddDeveloperSigningCredential();
        }

        private void ConfigureIdentityServerResources(IIdentityServerBuilder identityServerBuilder)
        {
            if (internalClients.Any()) identityServerBuilder.AddInMemoryClients(internalClients);

            if (internalApiResources.Any()) identityServerBuilder.AddInMemoryApiResources(internalApiResources);

            if (!internalIdentityResources.Any()) return;

            identityServerBuilder.AddInMemoryIdentityResources(internalIdentityResources);
        }
    }
}