using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Kirpichyov.FriendlyJwt.Constants;
using Kirpichyov.FriendlyJwt.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace Kirpichyov.FriendlyJwt.RefreshTokenUtilities
{
    public class JwtTokenVerifier : IJwtTokenVerifier
    {
        private readonly TokenValidationParameters _tokenValidationParameters;
        
        public JwtTokenVerifier(TokenValidationParameters tokenValidationParameters)
        {
            _tokenValidationParameters = tokenValidationParameters;
        }

        /// <inheritdoc/>
        public JwtVerificationResult Verify(string token, string tokenIdPayloadKey = null, string userIdPayloadKey = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            TokenValidationParameters validationParameters = _tokenValidationParameters.Clone();
            validationParameters.ValidateLifetime = false;

            try
            {
                ClaimsPrincipal principal =
                    tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);
                
                if (ValidateJwtSecurityAlgorithm(securityToken))
                {
                    string tokenId = principal.Claims.Single(claim => claim.Type == (tokenIdPayloadKey ?? PayloadDataKeys.TokenId)).Value;
                    string userId = principal.Claims.Single(claim => claim.Type == (userIdPayloadKey ?? PayloadDataKeys.UserId)).Value;
                    
                    return new JwtVerificationResult
                    {
                        IsValid = true,
                        TokenId = tokenId,
                        UserId = userId
                    };
                }
            }
            catch
            {
                return InvalidResult();
            }

            return InvalidResult();
        }

        private static bool ValidateJwtSecurityAlgorithm(SecurityToken token)
        {
            return token is JwtSecurityToken jwtSecurityToken &&
                   jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                       StringComparison.InvariantCultureIgnoreCase);
        }

        private static JwtVerificationResult InvalidResult()
        {
            return new JwtVerificationResult
            {
                IsValid = false,
                TokenId = null,
                UserId = null
            };
        }
    }
}