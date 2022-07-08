using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Kirpichyov.FriendlyJwt.Contracts;
using Kirpichyov.FriendlyJwt.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Kirpichyov.FriendlyJwt.UnitTests
{
    [ExcludeFromCodeCoverage]
    public class MvcBuilderExtensionsTests
    {
        private readonly Faker _faker;
        
        public MvcBuilderExtensionsTests()
        {
            _faker = new Faker();
        }
        
        [Fact]
        public void AddFriendlyJwtAuthentication_FullConfigurationProvided_TokenValidationParametersShouldBeRegistered()
        {
            // Arrange
            var expectedConfiguration = new JwtAuthConfiguration()
            {
                Secret = _faker.Random.AlphaNumeric(32),
                Audience = _faker.Internet.Url(),
                Issuer = _faker.Internet.Url(),
                RequireHttpsMetadata = _faker.Random.Bool(),
                SecurityAlgorithm = _faker.PickRandom(SecurityAlgorithms.HmacSha256Signature, 
                                                      SecurityAlgorithms.HmacSha384Signature, 
                                                      SecurityAlgorithms.HmacSha512Signature)
            };
            
            var services = new ServiceCollection();

            void SetupDelegate(JwtAuthConfiguration configuration)
            {
                configuration.Secret = expectedConfiguration.Secret;
                configuration.Audience = expectedConfiguration.Audience;
                configuration.Issuer = expectedConfiguration.Issuer;
                configuration.RequireHttpsMetadata = expectedConfiguration.RequireHttpsMetadata;
                configuration.SecurityAlgorithm = expectedConfiguration.SecurityAlgorithm;
            }
            
            // Act
            services.AddMvc().AddFriendlyJwtAuthentication(SetupDelegate);
            var provider = services.BuildServiceProvider();

            // Assert
            using (new AssertionScope())
            {
                var service = provider.GetRequiredService<ITokenValidationParametersProvider>();
                service.Should().NotBeNull();
                service.Value.Should().BeEquivalentTo(new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(expectedConfiguration.Secret)),
                    ValidAlgorithms = new[] { expectedConfiguration.SecurityAlgorithm },
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = expectedConfiguration.HasIssuer,
                    ValidateAudience = expectedConfiguration.HasAudience,
                    ValidIssuer = expectedConfiguration.HasIssuer ? expectedConfiguration.Issuer : null,
                    ValidAudience = expectedConfiguration.HasAudience ? expectedConfiguration.Audience : null
                });
            }
        }
        
        [Fact]
        public void AddFriendlyJwtAuthentication_ConfigurationProvidedWithInvalidSecret_ShouldThrowArgumentException()
        {
            // Arrange
            var expectedConfiguration = new JwtAuthConfiguration()
            {
                Secret = string.Empty
            };
            
            var services = new ServiceCollection();

            void SetupDelegate(JwtAuthConfiguration configuration)
            {
                configuration.Secret = expectedConfiguration.Secret;
            }
            
            // Act
            Func<IMvcBuilder> func = () => services.AddMvc().AddFriendlyJwtAuthentication(SetupDelegate);

            // Assert
            func.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void AddFriendlyJwtAuthentication_ConfigurationProvidedWithInvalidAlgorithm_ShouldThrowArgumentException()
        {
            // Arrange
            var expectedConfiguration = new JwtAuthConfiguration()
            {
                Secret = _faker.Random.AlphaNumeric(32),
                SecurityAlgorithm = string.Empty
            };
            
            var services = new ServiceCollection();

            void SetupDelegate(JwtAuthConfiguration configuration)
            {
                configuration.Secret = expectedConfiguration.Secret;
                configuration.SecurityAlgorithm = expectedConfiguration.SecurityAlgorithm;
            }
            
            // Act
            Func<IMvcBuilder> func = () => services.AddMvc().AddFriendlyJwtAuthentication(SetupDelegate);

            // Assert
            func.Should().ThrowExactly<ArgumentException>();
        }
    }
}