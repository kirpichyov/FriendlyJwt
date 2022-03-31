using Kirpichyov.FriendlyJwt.IntegrationsTests.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kirpichyov.FriendlyJwt.IntegrationsTests.Common
{
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private const string LooksFine = "Looks fine!";
        
        [HttpGet(ApiConstants.Auth.Anonymous)]
        [AllowAnonymous]
        public IActionResult Anonymous()
        {
            return Ok(LooksFine);
        }

        [HttpPost(ApiConstants.Auth.Protected)]
        public IActionResult Protected()
        {
            return Ok(LooksFine);
        }
        
        [Authorize(Roles = AuthConstants.Roles.Admin)]
        [HttpPost(ApiConstants.Auth.ProtectedAdmin)]
        public IActionResult AdminOnly()
        {
            return Ok(LooksFine);
        }
    }
}