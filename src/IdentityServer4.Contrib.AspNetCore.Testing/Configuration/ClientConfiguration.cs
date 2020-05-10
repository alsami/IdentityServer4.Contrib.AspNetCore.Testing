namespace IdentityServer4.Contrib.AspNetCore.Testing.Configuration
{
    public class ClientConfiguration
    {
        public ClientConfiguration(string id, string secret)
        {
            this.Id = id;
            this.Secret = secret;
        }

        public string Id { get; }

        public string Secret { get; }
    }
}