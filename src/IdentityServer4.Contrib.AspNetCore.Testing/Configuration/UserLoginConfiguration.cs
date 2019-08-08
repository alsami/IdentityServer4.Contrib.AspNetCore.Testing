namespace IdentityServer4.Contrib.AspNetCore.Testing.Configuration
{
    public class UserLoginConfiguration
    {
        public string Username { get; }

        public string Password { get; }

        public UserLoginConfiguration(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}