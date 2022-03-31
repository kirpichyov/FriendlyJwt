namespace Kirpichyov.FriendlyJwt.IntegrationsTests.Common
{
    public class AuthReaderResponse
    {
        public bool IsLoggedIn { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string CustomClaim { get; set; }
        public string[] Roles { get; set; }
    }
}