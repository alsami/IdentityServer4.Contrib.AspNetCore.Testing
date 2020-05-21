using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Builder
{
    public sealed class IdentityServerTestHostCustomContainerBuilder<TContainer> : IdentityServerTestHostBuilder
    {
        private IServiceProviderFactory<TContainer> internalServiceProviderFactory;
        private Action<TContainer> internalContainerBuilderConfigurationAction;

        public IdentityServerTestHostBuilder UseServiceProviderFactory(
            IServiceProviderFactory<TContainer> serviceProviderFactory,
            Action<TContainer> containerBuilderConfigurationAction)
        {
            this.internalServiceProviderFactory = serviceProviderFactory ??
                                                  throw new ArgumentNullException(nameof(serviceProviderFactory));

            this.internalContainerBuilderConfigurationAction =
                containerBuilderConfigurationAction ??
                throw new ArgumentNullException(nameof(containerBuilderConfigurationAction));

            return this;
        }

        public override IHostBuilder CreateHostBuilder()
        {
            return base.CreateHostBuilder()
                .UseServiceProviderFactory(this.internalServiceProviderFactory)
                .ConfigureContainer(this.internalContainerBuilderConfigurationAction);
        }

        protected override void Validate()
        {
            if (this.internalServiceProviderFactory is null)
                throw new InvalidOperationException(
                    $"Must provide a {nameof(IServiceProviderFactory<TContainer>)} in order to use a custom container!!");

            if (this.internalContainerBuilderConfigurationAction is null)
                throw new InvalidOperationException(
                    $"Must provide an {nameof(Action<TContainer>)} in order to setup the custom container!");

            base.Validate();
        }
    }
}