using System.Net.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Api
{
    public class Startup
    {
        private readonly HttpMessageHandler identityServerMessageHandler;

        public Startup(HttpMessageHandler identityServerMessageHandler)
        {
            this.identityServerMessageHandler = identityServerMessageHandler;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Authority = "http://localhost";
                    options.JwtBackChannelHandler = this.identityServerMessageHandler;
                });
            services.AddMvc(options => options.EnableEndpointRouting = false);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}