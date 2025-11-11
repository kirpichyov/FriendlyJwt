using System;
using System.IdentityModel.Tokens.Jwt;

namespace Kirpichyov.FriendlyJwt
{
    public class GeneratedTokenInfo
    {
        public string TokenId { get; init; }
        public string Audience { get; init; }
        public string Issuer { get; init; }
        [Obsolete("Use ExpiresAtUtc property instead.")]
        public DateTime ExpiresOn { get; init; }
        public DateTime ExpiresAtUtc { get; init; }
        public string Token { get; init; }
        public JwtSecurityToken SecurityToken { get; init; }
    }
}