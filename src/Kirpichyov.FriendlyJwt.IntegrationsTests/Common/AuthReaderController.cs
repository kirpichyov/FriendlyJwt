using Kirpichyov.FriendlyJwt.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kirpichyov.FriendlyJwt.IntegrationsTests.Common
{
    [Route("api/[controller]")]
    [Authorize]
    public class AuthReaderController : ControllerBase
    {
        private readonly IJwtTokenReader _jwtTokenReader;

        public AuthReaderController(IJwtTokenReader jwtTokenReader)
        {
            _jwtTokenReader = jwtTokenReader;
        }

        [HttpGet]
        public AuthReaderResponse Get()
        {
            var response = new AuthReaderResponse();

            response.UserName = _jwtTokenReader.UserName;
            response.UserEmail = _jwtTokenReader.UserEmail;
            response.UserId = _jwtTokenReader.UserId;
            response.Roles = _jwtTokenReader.UserRoles;
            response.IsLoggedIn = _jwtTokenReader.IsLoggedIn;
            response.CustomClaim = _jwtTokenReader.GetPayloadValueOrDefault("custom_claim");

            return response;
        }
    }
}