﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using Kirpichyov.FriendlyJwt.RefreshTokenUtilities;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Kirpichyov.FriendlyJwt.UnitTests
{
    [ExcludeFromCodeCoverage]
    public class JwtVerifierTests
    {
        [Theory]
        [MemberData(nameof(JwtTestCases))]
        public void Verify_TokenProvided_JwtVerificationResultValuesShouldBeEqualExpected(
            string token,
            string secret, 
            string audience, 
            string issuer, 
            string tokenId, 
            string userId,
            bool isValid)
        {
            // Arrange
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                RequireExpirationTime = true,
                ValidateAudience = !string.IsNullOrEmpty(audience),
                ValidateIssuer = !string.IsNullOrEmpty(issuer),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                ClockSkew = TimeSpan.Zero
            };

            if (!string.IsNullOrEmpty(audience))
            {
                tokenValidationParameters.ValidAudience = audience;
            }
            if (!string.IsNullOrEmpty(issuer))
            {
                tokenValidationParameters.ValidIssuer = issuer;
            }

            var sut = new JwtTokenVerifier(new TokenValidationParametersProvider(tokenValidationParameters));

            // Act
            JwtVerificationResult result = sut.Verify(token);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().Be(isValid);
                result.TokenId.Should().Be(isValid ? tokenId : null);
                result.UserId.Should().Be(isValid ? userId : null);
            }
        }
        
        [Fact]
        public void Verify_TokenWithInvalidSignatureProvided_JwtVerificationResultValuesShouldBeEqualExpected()
        {
            string token = "eyJhbGciOiJIUzM4NCIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJkYTgzYTFlOS1mNDg5LTRiYzUtYmQ2Yi0zYTIxOTQ4" +
                           "NzI5MGUiLCJuYmYiOjE2NDg2NjgzMzYsImV4cCI6MTY0ODY3MTkzNiwiaWF0IjoxNjQ4NjY4MzM2LCJpc3MiOiJmc" +
                           "mllbmRseS1qd3QuY29tIiwiYXVkIjoidGVzdC1hdWRpZW5jZS5jb20ifQ.GHTCPHnNJ6xm6CVZt56JSjsLBOPWRxs" +
                           "IVp5zJ6xqKNYCSLdx3GsyE99AzQWkP9ut";
            
            string secret = "5t14b251pd4z3nh0mf3323j4ohry0zkj";
            
            // Arrange
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                RequireExpirationTime = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256Signature },
                ClockSkew = TimeSpan.Zero,
            };

            var sut = new JwtTokenVerifier(new TokenValidationParametersProvider(tokenValidationParameters));

            // Act
            JwtVerificationResult result = sut.Verify(token);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TokenId.Should().BeNull();
                result.UserId.Should().BeNull();
            }
        }
        
        [Fact]
        public void Verify_TokenWithValidSignatureProvided_JwtVerificationResultValuesShouldBeEqualExpected()
        {
            var secret = "iSr7b6u83AxvcW8tre3J1K4daKvRF7YA1tA61kcyTDR9rfQ4f09tMVBi7gSNdXn7PB1Du74wDRCURHarT5nba3hTif1wNmZg04HQ";

            var token = "eyJhbGciOiJIUzM4NCIsInR5cCI6IkpXVCJ9." +
                        "eyJqdGkiOiJkMGY4N2QzNi1hZTAxLTQ1NzItYWI4Ny1iYjgzNDJlZTczZTciLCJuYmYiOjE3MjY4MjU1OTUsImV4cCI6MTcyNjgyNTg5NSwiaWF0IjoxNzI2ODI1NTk1fQ." +
                        "KrYjf2xRPusE5e6pnBQqEUENSm95z0g8Y2jir6N3RiOarSr3_Ga-y4nUZfEo-xIS";
            
            // Arrange
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                RequireExpirationTime = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha384 },
                ClockSkew = TimeSpan.Zero,
            };

            var sut = new JwtTokenVerifier(new TokenValidationParametersProvider(tokenValidationParameters));

            // Act
            JwtVerificationResult result = sut.Verify(token);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeTrue();
                result.TokenId.Should().NotBeNullOrEmpty();
                result.UserId.Should().BeNull();
            }
        }
        
        public static IEnumerable<object[]> JwtTestCases = new[]
        {
            // token, secret, audience, issuer, tokenId, userId
            new [] 
            {
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoidGVzdC11c2VyLWlkIiwianRpIjoiYmQwMmUzYWMtOTA0My00NDI3LWFhMjgtODA3ZTY0YzUzYjlmIiwibmJmIjoxNjQyMDI0NDEwLCJleHAiOjE2NDIwMjYyMTAsImlhdCI6MTY0MjAyNDQxMCwiaXNzIjoiZnJpZW5kbHktand0LmNvbSIsImF1ZCI6InRlc3QtYXVkaWVuY2UuY29tIn0.m65Mzy3RKQTBHodtj11kjPP3iFSoJpgFfoZwsKZ73iw",
                "5t14b251pd4z3nh0mf3323j4ohry0zkj",
                "test-audience.com",
                "friendly-jwt.com",
                "bd02e3ac-9043-4427-aa28-807e64c53b9f",
                "test-user-id",
                "true"
            },
            new [] 
            {
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoidGVzdC11c2VyLWlkIiwianRpIjoiYmQyYWU0NjYtZmM0Ny00YTBlLWIxZGYtZmM5NDg0MmVkOTViIiwibmJmIjoxNjQyMDI0OTI1LCJleHAiOjE2NDIwMjY3MjUsImlhdCI6MTY0MjAyNDkyNSwiaXNzIjoiZnJpZW5kbHktand0LmNvbV9PVEhFUiIsImF1ZCI6InRlc3QtYXVkaWVuY2UuY29tX09USEVSIn0.HqGFGnMxBNLa8y-EyuJjYpiVI_iYN8fzBni3hY5uuLE",
                "bru935yt6x4dtwy0gm76ktd3kuk0g27r",
                "test-audience.com",
                "friendly-jwt.com",
                "bd2ae466-fc47-4a0e-b1df-fc94842ed95b",
                "test-user-id",
                "false"
            }
        };
    }
}