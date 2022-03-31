### Overview

![main workflow](https://github.com/kirpichyov/FriendlyJwt/actions/workflows/dotnet.yml/badge.svg)
[![Coverage Status](https://coveralls.io/repos/github/kirpichyov/FriendlyJwt/badge.svg?branch=main)](https://coveralls.io/github/kirpichyov/FriendlyJwt?branch=main)
[![NuGet](http://img.shields.io/nuget/vpre/Kirpichyov.FriendlyJwt.svg?version=2&label=NuGet)](https://www.nuget.org/packages/Kirpichyov.FriendlyJwt/)
[![License](https://img.shields.io/badge/license-MIT-green)](./LICENSE)

**FriendlyJwt** is the custom JWT token authentication services wrapper library for ASP.NET Core 5.0.

### Get started
🎯 Download the [NuGet package](https://www.nuget.org/packages/Kirpichyov.FriendlyJwt/).

🎯 Register services in the **Startup.cs**.

```c#
    public void ConfigureServices(IServiceCollection services)
    {
        // ......
        
        services.AddHttpClient();
        services.AddHttpContextAccessor();

        services.AddFriendlyJwt(); // <-- FriendlyJwt services registration
        
        // ......
    }
```

🎯 Register authentication handlers in the **Startup.cs**.

```c#
    public void ConfigureServices(IServiceCollection services)
    {
    
        // ......
    
        services.AddControllers()
                // FriendlyJwt authorization services registration below
                .AddFriendlyJwtAuthentication(configuration =>
                {
                    configuration.Audience = "someaudience.com";
                    configuration.Issuer = "someissuer";
                    configuration.Secret = "SecretYGPV8XC6bPJhQCUBV2LtDSharp";
                });
                
        // ......
    }
```

💡**Audience** and **Issuer** are optional. If values not provided, then validation will be disabled.

💡 Method has the second parameter (**postSetupDelegate**), that allows to perform post configuration for authentication.

⚠️⚠️ Ensure that **UseAuthentication** and **UseAuthorization** was called in **Startup.cs**.


```c#
    // ......

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    
    app.UseAuthentication(); // <--
    app.UseAuthorization(); // <--
    
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapGet("/healthcheck", async context =>
        { 
            await context.Response.WriteAsync($"Healthy! [{DateTime.Now}]");
        });
    });

    // ......
```

---

### How to create token
You can find the example below:
```c#
    TimeSpan lifeTime = TimeSpan.FromMinutes(1);
    string secret = "SecretYGPV8XC6bPJhQCUBV2LtDSharp";

    GeneratedTokenInfo generatedTokenInfo =
        new JwtTokenBuilder(lifeTime, secret)
            .WithIssuer("someissuer")
            .WithAudience("someaudience")
            .WithUserRolesPayloadData(new[] { "admin", "supervisor" });
            .WithUserIdPayloadData("13567")
            .WithUserEmailPayloadData("usermail@example.com")
            .WithPayloadData("time_zone", "Mid-Atlantic Standard Time")
            .WithPayloadData("custom_key", "some custom value")
            .Build();
```
Builder will return the **GeneratedTokenInfo** object that will contain the token and related information like expiration date and token identifier (jti).

💡 In case if you does not want to use GUID based token id (jti) you can use custom, just use the method **.WithCustomTokenId("your_value")**.

💡 Constructor contains the required parameters, so you can just call
**new JwtTokenBuilder.Build()** to get token, if you does not need the additional information or validation.

### How to read the token payload values
🎯 Inject **IJwtTokenReader** service via constructor:

```c#
    public SomeService(IJwtTokenReader jwtTokenReader, .....)
    {
       //......
    }
```

Now you can use different methods and properties to access the payload data:

```c#
    //......
    
    // will return true if user authenticated
    bool isLogged = _jwtTokenReader.IsLoggedIn;

    // will retrieve the email if default key was used (via WithUserEmailPayloadData() method)
    string userEmail = _jwtTokenReader.UserEmail;

    // will retrieve the user id if default key was used (via WithUserIdPayloadData() method)
    string userId = _jwtTokenReader.UserId;
    
    // will retrieve the user roles if default key was used (via WithUserRolesPayloadData() method)
    string[] userRoles = _jwtTokenReader.UserRoles;

    // will retrieve the value via key passed to indexer
    // will throw exception if key is not present
    string someValue = _jwtTokenReader["my_key"];

    // will retrieve the value via key passed to method
    // will throw exception if key is not present
    string someOtherValue = _jwtTokenReader.GetPayloadValue("my_key");

    // will retrieve the value via key passed to method
    // will return null if key is not present
    string someVeryOtherValue = _jwtTokenReader.GetPayloadValueOrDefault("my_key");

    // will retrieve the all values for passed key
    // will return empty array if key is not present
    string[] someManyValues = _jwtTokenReader.GetPayloadValues("my_shared_key");

    // will return the all payload entries
    (string Key, string Value)[] allValues = _jwtTokenReader.GetPayloadData();

    //......
```

---

### How to validate the issued token (refresh token approach)
🎯 Inject **IJwtTokenVerifier** service via constructor:

```c#
    public SomeService(IJwtTokenVerifier jwtTokenVerifier, .....)
    {
       //......
    }
```

🎯 Call the verification method:

```c#
    JwtVerificationResult verificationResult =_jwtTokenVerifier.Verify(refreshTokenDto.Token);
```

JwtVerificationResult will contain the **IsValid** property  and retrieved **TokenId** and **UserId**.

💡 You should pass the values for **tokenIdPayloadKey** and **userIdPayloadKey** properties in case if you are using custom payload keys to store this values.
