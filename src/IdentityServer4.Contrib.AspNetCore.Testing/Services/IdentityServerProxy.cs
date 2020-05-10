using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Services
{
    [Obsolete("Please use IdentityServerWebHostProxy. IdentityServerProxy will be removed with version 5")]
    // ReSharper disable once UnusedType.Global
    public sealed class IdentityServerProxy : IdentityServerWebHostProxy
    {
        public IdentityServerProxy(IWebHostBuilder webHostBuilder) : base(webHostBuilder)
        {
        }

        public IdentityServerProxy(TestServer identityServer) : base(identityServer)
        {
        }
    }
}