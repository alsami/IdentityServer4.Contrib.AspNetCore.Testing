using Microsoft.Extensions.Logging;

namespace IdentityServer4.Testing.Infrastructure.Validators
{
    public class ResourceOwnerValidatorWithDependencies : SimpleResourceOwnerPasswordValidator
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ILogger<ResourceOwnerValidatorWithDependencies> logger;

        public ResourceOwnerValidatorWithDependencies(ILogger<ResourceOwnerValidatorWithDependencies> logger)
        {
            this.logger = logger;
        }
    }
}