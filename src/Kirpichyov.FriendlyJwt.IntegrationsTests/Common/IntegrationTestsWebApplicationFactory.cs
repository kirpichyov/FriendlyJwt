using System.IO;
using Kirpichyov.FriendlyJwt.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kirpichyov.FriendlyJwt.IntegrationsTests.Common;

public sealed class IntegrationTestsWebApplicationFactory : WebApplicationFactory<IntegrationTestsWebAppEntrypoint>
{
    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseEnvironment("Test")
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.AddFriendlyJwt();
                    services.AddControllers()
                        .AddFriendlyJwtAuthentication(configuration =>
                        {
                            configuration.Audience = JwtOptions.Audience;
                            configuration.Issuer = JwtOptions.Issuer;
                            configuration.Secret = JwtOptions.Secret;
                        });
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();

                    app.UseAuthentication();
                    app.UseAuthorization();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });
            });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseContentRoot(Directory.GetCurrentDirectory());
    }
}