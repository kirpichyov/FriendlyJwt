namespace Kirpichyov.FriendlyJwt.RefreshTokenUtilities
{
    public readonly struct JwtVerificationResult
    {
        public bool IsValid { get; init; }
        public string TokenId { get; init; }
        public string UserId { get; init; }
    }
}