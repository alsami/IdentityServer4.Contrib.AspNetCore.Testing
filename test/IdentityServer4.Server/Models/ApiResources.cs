using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Server.Models
{
    public static class ApiResources
    {
        public static IEnumerable<ApiResource> GetApiResources
            => new List<ApiResource>
            {
                new ApiResource("api1", "api1")
            };
    }
}