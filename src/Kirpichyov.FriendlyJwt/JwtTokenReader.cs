﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Kirpichyov.FriendlyJwt.Constants;
using Kirpichyov.FriendlyJwt.Contracts;
using Microsoft.AspNetCore.Http;

namespace Kirpichyov.FriendlyJwt
{
    public class JwtTokenReader : IJwtTokenReader
    {
        /// <inheritdoc/>
        public bool IsLoggedIn { get; }
        
        /// <inheritdoc/>
        public string UserId { get; }
        
        /// <inheritdoc/>
        public string UserEmail { get; }

        /// <inheritdoc/>
        public string UserName { get; }

        /// <inheritdoc/>
        public string[] UserRoles { get; }

        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtTokenReader(IHttpContextAccessor httpContextAccessor, 
                              ITokenValidationParametersProvider tokenValidationParameters)
        {
            IsLoggedIn = false;
            
            if (httpContextAccessor.HttpContext is null)
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

            UserName = httpContext.User.FindFirst(tokenValidationParameters.Value.NameClaimType)?.Value;
            UserId = GetPayloadValueOrDefault(PayloadDataKeys.UserId);
            UserEmail = GetPayloadValueOrDefault(PayloadDataKeys.UserEmail);
            
            UserRoles = httpContext.User.FindAll(tokenValidationParameters.Value.RoleClaimType)
                                        .Select(roleClaim => roleClaim.Value)
                                        .ToArray();
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
        public string[] GetPayloadValues(string key)
        {
            ValidateIfLoggedInAndThrow();

            return _httpContextAccessor.HttpContext.User.Claims
                .Where(claim => claim.Type == key)
                .Select(claim => claim.Value)
                .ToArray();
        }

        /// <inheritdoc/>
        public string GetPayloadValueOrDefault(string key) => RetrieveClaimOrDefault(key)?.Value;

        /// <inheritdoc/>
        public (string Key, string Value)[] GetPayloadData()
        {
            ValidateIfLoggedInAndThrow();

            return _httpContextAccessor.HttpContext.User.Claims
                .Select(claim => (claim.Type, claim.Value))
                .ToArray();
        }
        
        /// <inheritdoc/>
        public string this[string key] => GetPayloadValue(key);

        private Claim RetrieveClaimOrDefault(string key)
        {
            ValidateIfLoggedInAndThrow();

            return _httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(claim => claim.Type == key);
        }

        private void ValidateIfLoggedInAndThrow()
        {
            if (!IsLoggedIn)
            {
                throw new InvalidOperationException("User must be logged in to perform payload reading.");
            }
        }
    }
}