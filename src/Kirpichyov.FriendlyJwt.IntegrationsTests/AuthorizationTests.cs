using System;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl.Http;
using Kirpichyov.FriendlyJwt.IntegrationsTests.Common;
using Kirpichyov.FriendlyJwt.IntegrationsTests.Constants;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Kirpichyov.FriendlyJwt.IntegrationsTests
{
    public class AuthorizationTests : HttpTestBase
    {

        [Fact]
        public async Task CallAnonymousEndpoint_Unauthorized_ResponseCodeShouldBe200()
        {
            // Arrange
            using var httpClient = Server.CreateClient();
            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthController);
            
            // Act
            var response = await flurlClient.Request(ApiConstants.Auth.Anonymous)
                                            .AllowAnyHttpStatus()
                                            .GetAsync();
            
            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
        
        [Fact]
        public async Task CallProtectedEndpoint_Unauthorized_ResponseCodeShouldBe401()
        {
            // Arrange
            using var httpClient = Server.CreateClient();
            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthController);
            
            // Act
            var response = await flurlClient.Request(ApiConstants.Auth.Protected)
                                            .AllowAnyHttpStatus()
                                            .PostAsync();
            
            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
        
        [Fact]
        public async Task CallProtectedEndpoint_Authorized_ResponseCodeShouldBe200()
        {
            // Arrange
            using var httpClient = Server.CreateClient();

            var authConfiguration = new AuthConfiguration();
            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthController, authConfiguration);
            
            // Act
            var response = await flurlClient.Request(ApiConstants.Auth.Protected)
                                            .AllowAnyHttpStatus()
                                            .PostAsync();
            
            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
        
        [Fact]
        public async Task CallProtectedEndpoint_AuthorizedWithInvalidIssuer_ResponseCodeShouldBe401()
        {
            // Arrange
            using var httpClient = Server.CreateClient();

            var authConfiguration = new AuthConfiguration()
            {
                InvalidIssuer = true
            };

            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthController, authConfiguration);
            
            // Act
            var response = await flurlClient.Request(ApiConstants.Auth.Protected)
                .AllowAnyHttpStatus()
                .PostAsync();
            
            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
        
        [Fact]
        public async Task CallProtectedEndpoint_AuthorizedWithInvalidAudience_ResponseCodeShouldBe401()
        {
            // Arrange
            using var httpClient = Server.CreateClient();

            var authConfiguration = new AuthConfiguration()
            {
                InvalidAudience = true
            };

            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthController, authConfiguration);
            
            // Act
            var response = await flurlClient.Request(ApiConstants.Auth.Protected)
                .AllowAnyHttpStatus()
                .PostAsync();
            
            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
        
        [Fact]
        public async Task CallProtectedEndpoint_AuthorizedWithInvalidSecret_ResponseCodeShouldBe401()
        {
            // Arrange
            using var httpClient = Server.CreateClient();

            var authConfiguration = new AuthConfiguration()
            {
                InvalidSecret = true
            };

            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthController, authConfiguration);
            
            // Act
            var response = await flurlClient.Request(ApiConstants.Auth.Protected)
                .AllowAnyHttpStatus()
                .PostAsync();
            
            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
        
        [Fact]
        public async Task CallProtectedEndpoint_AuthorizedWithExpiredToken_ResponseCodeShouldBe401()
        {
            // Arrange
            using var httpClient = Server.CreateClient();

            var authConfiguration = new AuthConfiguration()
            {
                LifeTime = TimeSpan.FromSeconds(1)
            };
  
            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthController, authConfiguration);

            await Task.Delay(1200);
            
            // Act
            var response = await flurlClient.Request(ApiConstants.Auth.Protected)
                .AllowAnyHttpStatus()
                .PostAsync();
            
            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
        
        [Fact]
        public async Task CallProtectedAdminEndpoint_Unauthorized_ResponseCodeShouldBe401()
        {
            // Arrange
            using var httpClient = Server.CreateClient();
            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthController);
            
            // Act
            var response = await flurlClient.Request(ApiConstants.Auth.ProtectedAdmin)
                                            .AllowAnyHttpStatus()
                                            .PostAsync();
            
            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
        
        [Fact]
        public async Task CallProtectedAdminEndpoint_AuthorizedAsUser_ResponseCodeShouldBe403()
        {
            // Arrange
            using var httpClient = Server.CreateClient();
            var authConfiguration = new AuthConfiguration(AuthConstants.Roles.User);
            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthController, authConfiguration);
            
            // Act
            var response = await flurlClient.Request(ApiConstants.Auth.ProtectedAdmin)
                                            .AllowAnyHttpStatus()
                                            .PostAsync();
            
            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async Task CallProtectedAdminEndpoint_AuthorizedAsAdmin_ResponseCodeShouldBe200()
        {
            // Arrange
            using var httpClient = Server.CreateClient();
            var authConfiguration = new AuthConfiguration(AuthConstants.Roles.Admin);
            var flurlClient = GetFlurlClient(httpClient, ApiConstants.Controllers.AuthController, authConfiguration);
            
            // Act
            var response = await flurlClient.Request(ApiConstants.Auth.ProtectedAdmin)
                                            .AllowAnyHttpStatus()
                                            .PostAsync();
            
            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }
}