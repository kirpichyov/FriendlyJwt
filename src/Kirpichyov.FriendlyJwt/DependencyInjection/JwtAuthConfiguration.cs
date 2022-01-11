namespace Kirpichyov.FriendlyJwt.DependencyInjection
{
    public class JwtAuthConfiguration
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Secret { get; set; }
        public bool RequireHttpsMetadata { get; set; }

        public bool HasIssuer => !string.IsNullOrWhiteSpace(Issuer);
        public bool HasAudience => !string.IsNullOrWhiteSpace(Audience);
    }
}