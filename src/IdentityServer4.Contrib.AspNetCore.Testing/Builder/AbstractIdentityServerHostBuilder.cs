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
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Builder
{
    public abstract class AbstractIdentityServerHostBuilder<TBuilder>
        where TBuilder : AbstractIdentityServerHostBuilder<TBuilder>
    {
        private readonly List<ApiResource> internalApiResources;
        private readonly List<ApiScope> internalApiScopes;
        private readonly List<Client> internalClients;
        private readonly List<IdentityResource> internalIdentityResources;

        private Func<IServiceCollection, IIdentityServerBuilder> internalIdentityServerBuilder;
        private Action<IdentityServerOptions> internalIdentityServerOptionsBuilder;
        private IProfileService internalProfileService;
        private Type internalProfileServiceType;
        private IResourceOwnerPasswordValidator internalResourceOwnerPasswordValidator;
        private Type internalResourceOwnerPasswordValidatorType;

        protected Action<IApplicationBuilder> InternalApplicationBuilder;


        protected AbstractIdentityServerHostBuilder()
        {
            this.InternalApplicationBuilder = builder => { };

            this.internalApiResources = new List<ApiResource>();

            this.internalApiScopes = new List<ApiScope>();

            this.internalClients = new List<Client>();

            this.internalIdentityResources = new List<IdentityResource>();

            this.internalIdentityServerOptionsBuilder = options =>
            {
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;
            };
        }

        public TBuilder AddApiResources(params ApiResource[] apiResources)
        {
            if (!apiResources?.Any() ?? true)
                throw new ArgumentException("ApiResources must not be null or empty", nameof(apiResources));

            this.internalApiResources.AddRange(apiResources);

            return (TBuilder) this;
        }

        public TBuilder AddApiScopes(params ApiScope[] apiScopes)
        {
            if (!apiScopes?.Any() ?? true)
                throw new ArgumentException("ApiScopes must not be null or empty", nameof(apiScopes));

            this.internalApiScopes.AddRange(apiScopes);

            return (TBuilder) this;
        }

        public TBuilder AddClients(params Client[] clients)
        {
            if (!clients?.Any() ?? true)
                throw new ArgumentException("Clients must not be null or empty", nameof(clients));

            this.internalClients.AddRange(clients);

            return (TBuilder) this;
        }

        public TBuilder AddIdentityResources(params IdentityResource[] identityResources)
        {
            if (!identityResources?.Any() ?? true)
                throw new ArgumentException("Clients must not be null or empty", nameof(identityResources));

            this.internalIdentityResources.AddRange(identityResources);

            return (TBuilder) this;
        }

        // ReSharper disable once UnusedMember.Global
        public TBuilder UseApplicationBuilder(Action<IApplicationBuilder> applicationBuilder)
        {
            this.InternalApplicationBuilder =
                applicationBuilder ?? throw new ArgumentNullException(nameof(applicationBuilder));

            return (TBuilder) this;
        }

        public TBuilder UseResourceOwnerPasswordValidator(Type type)
        {
            if (!typeof(IResourceOwnerPasswordValidator).IsAssignableFrom(type))
                throw new ArgumentException($"Type must be assignable to {nameof(IResourceOwnerPasswordValidator)}",
                    nameof(type));

            this.internalResourceOwnerPasswordValidatorType = type;

            return (TBuilder) this;
        }

        public TBuilder UseResourceOwnerPasswordValidator<TResourceOwnerPasswordValidator>(
            TResourceOwnerPasswordValidator resourceOwnerPasswordValidator)
            where TResourceOwnerPasswordValidator : class, IResourceOwnerPasswordValidator
        {
            this.internalResourceOwnerPasswordValidator = resourceOwnerPasswordValidator;

            return (TBuilder) this;
        }

        public TBuilder UseProfileService(Type type)
        {
            if (!typeof(IProfileService).IsAssignableFrom(type))
                throw new ArgumentException($"Type must be assignable to {nameof(IProfileService)}",
                    nameof(type));

            this.internalProfileServiceType = type;

            return (TBuilder) this;
        }

        public TBuilder UseProfileService<TProfileService>(
            TProfileService profileService) where TProfileService : class, IProfileService
        {
            this.internalProfileService = profileService;

            return (TBuilder) this;
        }

        public TBuilder UseIdentityServerOptionsBuilder(
            Action<IdentityServerOptions> identityServerOptionsBuilder)
        {
            this.internalIdentityServerOptionsBuilder = identityServerOptionsBuilder ??
                                                        throw new ArgumentNullException(
                                                            nameof(identityServerOptionsBuilder),
                                                            $"{nameof(identityServerOptionsBuilder)} must not be null!");

            return (TBuilder) this;
        }

        public TBuilder UseIdentityServerBuilder(
            Func<IServiceCollection, IIdentityServerBuilder> identityServerBuilder)
        {
            this.internalIdentityServerBuilder = identityServerBuilder ??
                                                 throw new ArgumentNullException(nameof(identityServerBuilder),
                                                     $"{nameof(identityServerBuilder)} must not be null!");

            return (TBuilder) this;
        }

        protected void ConfigureIdentityServerServices(IServiceCollection services)
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

        protected IIdentityServerBuilder CreatePreconfiguredIdentityServerBuilder(IServiceCollection services)
        {
            if (this.internalIdentityServerBuilder != null) return this.internalIdentityServerBuilder(services);

            return services
                .AddIdentityServer(this.internalIdentityServerOptionsBuilder)
                .AddDefaultEndpoints()
                .AddDefaultSecretParsers()
                .AddDeveloperSigningCredential();
        }

        protected void ConfigureIdentityServerResources(IIdentityServerBuilder identityServerBuilder)
        {
            if (this.internalClients.Any()) identityServerBuilder.AddInMemoryClients(this.internalClients);

            if (this.internalApiResources.Any())
                identityServerBuilder.AddInMemoryApiResources(this.internalApiResources);

            if (this.internalApiScopes.Any())
                identityServerBuilder.AddInMemoryApiScopes(this.internalApiScopes);

            if (this.internalIdentityResources.Any())
                identityServerBuilder.AddInMemoryIdentityResources(this.internalIdentityResources);
        }

        protected virtual void Validate()
        {
            if (this.internalApiResources.Any() && this.internalApiResources.Count != this.internalApiScopes.Count)
                throw new InvalidOperationException(
                    "IdentityServer4 version 4 requires API scopes for each API resource!");
        }
    }
}