using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using Bogus;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Execution;
using Kirpichyov.FriendlyJwt.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Kirpichyov.FriendlyJwt.UnitTests
{
    [ExcludeFromCodeCoverage]
    public class JwtTokenReaderTests
    {
        private readonly Fake<IHttpContextAccessor> _httpContextAccessorFake;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly Faker _faker;
        
        public JwtTokenReaderTests()
        {
            _httpContextAccessorFake = new Fake<IHttpContextAccessor>();
            _faker = new Faker();

            _tokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = PayloadDataKeys.UserName,
                RoleClaimType = PayloadDataKeys.UserRole
            };
        }

        [Fact]
        public void IsLoggedIn_HttpContextWasNotProvided_ShouldBeFalse()
        {
            // Arrange
            _httpContextAccessorFake.CallsTo(accessor => accessor.HttpContext)
                                    .Returns(null);

            var sut = new JwtTokenReader(_httpContextAccessorFake.FakedObject, new TokenValidationParameters());

            // Assert
            using (new AssertionScope())
            {
                sut.IsLoggedIn.Should().BeFalse();
                _httpContextAccessorFake.CallsTo(accessor => accessor.HttpContext)
                                        .MustHaveHappenedOnceExactly();
            }
        }
        
        [Fact]
        public void IsLoggedIn_HttpContextProvidedAndUserHasNoClaims_ShouldBeFalse()
        {
            // Arrange
            JwtTokenReader sut = BuildSut();

            // Assert
            sut.IsLoggedIn.Should().BeFalse();
        }
        
        [Fact]
        public void IsLoggedIn_HttpContextProvidedAndUserHasClaims_ShouldBeTrue()
        {
            // Arrange
            string userId = _faker.Random.Guid().ToString();
            
            JwtTokenReader sut = BuildSut((PayloadDataKeys.UserId, userId));

            // Assert
            sut.IsLoggedIn.Should().BeTrue();
        }

        [Fact]
        public void UserId_HttpContextProvidedAndUserHasUserIdClaim_ShouldBeEqualExpected()
        {
            // Arrange
            string userId = _faker.Random.Guid().ToString();
            
            JwtTokenReader sut = BuildSut((PayloadDataKeys.UserId, userId));

            // Assert
            sut.UserId.Should().Be(userId);
        }
        
        [Fact]
        public void UserId_HttpContextProvidedAndUserHasNoUserIdClaim_ShouldBeNull()
        {
            // Arrange
            string userId = _faker.Random.Guid().ToString();
            
            JwtTokenReader sut = BuildSut(("some_custom_user_id", userId));

            // Assert
            sut.UserId.Should().BeNull();
        }

        [Fact]
        public void UserName_HttpContextProvidedAndUserHasUserEmailClaim_ShouldBeEqualExpected()
        {
            // Arrange
            string userName = _faker.Internet.UserName();
            
            JwtTokenReader sut = BuildSut((PayloadDataKeys.UserName, userName));

            // Assert
            sut.UserName.Should().Be(userName);
        }
        
        [Fact]
        public void UserName_HttpContextProvidedAndUserHasNoUserNameClaim_ShouldBeNull()
        {
            // Arrange
            string userId = _faker.Random.Guid().ToString();

            JwtTokenReader sut = BuildLoggedSut();

            // Assert
            sut.UserName.Should().BeNull();
        }

        [Fact]
        public void UserEmail_HttpContextProvidedAndUserHasUserEmailClaim_ShouldBeEqualExpected()
        {
            // Arrange
            string userEmail = _faker.Internet.Email();
            
            JwtTokenReader sut = BuildSut((PayloadDataKeys.UserEmail, userEmail));

            // Assert
            sut.UserEmail.Should().Be(userEmail);
        }
        
        [Fact]
        public void UserEmail_HttpContextProvidedAndUserHasNoUserEmailClaim_ShouldBeNull()
        {
            // Arrange
            string userEmail = _faker.Internet.Email();
            
            JwtTokenReader sut = BuildLoggedSut();

            // Assert
            sut.UserEmail.Should().BeNull();
        }
        
        [Fact]
        public void UserRoles_HttpContextProvidedAndUserHasUserRoles_ShouldBeEqualExpected()
        {
            // Arrange
            (string Key, string Value)[] expectedUserRoles = _faker.Make(5, () =>
            {
                string key = PayloadDataKeys.UserRole;
                string value = _faker.Random.AlphaNumeric(16);

                return (key, value);
            }).ToArray();
            
            JwtTokenReader sut = BuildSut(expectedUserRoles);

            // Assert
            sut.UserRoles.Except(expectedUserRoles.Select(role => role.Value)).Should().BeEmpty();
        }

        [Fact]
        public void Indexer_HttpContextProvidedAndUserHasExpectedClaim_ShouldBeEqualExpected()
        {
            // Arrange
            string key = _faker.Lorem.Word();
            string value = _faker.Random.AlphaNumeric(12);
            
            JwtTokenReader sut = BuildSut((key, value));

            // Assert
            sut[key].Should().Be(value);
        }
        
        [Fact]
        public void Indexer_HttpContextProvidedAndUserHasNoExpectedClaim_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            string key = _faker.Lorem.Word();

            JwtTokenReader sut = BuildLoggedSut();

            // Act
            Func<string> func = () => sut[key];

            // Assert
            func.Should().ThrowExactly<KeyNotFoundException>();
        }
        
        [Fact]
        public void Indexer_HttpContextProvidedAndUserHasNoClaims_ShouldThrowInvalidOperationException()
        {
            // Arrange
            string key = _faker.Lorem.Word();

            JwtTokenReader sut = BuildSut();

            // Act
            Func<string> func = () => sut[key];

            // Assert
            func.Should().ThrowExactly<InvalidOperationException>();
        }
        
        [Fact]
        public void GetPayloadValue_HttpContextProvidedAndUserHasExpectedClaim_ShouldBeEqualExpected()
        {
            // Arrange
            string key = _faker.Lorem.Word();
            string value = _faker.Random.AlphaNumeric(12);
            
            JwtTokenReader sut = BuildSut((key, value));

            // Assert
            sut.GetPayloadValue(key).Should().Be(value);
        }
        
        [Fact]
        public void GetPayloadValue_HttpContextProvidedAndUserHasNoExpectedClaim_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            string key = _faker.Lorem.Word();

            JwtTokenReader sut = BuildLoggedSut();

            // Act
            Func<string> func = () => sut.GetPayloadValue(key);

            // Assert
            func.Should().ThrowExactly<KeyNotFoundException>();
        }
        
        [Fact]
        public void GetPayloadValue_HttpContextProvidedAndUserHasNoClaims_ShouldThrowInvalidOperationException()
        {
            // Arrange
            string key = _faker.Lorem.Word();

            JwtTokenReader sut = BuildSut();

            // Act
            Func<string> func = () => sut.GetPayloadValue(key);

            // Assert
            func.Should().ThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void GetPayloadValues_HttpContextProvidedAndUserHasExpectedClaims_ShouldBeEqualExpected()
        {
            // Arrange
            string key = _faker.Lorem.Word();
            string[] values = _faker.Make(5, () => _faker.Random.AlphaNumeric(12)).ToArray();

            JwtTokenReader sut = BuildSut(values.Select(value => (key, value)).ToArray());

            // Act
            string[] result = sut.GetPayloadValues(key);

            // Assert
            result.Except(values).Should().BeEmpty();
        }
        
        [Fact]
        public void GetPayloadValues_HttpContextProvidedAndUserHasNoExpectedClaims_ShouldBeEqualExpected()
        {
            // Arrange
            string key = _faker.Lorem.Word();

            JwtTokenReader sut = BuildLoggedSut();

            // Act
            string[] result = sut.GetPayloadValues(key);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetPayloadData_HttpContextProvidedAndUserHasExpectedClaims_ShouldBeEqualExpected()
        {
            // Arrange
            (string Key, string Value)[] payloadData = _faker.Make(5, () =>
            {
                string key = _faker.Lorem.Word()+_faker.UniqueIndex;
                string value = _faker.Random.AlphaNumeric(16);

                return (key, value);
            }).ToArray();
            
            JwtTokenReader sut = BuildSut(payloadData);
            
            // Act
            (string Key, string Value)[] result = sut.GetPayloadData();

            // Assert
            result.Should().BeEquivalentTo(payloadData);
        }
        
        private JwtTokenReader BuildSut(params (string Type, string Value)[] claims)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User.AddIdentity(new ClaimsIdentity(claims.Select(tuple => new Claim(tuple.Type, tuple.Value))));

            _httpContextAccessorFake.CallsTo(accessor => accessor.HttpContext)
                                    .Returns(httpContext);

            return new JwtTokenReader(_httpContextAccessorFake.FakedObject, _tokenValidationParameters);
        }

        private JwtTokenReader BuildLoggedSut() => BuildSut(("some_key", "some_value"));
    }
}