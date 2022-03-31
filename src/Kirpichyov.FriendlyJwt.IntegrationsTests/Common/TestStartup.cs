using Kirpichyov.FriendlyJwt.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Kirpichyov.FriendlyJwt.IntegrationsTests.Common
{
    public class TestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFriendlyJwt();
            services.AddControllers()
                    .AddFriendlyJwtAuthentication(configuration =>
                    {
                        configuration.Audience = JwtOptions.Audience;
                        configuration.Issuer = JwtOptions.Issuer;
                        configuration.Secret = JwtOptions.Secret;
                    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}