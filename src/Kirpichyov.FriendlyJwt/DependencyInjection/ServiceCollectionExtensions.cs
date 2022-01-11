using Kirpichyov.FriendlyJwt.Contracts;
using Kirpichyov.FriendlyJwt.RefreshTokenUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kirpichyov.FriendlyJwt.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the FriendlyJwt services and <see cref="Microsoft.AspNetCore.Http.IHttpContextAccessor"/>.
        /// </summary>
        public static IServiceCollection AddFriendlyJwt(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.TryAddScoped<IJwtTokenReader, JwtTokenReader>();
            services.TryAddScoped<IJwtTokenVerifier, JwtTokenVerifier>();
            
            return services;
        }
    }
}