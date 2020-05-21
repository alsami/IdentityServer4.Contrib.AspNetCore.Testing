using Autofac;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Tests
{
    public static class ContainerBuilderConfiguration
    {
        public static void ConfigureContainer(ContainerBuilder containerBuilder) =>
            containerBuilder.RegisterType<Dependency>().AsSelf();
    }
}