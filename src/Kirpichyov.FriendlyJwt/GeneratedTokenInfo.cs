using System;

namespace Kirpichyov.FriendlyJwt
{
    public class GeneratedTokenInfo
    {
        public string TokenId { get; init; }
        public string Audience { get; init; }
        public string Issuer { get; init; }
        public DateTime ExpiresOn { get; init; }
        public string Token { get; init; }
    }
}