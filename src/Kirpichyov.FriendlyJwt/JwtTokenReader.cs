using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Kirpichyov.FriendlyJwt.Constants;
using Kirpichyov.FriendlyJwt.Contracts;
using Microsoft.AspNetCore.Http;

namespace Kirpichyov.FriendlyJwt
{
    public sealed class JwtTokenReader : IJwtTokenReader
    {
        /// <inheritdoc/>
        public bool IsLoggedIn { get; }
        
        /// <inheritdoc/>
        public string UserId { get; }
        
        /// <inheritdoc/>
        public string UserEmail { get; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public JwtTokenReader(IHttpContextAccessor httpContextAccessor)
        {
            IsLoggedIn = false;
            
            if (httpContextAccessor.HttpContext == null)
            {
                return;
            }
            
            HttpContext httpContext = httpContextAccessor.HttpContext;
            if (!httpContext.User.Claims.Any())
            {
                return;
            }
            
            _httpContextAccessor = httpContextAccessor;
            IsLoggedIn = true;
            
            UserId = GetPayloadValueOrDefault(PayloadDataKeys.UserId);
            UserEmail = GetPayloadValueOrDefault(PayloadDataKeys.UserEmail);
        }

        /// <inheritdoc/>
        public string GetPayloadValue(string key)
        {
            Claim claim = RetrieveClaimOrDefault(key);
            
            if (claim is null)
            {
                throw new KeyNotFoundException($"Data with key '{key}' is not present in the payload data.");
            }
            
            return claim.Value;
        }

        /// <inheritdoc/>
        public string GetPayloadValueOrDefault(string key) => RetrieveClaimOrDefault(key)?.Value;

        /// <inheritdoc/>
        public (string Key, string Value)[] GetPayloadData()
        {
            if (!IsLoggedIn)
            {
                throw new InvalidOperationException("User must be logged in to perform payload reading.");
            }

            return _httpContextAccessor.HttpContext.User.Claims
                .Select(claim => (claim.Type, claim.Value))
                .ToArray();
        }
        
        /// <inheritdoc/>
        public string this[string key] => GetPayloadValue(key);

        private Claim RetrieveClaimOrDefault(string key)
        {
            if (!IsLoggedIn)
            {
                throw new InvalidOperationException("User must be logged in to perform payload reading.");
            }

            return _httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(claim => claim.Type == key);
        }
    }
}