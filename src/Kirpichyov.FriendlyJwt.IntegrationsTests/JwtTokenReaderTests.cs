using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Flurl.Http;
using Kirpichyov.FriendlyJwt.Constants;
using Kirpichyov.FriendlyJwt.IntegrationsTests.Common;
using Kirpichyov.FriendlyJwt.IntegrationsTests.Constants;
using Xunit;

namespace Kirpichyov.FriendlyJwt.IntegrationsTests
{
    public class JwtTokenReaderTests : HttpTestBase
    {
        private readonly Faker _faker;

        public JwtTokenReaderTests()
        {
            _faker = new Faker();
        }
        
        [Fact]
        public async Task CallProtectedEndpoint_AuthorizedWithMinimalClaims_ResponseShouldBeEquivalentToExpected()
        {
            // Arrange
            using var httpClient = Server.CreateClient();

            var authConfiguration = new AuthConfiguration();
            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthReaderController, authConfiguration);
            
            // Act
            var response = await flurlClient.Request()
                                            .AllowAnyHttpStatus()
                                            .GetJsonAsync<AuthReaderResponse>();
            
            // Assert
            response.Should().NotBeNull();
        }
        
        [Fact]
        public async Task CallProtectedEndpoint_AuthorizedWithPropertyClaims_ResponseShouldBeEquivalentToExpected()
        {
            // Arrange
            var expected = new
            {
                IsLoggedIn = true,
                UserId = _faker.Random.Guid().ToString(),
                UserEmail = _faker.Person.Email,
                UserName = _faker.Person.UserName,
                CustomClaim = "custom_value",
                Roles = new [] { AuthConstants.Roles.Admin, AuthConstants.Roles.User }
            };
            
            using var httpClient = Server.CreateClient();

            var authConfiguration = new AuthConfiguration()
            {
                UserName = expected.UserName,
                PayloadData = new []
                {
                    (PayloadDataKeys.UserId, expected.UserId),
                    (PayloadDataKeys.UserEmail, expected.UserEmail),
                    ("custom_claim", expected.CustomClaim),
                },
                Roles = expected.Roles
            };
            
            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthReaderController, authConfiguration);
            
            // Act
            var response = await flurlClient.Request()
                                            .AllowAnyHttpStatus()
                                            .GetJsonAsync<AuthReaderResponse>();
            
            // Assert
            using (new AssertionScope())
            {
                response.Should().NotBeNull();
                response.Should().BeEquivalentTo(expected);
            }
            
        }
    }
}