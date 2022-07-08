using Kirpichyov.FriendlyJwt.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace Kirpichyov.FriendlyJwt
{
    public class TokenValidationParametersProvider : ITokenValidationParametersProvider
    {
        public TokenValidationParameters Value { get; }

        public TokenValidationParametersProvider(TokenValidationParameters value)
        {
            Value = value;
        }
    }
}