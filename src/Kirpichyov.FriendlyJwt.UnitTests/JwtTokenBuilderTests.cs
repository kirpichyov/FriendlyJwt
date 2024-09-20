using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Kirpichyov.FriendlyJwt.Constants;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Kirpichyov.FriendlyJwt.UnitTests
{
    [ExcludeFromCodeCoverage]
    public class JwtTokenBuilderTests
    {
        private const int SecretLength = 64;
        private const string LifeTimeComparisonPattern = "yyyy-dd-MM hh:mm";

        private readonly Faker _faker;
        
        public JwtTokenBuilderTests()
        {
            _faker = new Faker();
        }
        
        [Fact]
        public void Ctor_ValidLifetimeAndSignatureSecretKeyProvided_ShouldNotThrowAnyException()
        {
            // Arrange
            TimeSpan lifeTime = _faker.Date.Timespan();
            string signatureSecretKey = _faker.Random.AlphaNumeric(SecretLength);
            
            // Act
            Func<JwtTokenBuilder> func = () => new JwtTokenBuilder(lifeTime, signatureSecretKey);
            
            // Assert
            func.Should().NotThrow();
        }
        
        [Fact]
        public void Ctor_LifetimeAndInvalidSignatureSecretKeyProvided_ShouldThrowArgumentException()
        {
            // Arrange
            TimeSpan lifeTime = _faker.Date.Timespan();
            string signatureSecretKey = _faker.Random.AlphaNumeric(31);
            
            // Act
            Func<JwtTokenBuilder> func = () => new JwtTokenBuilder(lifeTime, signatureSecretKey);
            
            // Assert
            func.Should().ThrowExactly<ArgumentException>();
        }
        
        [Fact]
        public void Ctor_ValidLifetimeAndSignatureSecretKeyProvided_GeneratedTokenInfoValuesShouldBeSetToExpected()
        {
            // Arrange
            TimeSpan lifeTime = _faker.Date.Timespan();
            string signatureSecretKey = _faker.Random.AlphaNumeric(SecretLength);

            DateTime expectedExpirationDate = DateTime.UtcNow.Add(lifeTime);
            
            // Act
            GeneratedTokenInfo result = new JwtTokenBuilder(lifeTime, signatureSecretKey).Build();
            
            // Assert
            using (new AssertionScope())
            {
                result.TokenId.Should().NotBeEmpty();
                
                result.ExpiresOn.ToUniversalTime().Should().Be(result.ExpiresOn);
                
                result.ExpiresOn.ToString(LifeTimeComparisonPattern)
                                .Should().Be(expectedExpirationDate.ToString(LifeTimeComparisonPattern));

                result.Token.Should().NotBeEmpty();
            }
        }
        
        [Fact]
        public void Ctor_ValidLifetimeAndSignatureSecretKeyProvided_ProvidedTokenValuesShouldBeSetToExpected()
        {
            // Arrange
            TimeSpan lifeTime = _faker.Date.Timespan();
            string signatureSecretKey = _faker.Random.AlphaNumeric(SecretLength);

            DateTime expectedExpirationDate = DateTime.UtcNow.Add(lifeTime);
            
            // Act
            GeneratedTokenInfo result = new JwtTokenBuilder(lifeTime, signatureSecretKey).Build();
            
            // Assert
            using (new AssertionScope())
            {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(result.Token);

                jwtSecurityToken.Id.Should().NotBeEmpty();
                jwtSecurityToken.ValidTo.ToString(LifeTimeComparisonPattern)
                                        .Should().Be(expectedExpirationDate.ToString(LifeTimeComparisonPattern));
            }
        }
        
        [Fact]
        public void WithAudienceAndBuild_ValidAudienceProvided_GeneratedTokenInfoAndTokenValuesShouldBeSetToExpected()
        {
            // Arrange
            string audience = _faker.Internet.Url();
            
            // Act
            GeneratedTokenInfo result = BuildSut().WithAudience(audience).Build();
            
            // Assert
            using (new AssertionScope())
            {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(result.Token);

                result.Audience.Should().Be(audience);
                jwtSecurityToken.Audiences.Should().AllBe(audience);
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WithAudienceAndBuild_InvalidAudienceProvided_ShouldThrowArgumentException(string audience)
        {
            // Act
            Func<GeneratedTokenInfo> func = () => BuildSut().WithAudience(audience).Build();
            
            // Assert
            func.Should().ThrowExactly<ArgumentException>();
        }
        
        [Fact]
        public void WithIssuerAndBuild_ValidIssuerProvided_GeneratedTokenInfoAndTokenValuesShouldBeSetToExpected()
        {
            // Arrange
            string issuer = _faker.Internet.Url();
            
            // Act
            GeneratedTokenInfo result = BuildSut().WithIssuer(issuer).Build();
            
            // Assert
            using (new AssertionScope())
            {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(result.Token);

                result.Issuer.Should().Be(issuer);
                jwtSecurityToken.Issuer.Should().Be(issuer);
            }
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WithIssuerAndBuild_InvalidIssuerProvided_ShouldThrowArgumentException(string issuer)
        {
            // Act
            Func<GeneratedTokenInfo> func = () => BuildSut().WithIssuer(issuer).Build();
            
            // Assert
            func.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void WithSecurityAlgorithmAndBuild_ValidAlgorithmProvided_ShouldNotThrowAnyException()
        {
            // Arrange
            string algorithm = SecurityAlgorithms.HmacSha512Signature;
            
            // Act
            Func<GeneratedTokenInfo> func = () => BuildSut().WithSecurityAlgorithm(algorithm).Build();
            
            // Assert
            using (new AssertionScope())
            {
                func.Should().NotThrow();
            }
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WithSecurityAlgorithmAndBuild_InvalidAlgorithmProvided_ShouldThrowArgumentException(string algorithm)
        {
            // Act
            Func<GeneratedTokenInfo> func = () => BuildSut().WithSecurityAlgorithm(algorithm).Build();
            
            // Assert
            func.Should().ThrowExactly<ArgumentException>();
        }
        
        [Fact]
        public void WithUserNameAlgorithmAndBuild_ValidAlgorithmProvided_ShouldNotThrowAnyException()
        {
            // Arrange
            string userName = _faker.Person.UserName;
            
            // Act
            Func<GeneratedTokenInfo> func = () => BuildSut().WithUserName(userName).Build();
            
            // Assert
            using (new AssertionScope())
            {
                func.Should().NotThrow();
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WithUserNameAlgorithmAndBuild_InvalidAlgorithmProvided_ShouldThrowArgumentException(string userName)
        {
            // Act
            Func<GeneratedTokenInfo> func = () => BuildSut().WithUserName(userName).Build();
            
            // Assert
            func.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void WithPayloadData_CustomJtiPayloadDataWasProvided_ShouldNotOverwriteTokenId()
        {
            // Arrange
            string customJti = _faker.Random.AlphaNumeric(12);
            
            // Act
            GeneratedTokenInfo result = BuildSut().WithPayloadData("jti", customJti).Build();
            
            // Assert
            using (new AssertionScope())
            {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(result.Token);
                
                result.TokenId.Should().Be(customJti);
                jwtSecurityToken.Id.Should().Be(customJti);
            }
        }
        
        [Fact]
        public void WithCustomTokenId_ValidTokenIdWasProvided_GeneratedTokenInfoAndTokenValuesShouldBeSetToExpected()
        {
            // Arrange
            string customJti = _faker.Random.AlphaNumeric(12);
            
            // Act
            GeneratedTokenInfo result = BuildSut().WithCustomTokenId(customJti).Build();
            
            // Assert
            using (new AssertionScope())
            {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(result.Token);
                
                result.TokenId.Should().Be(customJti);
                jwtSecurityToken.Id.Should().Be(customJti);
            }
        }

        [Fact]
        public void WithUserIdPayloadData_ValidUserIdWasProvided_TokenValuesShouldBeSetToExpected()
        {
            // Arrange
            string userId = _faker.Random.Guid().ToString();
            
            // Act
            GeneratedTokenInfo result = BuildSut().WithUserIdPayloadData(userId).Build();
            
            // Arrange
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(result.Token);

            jwtSecurityToken.Claims.Should().Contain(claim => claim.Type == PayloadDataKeys.UserId &&
                                                              claim.Value == userId);
        }
        
        [Fact]
        public void WithUserEmailPayloadData_ValidUserEmailWasProvided_TokenValuesShouldBeSetToExpected()
        {
            // Arrange
            string userEmail = _faker.Internet.Email();
            
            // Act
            GeneratedTokenInfo result = BuildSut().WithUserEmailPayloadData(userEmail).Build();
            
            // Arrange
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(result.Token);

            jwtSecurityToken.Claims.Should().Contain(claim => claim.Type == PayloadDataKeys.UserEmail &&
                                                              claim.Value == userEmail);
        }
        
        [Fact]
        public void WithUserRolePayloadData_ValidUserRoleWasProvided_TokenValuesShouldBeSetToExpected()
        {
            // Arrange
            string role = _faker.Lorem.Word();
            
            // Act
            GeneratedTokenInfo result = BuildSut().WithUserRolePayloadData(role).Build();
            
            // Arrange
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(result.Token);

            jwtSecurityToken.Claims.Should().Contain(claim => claim.Type == PayloadDataKeys.UserRole &&
                                                              claim.Value == role);
        }
        
        [Fact]
        public void WithUserRolePayloadData_CalledTwiceWithValidUserRoleProvided_TokenValuesShouldBeSetToExpected()
        {
            // Arrange
            string role1 = _faker.Lorem.Word() + _faker.UniqueIndex;
            string role2 = _faker.Lorem.Word() + _faker.UniqueIndex;

            string[] expected = new[] { role1, role2 };
            
            // Act
            GeneratedTokenInfo result = BuildSut()
                .WithUserRolePayloadData(role1)
                .WithUserRolePayloadData(role2)
                .Build();
            
            // Arrange
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(result.Token);

            jwtSecurityToken.Claims.Where(claim => claim.Type == PayloadDataKeys.UserRole)
                                   .Select(claim => claim.Value)
                                   .Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public void WithUserRolesPayloadData_ValidUserRolesProvided_TokenValuesShouldBeSetToExpected()
        {
            // Arrange
            string[] expected = _faker.Make(5, () => _faker.Lorem.Word() + _faker.UniqueIndex).ToArray();
            
            // Act
            GeneratedTokenInfo result = BuildSut()
                .WithUserRolesPayloadData(expected)
                .Build();
            
            // Arrange
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(result.Token);

            jwtSecurityToken.Claims.Where(claim => claim.Type == PayloadDataKeys.UserRole)
                                   .Select(claim => claim.Value)
                                   .Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public void WithPayloadData_ValidTuplesProvided_TokenValuesShouldBeSetToExpected()
        {
            // Arrange
            (string Key, string Value)[] expected = _faker.Make(5, () =>
            {
                string key = _faker.Lorem.Word() + _faker.UniqueIndex;
                string value = _faker.Random.AlphaNumeric(16);

                return (key, value);
            }).ToArray();
            
            // Act
            GeneratedTokenInfo result = BuildSut()
                .WithPayloadData(expected)
                .Build();
            
            // Arrange
            using (new AssertionScope())
            {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(result.Token);

                foreach ((string Key, string Value) valueTuple in expected)
                {
                    jwtSecurityToken.Claims.Should().Contain(claim => claim.Type == valueTuple.Key &&
                                                                      claim.Value == valueTuple.Value);
                }
            }
        }
        
        private JwtTokenBuilder BuildSut(TimeSpan? lifeTime = null, string secret = null)
        {
            lifeTime ??= _faker.Date.Timespan();
            secret ??= _faker.Random.AlphaNumeric(SecretLength);

            return new JwtTokenBuilder(lifeTime.Value, secret);
        }
    }
}