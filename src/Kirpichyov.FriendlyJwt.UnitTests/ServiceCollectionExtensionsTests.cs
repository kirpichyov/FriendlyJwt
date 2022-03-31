using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Kirpichyov.FriendlyJwt.Contracts;
using Kirpichyov.FriendlyJwt.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kirpichyov.FriendlyJwt.UnitTests
{
    [ExcludeFromCodeCoverage]
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddFriendlyJwt_ServicesShouldBeRegistered()
        {
            // Arrange
            var sut = new ServiceCollection();

            // Act
            sut.AddFriendlyJwt();

            // Assert
            using (new AssertionScope())
            {
                sut.Any(service => service.ServiceType == typeof(IJwtTokenReader)).Should().BeTrue();
                sut.Any(service => service.ServiceType == typeof(IJwtTokenVerifier)).Should().BeTrue();
                sut.Any(service => service.ServiceType == typeof(IHttpContextAccessor)).Should().BeTrue();
            }
        }
    }
}