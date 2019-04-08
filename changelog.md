# [1.1.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/1.1.0) (2019-04-08)

## Features

* Expose `TestServer` from class `IdentityServerProxy` in order to be able to create a handler manually.

# [1.0.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/1.0.0) (2019-04-08)

## Intial Release

This is the inital release of the package. This release contains two mandatory classes

* `IdentityServerWebHostBuilder`
* `IdentityServerProxy`

### [IdentityServerWebHostBuilder](https://github.com/cleancodelabs/IdentityServer4.Contrib.AspNetCore.Testing/blob/master/src/IdentityServer4.Contrib.AspNetCore.Testing/Builder/IdentityServerWebHostBuilder.cs)

The `IdentityServerWebHostBuilder` class is a fluent-builder that contains the following functions

* `AddApiResources(params ApiResource[] apiResources)`
* `AddClients(params Client[] clients)`
* `AddIdentityResources(params IdentityResource[] identityResources)`
* `UseApplicationBuilder(Action<IApplicationBuilder> applicationBuilder)`
* `UseConfigurationBuilder(
            Action<WebHostBuilderContext, IConfigurationBuilder> configurationBuilder)`
* `UseServices(
            Action<WebHostBuilderContext, IServiceCollection> servicesBuilder)`
* `UseLoggingBuilder(
            Action<WebHostBuilderContext, ILoggingBuilder> loggingBuilder)`
* `UseResourceOwnerPasswordValidator(Type type)`
* `UseResourceOwnerPasswordValidator<TResourceOwnerPasswordValidator>(
            TResourceOwnerPasswordValidator resourceOwnerPasswordValidator)
            where TResourceOwnerPasswordValidator : class, IResourceOwnerPasswordValidator`
* `UseProfileService(Type type)`
* `UseProfileService<TProfileService>(
            TProfileService profileService) where TProfileService : class, IProfileService`
* `CreateWebHostBuilder()`
* `UseWebHostBuilder(IWebHostBuilder webHostBuilder)`

When calling `CreateWebHostBuilder()` a `WebHostBuilder` is created based on the configuration. With that builder we create the `IdentityServerProxy`.

### [IdentityServerProxy](https://github.com/cleancodelabs/IdentityServer4.Contrib.AspNetCore.Testing/blob/master/src/IdentityServer4.Contrib.AspNetCore.Testing/Services/IdentityServerProxy.cs)

The `IndentityServerProxy`, as the name says, serves as a proxy for `IdentityServer4` and takes a `WebHostBuilder` as constructor parameter, then creates a `TestServer` from `Microsoft.AspNetCore.Mvc.Testing`

Following functions available

* `GetDiscoverResponseAsync()`
* `GetTokenAsync(ClientConfiguration clientConfiguration, string grantType,
            IDictionary<string, string> parameters)`
* `GetClientAccessTokenAsync(ClientConfiguration clientConfiguration,
            params string[] scopes)`
* `GetClientAccessTokenAsync(ClientConfiguration clientConfiguration,
            IDictionary<string, string> parameters, params string[] scopes)`
* `GetResourceOwnerPasswordAccessTokenAsync(
            ClientConfiguration clientConfiguration, UserLoginConfiguration userLoginConfiguration,
            params string[] scopes)`
* `GetResourceOwnerPasswordAccessTokenAsync(
            ClientConfiguration clientConfiguration, UserLoginConfiguration userLoginConfiguration,
            Dictionary<string, string> parameters)`
* `GetRefreshTokenAsync(ClientConfiguration clientConfiguration,
            string refreshToken, params string[] scopes)`
* `GetRefreshTokenAsync(ClientConfiguration clientConfiguration,
            string refreshToken, IDictionary<string, string> parameters)`
* `GetUserInfoAsync(string accessToken)`