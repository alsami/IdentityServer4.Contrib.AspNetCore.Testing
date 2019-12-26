namespace IdentityServer4.Contrib.AspNetCore.Testing.Configuration
{
    public class UserLoginConfiguration
    {
        public UserLoginConfiguration(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; }

        public string Password { get; }
    }
}