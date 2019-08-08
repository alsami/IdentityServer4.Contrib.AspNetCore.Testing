# IdentityServer4.Contrib.AspNetCore.Testing

![NuGet](https://img.shields.io/nuget/dt/IdentityServer4.Contrib.AspNetCore.Testing.svg)
![Nuget](https://img.shields.io/nuget/v/Identityserver4.contrib.aspnetcore.testing)
![NuGet](https://img.shields.io/nuget/vpre/IdentityServer4.Contrib.AspNetCore.Testing.svg)
![Build Status](https://travis-ci.com/cleancodelabs/IdentityServer4.Contrib.AspNetCore.Testing.svg?branch=master)

This is a cross platform library, written in `.netstandard 2.0`, that serves as a testing framework for [IdentityServer4](http://docs.identityserver.io/en/latest/) using [Microsoft.AspNetCore.Mvc.Testing](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.2) and makes it easy to test your web-applications with `IdentityServer4`.

## Installation

This package is available via nuget. You can install it using Visual-Studio-Nuget-Browser or by using the dotnet-cli for your test-project.

```unspecified
dotnet add package IdentityServer4.Contrib.AspNetCore.Testing
```

If you want to add a specific version of this package

```unspecified
dotnet add package IdentityServer4.Contrib.AspNetCore.Testing --version 1.0.0
```

For more information please visit the official [dotnet-cli documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-add-package).

## Usage

This library is supposed to be used within test-projects. Please checkout the [prerequisites](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.2#test-app-prerequisites) described by Microsoft.

### Load the discover-document

```csharp
var webHostBuilder = new IdentityServerWebHostBuilder()
    .AddClients(new Client
    {
        ClientId = "MyClient",
        ClientSecrets = new List<Secret>
        {
            new Secret("MySecret".Sha256())
        }
    })
    .AddApiResources(new ApiResource())
    .CreateWebHostBuilder();

var identityServerClient = new IdentityServerProxy(webHostBuilder);
var discoveryResponse = await identityServerClient.GetDiscoverResponseAsync();

Assert.NotNull(discoveryResponse);
Assert.False(discoveryResponse.IsError, discoveryResponse.Error);
```

### Request a token using client-credentials

```csharp
var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

var client = new Client
{
    ClientId = clientConfiguration.Id,
    ClientSecrets = new List<Secret>
    {
        new Secret(clientConfiguration.Secret.Sha256())
    },
    AllowedScopes = new[] {"api1"},
    AllowedGrantTypes = new[] {GrantType.ClientCredentials},
    AccessTokenType = AccessTokenType.Jwt,
    AccessTokenLifetime = 7200
};

var webHostBuilder = new IdentityServerWebHostBuilder()
    .AddClients(client)
    .AddApiResources(new ApiResource("api1", "api1name"))
    .CreateWebHostBuilder();

var identityServerProxy = new IdentityServerProxy(webHostBuilder);

var tokenResponse = await identityServerProxy.GetClientAccessTokenAsync(clientConfiguration, "api1");

Assert.NotNull(tokenResponse);
Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
```

### Resource-Owner-Grant with a typed `IResourceOwnerPasswordValidator`

```csharp
var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

var client = new Client
{
    ClientId = clientConfiguration.Id,
    ClientSecrets = new List<Secret>
    {
        new Secret(clientConfiguration.Secret.Sha256())
    },
    AllowedScopes = new[] {"api1", IdentityServerConstants.StandardScopes.OfflineAccess},
    AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
    AccessTokenType = AccessTokenType.Jwt,
    AccessTokenLifetime = 7200,
    AllowOfflineAccess = true
};

var webHostBuilder = new IdentityServerWebHostBuilder()
    .AddClients(client)
    .AddApiResources(new ApiResource("api1", "api1name"))
    .UseResourceOwnerPasswordValidator(typeof(SimpleResourceOwnerPasswordValidator))
    .CreateWebHostBuilder();

var identityServerProxy = new IdentityServerProxy(webHostBuilder);

var tokenResponse = await identityServerProxy.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
    new UserLoginConfiguration("user", "password"),
    "api1", "offline_access");

Assert.NotNull(tokenResponse);
Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
```

### Requesting a refresh-token

```csharp
var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

var client = new Client
{
    ClientId = clientConfiguration.Id,
    ClientSecrets = new List<Secret>
    {
        new Secret(clientConfiguration.Secret.Sha256())
    },
    AllowedScopes = new[] {"api1", IdentityServerConstants.StandardScopes.OfflineAccess},
    AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
    AccessTokenType = AccessTokenType.Jwt,
    AccessTokenLifetime = 7200,
    AllowOfflineAccess = true
};

var webHostBuilder = new IdentityServerWebHostBuilder()
    .AddClients(client)
    .AddApiResources(new ApiResource("api1", "api1name"))
    .UseResourceOwnerPasswordValidator(new SimpleResourceOwnerPasswordValidator())
    .CreateWebHostBuilder();

var identityServerProxy = new IdentityServerProxy(webHostBuilder);

var scopes = new[] {"api1", "offline_access"};

var tokenResponse = await identityServerProxy.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
    new UserLoginConfiguration("user", "password"),
    scopes);

// We are breaking the pattern arrange / act / assert here but we need to make sure token requested successfully first
Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);


var refreshTokenResponse = await identityServerProxy
    .GetRefreshTokenAsync(clientConfiguration, tokenResponse.RefreshToken,
        scopes);

Assert.NotNull(refreshTokenResponse);
Assert.False(refreshTokenResponse.IsError)
```

### Requesting user-infos

```csharp
var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

var client = new Client
{
    ClientId = clientConfiguration.Id,
    ClientSecrets = new List<Secret>
    {
        new Secret(clientConfiguration.Secret.Sha256())
    },
    AllowedScopes = new[]
    {
        "api1", IdentityServerConstants.StandardScopes.OfflineAccess,
        IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile
    },
    AllowedGrantTypes = new[] {GrantType.ClientCredentials, GrantType.ResourceOwnerPassword},
    AccessTokenType = AccessTokenType.Jwt,
    AccessTokenLifetime = 7200,
    AllowOfflineAccess = true
};

var webHostBuilder = new IdentityServerWebHostBuilder()
    .AddClients(client)
    .AddApiResources(new ApiResource("api1", "api1name"))
    .AddIdentityResources(new IdentityResources.OpenId(), new IdentityResources.Profile())
    .UseResourceOwnerPasswordValidator(new SimpleResourceOwnerPasswordValidator())
    .UseProfileService(new SimpleProfileService())
    .CreateWebHostBuilder();

var identityServerProxy = new IdentityServerProxy(webHostBuilder);

var scopes = new[] {"api1", "offline_access", "openid", "profile"};

var tokenResponse = await identityServerProxy.GetResourceOwnerPasswordAccessTokenAsync(clientConfiguration,
    new UserLoginConfiguration("user", "password"),
    scopes);

// We are breaking the pattern arrange / act / assert here but we need to make sure token requested successfully first
Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);


var userInfoResponse = await identityServerProxy
    .GetUserInfoAsync(tokenResponse.AccessToken);

Assert.NotNull(userInfoResponse);
Assert.False(userInfoResponse.IsError);
Assert.NotNull(userInfoResponse.Claims);
```

### Requesting a token from a custom-configuration

```csharp
var clientConfiguration = new ClientConfiguration("MyClient", "MySecret");

var client = new Client
{
    ClientId = clientConfiguration.Id,
    ClientSecrets = new List<Secret>
    {
        new Secret(clientConfiguration.Secret.Sha256())
    },
    AllowedScopes = new[]
    {
        "api1", IdentityServerConstants.StandardScopes.OfflineAccess,
        IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile
    },
    AllowedGrantTypes = new[] {"Custom"},
    AccessTokenType = AccessTokenType.Jwt,
    AccessTokenLifetime = 7200,
    AllowOfflineAccess = true
};

var webHostBuilder = new IdentityServerWebHostBuilder()
    .AddClients(client)
    .AddApiResources(new ApiResource("api1", "api1name"))
    .AddIdentityResources(new IdentityResources.OpenId(), new IdentityResources.Profile())
    .UseServices((context, collection) =>
        collection.AddScoped<IExtensionGrantValidator, ExtensionsGrantValidator>())
    .CreateWebHostBuilder();

var identityServerProxy = new IdentityServerProxy(webHostBuilder);

var scopes = new[] {"api1", "offline_access", "openid", "profile"};

var tokenResponse = await identityServerProxy.GetTokenAsync(clientConfiguration, "Custom",
    new Dictionary<string, string>
    {
        {"scope", string.Join(" ", scopes)},
        {"username", "user"},
        {"password", "password"},
    });

Assert.NotNull(tokenResponse);
Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
Assert.Equal(7200, tokenResponse.ExpiresIn);
Assert.NotNull(tokenResponse.AccessToken);
Assert.NotNull(tokenResponse.RefreshToken);
```

### Using an existing `WebHostBuilder`

```csharp
var webHostBuilder = new IdentityServerWebHostBuilder()
    .UseWebHostBuilder(Program.CreateWebHostBuilder(new string[] { }))
    .CreateWebHostBuilder();

var proxy = new IdentityServerProxy(webHostBuilder);

var scopes = new[] {"api1", "offline_access", "openid", "profile"};

var tokenResponse = await proxy.GetResourceOwnerPasswordAccessTokenAsync(
    new ClientConfiguration(Clients.Id, Clients.Secret),
    new UserLoginConfiguration("user1", "password1"), scopes);

Assert.False(tokenResponse.IsError, tokenResponse.Error ?? tokenResponse.ErrorDescription);
```

For more samples and usages, please also have a look at the [tests](https://github.com/cleancodelabs/IdentityServer4.Contrib.AspNetCore.Testing/tree/master/test).