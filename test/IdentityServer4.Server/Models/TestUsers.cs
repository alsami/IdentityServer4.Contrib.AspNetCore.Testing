using System.Collections.Generic;
using IdentityServer4.Test;

namespace IdentityServer4.Server.Models
{
    public static class TestUsers
    {
        public static List<TestUser> GeTestUsers()
            => new List<TestUser>
            {
                new TestUser
                {
                    Username = "user1",
                    Password = "password1",
                    IsActive = true,
                    SubjectId = "user1"
                }
            };
    }
}