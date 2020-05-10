namespace IdentityServer4.Contrib.AspNetCore.Testing.Configuration
{
    public class UserLoginConfiguration
    {
        public UserLoginConfiguration(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        public string Username { get; }

        public string Password { get; }
    }
}