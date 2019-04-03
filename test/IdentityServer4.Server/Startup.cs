using System.Threading.Tasks;
using IdentityServer4.Server.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseErrorEvents = true;
                })
                .AddDefaultEndpoints()
                .AddDefaultSecretParsers()
                .AddDeveloperSigningCredential()
                .AddInMemoryCaching()
                .AddTestUsers(TestUsers.GeTestUsers())
                .AddInMemoryClients(Clients.GetClients)
                .AddInMemoryApiResources(ApiResources.GetApiResources)
                .AddInMemoryIdentityResources(IdentityResources.GetIdentityResources);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIdentityServer();

            app.Run(ctx =>
            {
                var logger = ctx.RequestServices.GetRequiredService<ILogger<Startup>>();
                logger.LogInformation("Logging!");
                return Task.CompletedTask;
            });
        }
    }
}