# [4.2.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/4.2.0) (2021-11-04)

## Features

Allow `ApiResource` and `ApiScope` to differ and only check that an `ApiResource` has corresponding `ApiScope`. Thanks to [@gerrylowe](https://github.com/gerrylowe)! Closes [#5](https://github.com/alsami/IdentityServer4.Contrib.AspNetCore.Testing/issues/5)

# [4.0.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/4.0.0) (2020-06-20)

## Features

* Support for `IdentityServer4` version 4

# [4.0.0-rc.3](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/4.0.0-rc.3) (2020-06-13)

## Features

* Support for `IdentityServer4` version 4 preview 6

# [4.0.0-rc.2](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/4.0.0-rc.1) (2020-05-22)

## Features

* Remove class `IdentityServerTestHostCustomContainerBuilder` and add overload to build a host with custom service provider in `IdentityServerTestHostBuilder` class

# [4.0.0-rc.1](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/4.0.0-rc.1) (2020-05-21)

## Features

* Support for `IdentityServer4` version 4 preview 5
* New class `IdentityServerTestHostCustomContainerBuilder` which extends `IdentityServerTestHostBuilder` and allows hooking in a third-party container while building the host. You can find an `Autofac` sample in the tests.

# [4.0.0-rc.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/4.0.0-rc.0) (2020-05-10)

## Features

* Support for `IdentityServer4` version 4 preview 3
* New class `IdentityServerTestHostBuilder` which allows defining an `IHostBuilder`
* New class `IdentityServerTestWebHostBuilder` which contains the already defined logic of `IdentityServerHostBuilder`
* New class `IdentityServerHostProxy` which takes an `IHostBuilder` for creation of the `TestServer`
* New class `IdentityServerWebHostProxy` which contains the already defined logic of `IdentityServerProxy`

## Breaking changes

* When defining an `ApiResource` according `ApiScope` is required to be passed along. This is a breaking-change introduced through the update of `IdentityServer4` to version 4. The according builder used validates that api-scopes are present when using api-resources. It will throw an exception if not.
* For more details on the breaking changes of `IdentityServer4` please check out the [changelog](https://github.com/IdentityServer/IdentityServer4/releases/tag/4.0.0-preview.4) ot it.

## Deprecation notice

* `IdentityServerHostBuilder` has been marked as deprecated and will be removed with version 5. Please use `IdentityServerTestHostBuilder` or `IdentityServerTestWebHostBuilder` instead
* `IdentityServerProxy` has been marked as deprecated and will be removed with version 5. Please use `IdentityServerHostProxy` or `IdentityServerWebHostProxy` instead.

# [3.1.1](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/3.1.1) (2019-12-26)

## Chore

* Update `IdentityServer4` to version `3.1.0`

# [3.1.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/3.1.0) (2019-12-11)

## Breaking changes

* .NET Core 3.1 is now required

## Chore

* Update dependencies to `3.1.0`

# [3.0.2](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/3.0.2) (2019-11-12)

## Chore

* Update `IdentityServer4` to 3.0.2
* Update `IdentityModel` to 4.1.0

# [3.0.1](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/3.0.1) (2019-09-25)

## Chore

* Update `IdentityServer4` to 3.0.1

# [3.0.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/3.0.0) (2019-09-23)

## Breaking changes

* .NET-Core 3 is now required.
* `IdentityServerWebHostBuilder` has been renamed to `IdentityServerHostBuilder`

## Features

* .NET-Core 3 support!

# [1.4.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/1.4.0) (2019-07-19)

## Chore

* Update `IdentityServer4` to version 2.5.0
* Update `IdentitxModel` to version 3.10.10

# [1.3.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/1.3.0) (2019-05-05)

## Features

* Add constructor to `IdentityServerProxy` to directly pass in a `TestServer`

# [1.2.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/1.2.0) (2019-04-09)

## Features

* Allow passing of an `IdentityServerOptionsBuilder` to use custom options in `IdentityServerWebHostBuilder`
* Allow passing of an `IdentityServerBuilder` to use a custom builder in `IdentityServerWebHostBuilder`

## Chore

* Update Identity.Model to 3.10.7

## Features

* Expose `TestServer` from class `IdentityServerProxy` in order to be able to create a handler manually.

# [1.1.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/1.1.0) (2019-04-08)

## Features

* Expose `TestServer` from class `IdentityServerProxy` in order to be able to create a handler manually.

# [1.0.0](https://www.nuget.org/packages/IdentityServer4.Contrib.AspNetCore.Testing/1.0.0) (2019-04-08)

## Intial Release

This is the inital release of the package. This release contains two mandatory classes

* `IdentityServerWebHostBuilder`
* `IdentityServerProxy`

### [IdentityServerWebHostBuilder](https://github.com/alsami/IdentityServer4.Contrib.AspNetCore.Testing/blob/master/src/IdentityServer4.Contrib.AspNetCore.Testing/Builder/IdentityServerWebHostBuilder.cs)

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

### [IdentityServerProxy](https://github.com/alsami/IdentityServer4.Contrib.AspNetCore.Testing/blob/master/src/IdentityServer4.Contrib.AspNetCore.Testing/Services/IdentityServerProxy.cs)

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
