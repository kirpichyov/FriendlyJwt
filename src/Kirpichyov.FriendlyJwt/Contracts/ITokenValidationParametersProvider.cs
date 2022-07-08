using Microsoft.IdentityModel.Tokens;

namespace Kirpichyov.FriendlyJwt.Contracts
{
    public interface ITokenValidationParametersProvider
    {
        public TokenValidationParameters Value { get; }
    }
}