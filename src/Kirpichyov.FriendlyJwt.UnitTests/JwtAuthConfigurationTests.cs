﻿using System;
using System.Diagnostics.CodeAnalysis;
using Bogus;
using FluentAssertions;
using Kirpichyov.FriendlyJwt.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Kirpichyov.FriendlyJwt.UnitTests
{
    [ExcludeFromCodeCoverage]
    public class JwtAuthConfigurationTests
    {
        private readonly Faker _faker;

        public JwtAuthConfigurationTests()
        {
            _faker = new Faker();
        }

        [Fact]
        public void CreateFromOptionsObject_PassedConfigurationIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action func = () => JwtAuthConfiguration.CreateFromOptionsObject(null);

            // Assert
            func.Should().ThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public void CreateFromOptionsObject_ConfigurationWithAllPropertiesPassed_ShouldBeEquivalentToExpected()
        {
            // Arrange
            var configuration = new TestConfiguration1()
            {
                Issuer = _faker.Internet.Url(),
                Audience = _faker.Internet.Url(),
                Secret = _faker.Random.Guid().ToString(),
                SecurityAlgorithm = SecurityAlgorithms.HmacSha256,
                RequireHttpsMetadata = true
            };
            
            // Act
            var result = JwtAuthConfiguration.CreateFromOptionsObject(configuration);

            // Assert
            result.Should().BeEquivalentTo(configuration);
        }
        
        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("True", true)]
        [InlineData("False", false)]
        [InlineData(null, false)]
        [InlineData("other", false)]
        public void CreateFromOptionsObject_ConfigurationWithBoolAsStringPropertyPassed_ShouldBeEquivalentToExpected(string value, bool expected)
        {
            // Arrange
            var configuration = new TestConfiguration2()
            {
                RequireHttpsMetadata = value
            };
            
            // Act
            var result = JwtAuthConfiguration.CreateFromOptionsObject(configuration);

            // Assert
            result.RequireHttpsMetadata.Should().Be(expected);
        }
        
        [Fact]
        public void Bind_PassedConfigurationIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action func = () => new JwtAuthConfiguration().Bind(null);

            // Assert
            func.Should().ThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public void Bind_ConfigurationWithAllPropertiesPassed_ShouldBeEquivalentToExpected()
        {
            // Arrange
            var objectToBind = new TestConfiguration1()
            {
                Issuer = _faker.Internet.Url(),
                Audience = _faker.Internet.Url(),
                Secret = _faker.Random.Guid().ToString(),
                SecurityAlgorithm = SecurityAlgorithms.HmacSha256,
                RequireHttpsMetadata = true
            };

            var configuration = new JwtAuthConfiguration();

            // Act
            configuration.Bind(objectToBind);

            // Assert
            configuration.Should().BeEquivalentTo(objectToBind);
        }
        
        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("True", true)]
        [InlineData("False", false)]
        [InlineData(null, false)]
        [InlineData("other", false)]
        public void Bind_ConfigurationWithBoolAsStringPropertyPassed_ShouldBeEquivalentToExpected(string value, bool expected)
        {
            // Arrange
            var objectToBind = new TestConfiguration2()
            {
                RequireHttpsMetadata = value
            };
            
            var configuration = new JwtAuthConfiguration();

            // Act
            configuration.Bind(objectToBind);

            // Assert
            configuration.RequireHttpsMetadata.Should().Be(expected);
        }

        private record TestConfiguration1
        {
            public string Issuer { get; init; }
            public string Audience { get; init; }
            public string Secret { get; init; }
            public string SecurityAlgorithm { get; init; }
            public bool RequireHttpsMetadata { get; init; }
        }
        
        private record TestConfiguration2
        {
            public string RequireHttpsMetadata { get; init; }
        }
    }
}