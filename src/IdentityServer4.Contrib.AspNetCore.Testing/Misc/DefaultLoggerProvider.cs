using Microsoft.Extensions.Logging;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Misc
{
    internal class DefaultLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerFactory loggerFactory;

        public DefaultLoggerProvider(ILoggerFactory loggerFactor = null)
        {
            this.loggerFactory = loggerFactor ?? new LoggerFactory();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return this.loggerFactory.CreateLogger(categoryName);
        }

        public void Dispose()
        {
            this.loggerFactory.Dispose();
        }
    }
}