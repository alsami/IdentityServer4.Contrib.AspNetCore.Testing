using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Services
{
    public class IdentityServerWebHostProxy : AbstractIdentityServerProxy
    {
        public IdentityServerWebHostProxy(IWebHostBuilder webHostBuilder)
        {
            if (webHostBuilder == null)
                throw new ArgumentNullException(nameof(webHostBuilder),
                    "webHostBuilder must not be null");

            this.IdentityServer = new TestServer(webHostBuilder);
        }

        // ReSharper disable once UnusedMember.Global
        public IdentityServerWebHostProxy(TestServer identityServer)
        {
            this.IdentityServer = identityServer;
        }

        public override TestServer IdentityServer { get; }
    }
}