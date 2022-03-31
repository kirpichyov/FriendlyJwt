namespace Kirpichyov.FriendlyJwt.IntegrationsTests.Constants
{
    public static class ApiConstants
    {
        public const string RouteBase = "api/";

        public static class Auth
        {
            public const string Anonymous = "anonymous-get";
            public const string Protected = "protected-action";
            public const string ProtectedAdmin = "protected-action-admin";
        }
        
        public static class Controllers
        {
            public const string AuthController = "auth";
            public const string AuthReaderController = "authReader";
        }
    }
}