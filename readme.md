### Overview
**FriendlyJwt** is the custom JWT token authentication services wrapper library for ASP.NET Core 5.0.

### Get started
🎯 Download the [NuGet package](https://www.nuget.org/packages/Kirpichyov.FriendlyJwt/).

🎯 Register services in the **Startup.cs**.

    public void ConfigureServices(IServiceCollection services)
    {
        // ......
        
        services.AddHttpClient();
        services.AddHttpContextAccessor();

        services.AddFriendlyJwt(); // <-- FriendlyJwt services registration
        
        // ......
    }

🎯 Register authorization services in the **Startup.cs**.

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

💡**Audience** and **Issuer** are optional. If values not provided, then validation will be disabled.

💡 Method has the second parameter (**postSetupDelegate**), that allows to perform post configuration for authentication.

### How to create token
You can find the example below:

    TimeSpan lifeTime = TimeSpan.FromMinutes(1);
    string secret = "SecretYGPV8XC6bPJhQCUBV2LtDSharp";

    GeneratedTokenInfo generatedTokenInfo =
        new JwtTokenBuilder(lifeTime, secret)
            .WithIssuer("someissuer")
            .WithAudience("someaudience")
            .WithUserIdPayloadData("13567")
            .WithUserEmailPayloadData("usermail@example.com")
            .WithPayloadData("time_zone", "Mid-Atlantic Standard Time")
            .WithPayloadData("custom_key", "some custom value")
            .Build();

Builder will return the **GeneratedTokenInfo** object that will contain the token and related information like expiration date and token identifier (jti).

💡 In case if you does not want to use GUID based token id (jti) you can use custom, just use the method **.WithCustomTokenId("your_value")**.

💡 Constructor contains the required parameters, so you can just call
**new JwtTokenBuilder.Build()** to get token, if you does not need the additional information or validation.

### How to read the token payload values
🎯 Inject **IJwtTokenReader** service via constructor:

    public SomeService(IJwtTokenReader jwtTokenReader, .....)
    {
       //......
    }

Now you can use different methods and properties to access the payload data:

    //......
    
    // will return true if user authenticated
    bool isLogged = _jwtTokenReader.IsLoggedIn;

    // will retrieve the email if default key was used (via WithUserEmailPayloadData() method)
    string email = _jwtTokenReader.UserEmail;

    // will retrieve the user id if default key was used (via WithUserIdPayloadData() method)
    string userId = _jwtTokenReader.UserId;

    // will retrieve the value via key passed to indexer
    // will throw exception if value is not present
    string someValue = _jwtTokenReader["my_key"];

    // will retrieve the value via key passed to method
    // will throw exception if value is not present
    string someOtherValue = _jwtTokenReader.GetPayloadValue("my_key");

    // will retrieve the value via key passed to method
    // will return null if value is not present
    string someVeryOtherValue = _jwtTokenReader.GetPayloadValueOrDefault("my_key");

    // will return the all payload entries
    (string Key, string Value)[] allValues = _jwtTokenReader.GetPayloadData();

    //......

### How to validate the issued token (refresh token approach)
🎯 Inject **IJwtTokenVerifier** service via constructor:

    public SomeService(IJwtTokenVerifier jwtTokenVerifier, .....)
    {
       //......
    }

🎯 Call the verification method:

    JwtVerificationResult verificationResult =_jwtTokenVerifier.Verify(refreshTokenDto.Token);

JwtVerificationResult will contain the **IsValid** property  and retrieved **TokenId** and **UserId**.

💡 You should pass the values for **tokenIdPayloadKey** and **userIdPayloadKey** properties in case if you are using custom payload keys to store this values.