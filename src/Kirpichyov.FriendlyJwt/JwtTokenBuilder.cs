using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Kirpichyov.FriendlyJwt.Constants;
using Microsoft.IdentityModel.Tokens;

namespace Kirpichyov.FriendlyJwt
{
    /// <summary>
    /// Allows to build the JWT token.
    /// </summary>
    public sealed class JwtTokenBuilder
    {
        private readonly TimeSpan _lifeTime;
        private readonly string _signatureSecretKey;
        private readonly List<Claim> _claims;
        private string _customJti;
        private string _audience;
        private string _issuer;
        private string _securityAlgorithm;
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="lifeTime">Token lifetime. Once it expires, the token will be expired too.</param>
        /// <param name="signatureSecretKey">Secret key, that will be used for signature.</param>
        /// <exception cref="ArgumentException">In case if secret key value is empty or too short (less than 32 characters).</exception>
        public JwtTokenBuilder(TimeSpan lifeTime, string signatureSecretKey)
        {
            ValidateStringAndThrow(signatureSecretKey, "Secret key", nameof(signatureSecretKey));

            if (signatureSecretKey.Length < 32)
            {
                throw new ArgumentException("Secret key length should be at least 32 characters.", nameof(signatureSecretKey));
            }

            _lifeTime = lifeTime;
            _signatureSecretKey = signatureSecretKey;
            _customJti = null;
            _claims = new List<Claim>();
            _securityAlgorithm = SecurityAlgorithms.HmacSha256Signature;
        }

        /// <summary>
        /// Adds the audience.
        /// </summary>
        /// <param name="audience">Value.</param>
        /// <returns>Builder.</returns>
        public JwtTokenBuilder WithAudience(string audience)
        {
            ValidateStringAndThrow(audience, "Audience", nameof(audience));

            _audience = audience;
            return this;
        }
        
        /// <summary>
        /// Adds the issuer.
        /// </summary>
        /// <param name="issuer">Value.</param>
        /// <returns>Builder.</returns>
        public JwtTokenBuilder WithIssuer(string issuer)
        {
            ValidateStringAndThrow(issuer, "Issuer", nameof(issuer));
            
            _issuer = issuer;
            return this;
        }
        
        /// <summary>
        /// Adds the custom token id instead of the default <see cref="System.Guid"/> based.
        /// </summary>
        /// <param name="customTokenId">Value.</param>
        /// <returns>Builder.</returns>
        public JwtTokenBuilder WithCustomTokenId(string customTokenId)
        {
            ValidateStringAndThrow(customTokenId, "Token id", nameof(customTokenId));
            
            _customJti = customTokenId;
            return this;
        }

        /// <summary>
        /// Adds the user name record to payload section.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns>Builder.</returns>
        public JwtTokenBuilder WithUserName(string userName)
        {
            ValidateStringAndThrow(userName, "User name", nameof(userName));
            
            _claims.Add(new Claim(PayloadDataKeys.UserName, userName));
            return this;
        }
        
        /// <summary>
        /// Adds the data record to payload section.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <param name="value">Data value.</param>
        /// <returns>Builder.</returns>
        public JwtTokenBuilder WithPayloadData(string key, string value)
        {
            ValidateStringAndThrow(key, "Key", nameof(key));
            ValidateStringAndThrow(value, "Value", nameof(value));
            
            _claims.Add(new Claim(key, value));
            return this;
        }

        /// <summary>
        /// Adds the data record to payload section.
        /// </summary>
        /// <param name="records">Array of the tuples with records.</param>
        /// <returns>Builder.</returns>
        public JwtTokenBuilder WithPayloadData((string Key, string Value)[] records)
        {
            foreach (var tuple in records)
            {
                ValidateStringAndThrow(tuple.Key, "Key", nameof(tuple.Key));
                ValidateStringAndThrow(tuple.Value, "Value", nameof(tuple.Value));
            }
            
            _claims.AddRange(records.Select(record => new Claim(record.Key, record.Value)));
            return this;
        }

        /// <summary>
        /// Adds the value for <see cref="PayloadDataKeys.UserId"/> key to payload section.
        /// </summary>
        /// <param name="userId">Value.</param>
        /// <returns>Builder.</returns>
        public JwtTokenBuilder WithUserIdPayloadData(string userId)
        {
            ValidateStringAndThrow(userId, "User id", nameof(userId));
            
            _claims.Add(new Claim(PayloadDataKeys.UserId, userId));
            return this;
        }
        
        /// <summary>
        /// Adds the value for <see cref="PayloadDataKeys.UserEmail"/> key to payload section.
        /// </summary>
        /// <param name="email">Value.</param>
        /// <returns>Builder.</returns>
        public JwtTokenBuilder WithUserEmailPayloadData(string email)
        {
            ValidateStringAndThrow(email, "Email", nameof(email));
            
            _claims.Add(new Claim(PayloadDataKeys.UserEmail, email));
            return this;
        }

        /// <summary>
        /// Adds the value for <see cref="PayloadDataKeys.UserRole"/> key to payload section.
        /// </summary>
        /// <param name="role">Value.</param>
        /// <returns>Builder.</returns>
        public JwtTokenBuilder WithUserRolePayloadData(string role)
        {
            ValidateStringAndThrow(role, "Role", nameof(role));
            
            _claims.Add(new Claim(PayloadDataKeys.UserRole, role));
            return this;
        }
        
        /// <summary>
        /// Adds the values for <see cref="PayloadDataKeys.UserRole"/> key to payload section.
        /// </summary>
        /// <param name="roles">Values.</param>
        /// <returns>Builder.</returns>
        public JwtTokenBuilder WithUserRolesPayloadData(params string[] roles)
        {
            foreach (string role in roles)
            {
                WithUserRolePayloadData(role);
            }
            
            return this;
        }

        /// <summary>
        /// Sets the security algorithm.
        /// </summary>
        /// <param name="algorithm">Security algorithm.</param>
        /// <returns>Builder.</returns>
        public JwtTokenBuilder WithSecurityAlgorithm(string algorithm)
        {
            ValidateStringAndThrow(algorithm, "Security algorithm", nameof(algorithm));

            _securityAlgorithm = algorithm;
            return this;
        }
        
        /// <summary>
        /// Builds the JWT token.
        /// </summary>
        /// <returns>Token with related information.</returns>
        public GeneratedTokenInfo Build()
        {
            string jti = _customJti ?? Guid.NewGuid().ToString();
            DateTime expiresOn = DateTime.UtcNow.Add(_lifeTime);

            if (_claims.All(claim => claim.Type != PayloadDataKeys.TokenId))
            {
                _claims.Add(new Claim(PayloadDataKeys.TokenId, jti));   
            }
            else
            {
                jti = _claims.First(claim => claim.Type == PayloadDataKeys.TokenId).Value;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            
            byte[] key = Encoding.ASCII.GetBytes(_signatureSecretKey);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(_claims.ToArray()),
                Expires = expiresOn,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), _securityAlgorithm)
            };

            if (!string.IsNullOrEmpty(_audience))
            {
                tokenDescriptor.Audience = _audience;
            }
            if (!string.IsNullOrEmpty(_issuer))
            {
                tokenDescriptor.Issuer = _issuer;
            }
            
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return new GeneratedTokenInfo
            {
                Token = tokenHandler.WriteToken(token),
                Audience = _audience,
                Issuer = _issuer,
                ExpiresOn = expiresOn,
                TokenId = jti
            };
        }

        private void ValidateStringAndThrow(string stringValue, string exceptionName, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                throw new ArgumentException($"{exceptionName} can't be null or empty.", argumentName);
            }
        }
    }
}