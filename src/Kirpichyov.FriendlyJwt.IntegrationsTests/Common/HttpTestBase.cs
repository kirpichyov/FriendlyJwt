using System;
using System.Linq;
using System.Net.Http;
using Flurl.Http;
using Kirpichyov.FriendlyJwt.IntegrationsTests.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace Kirpichyov.FriendlyJwt.IntegrationsTests.Common
{
    public abstract class HttpTestBase
    {
        protected readonly TestServer Server;

        protected HttpTestBase()
        {
            var webHostBuilder =
                new WebHostBuilder()
                    .UseEnvironment("Test")
                    .UseStartup<TestStartup>();
            
            Server = new TestServer(webHostBuilder);
        }
        
        protected IFlurlClient GetFlurlClient(HttpClient httpClient, string controller, AuthConfiguration authConfiguration = null)
        {
            var flurlClient = new FlurlClient(httpClient);
            flurlClient.BaseUrl = httpClient.BaseAddress + ApiConstants.RouteBase + controller;

            if (authConfiguration is not null)
            {
                flurlClient.Headers.Add(HeaderNames.Authorization, "Bearer " + GetToken(authConfiguration));
            }
            
            return flurlClient;
        }

        protected string GetToken(AuthConfiguration authConfiguration)
        {
            string secret = authConfiguration.InvalidSecret ? JwtOptions.Secret + "_bad" : JwtOptions.Secret;
            
            var tokenBuilder = new JwtTokenBuilder(authConfiguration.LifeTime, secret)
                .WithIssuer(authConfiguration.InvalidIssuer ? JwtOptions.Issuer + "_bad" : JwtOptions.Issuer)
                .WithAudience(authConfiguration.InvalidAudience ? JwtOptions.Audience + "_bad" : JwtOptions.Audience)
                .WithSecurityAlgorithm(SecurityAlgorithms.HmacSha256Signature);

            if (!string.IsNullOrEmpty(authConfiguration.UserName))
            {
                tokenBuilder.WithUserName(authConfiguration.UserName);
            }
            
            if (authConfiguration.Roles.Any())
            {
                tokenBuilder.WithUserRolesPayloadData(authConfiguration.Roles);
            }

            if (authConfiguration.PayloadData.Any())
            {
                tokenBuilder.WithPayloadData(authConfiguration.PayloadData);
            }

            return tokenBuilder.Build().Token;
        }
        
        protected class AuthConfiguration
        {
            public (string Key, string Value)[] PayloadData { get; init; } = Array.Empty<(string, string)>();
            public string[] Roles { get; init; } = Array.Empty<string>();
            public string UserName { get; init; }
            public bool InvalidSecret { get; init; }
            public bool InvalidIssuer { get; init; }
            public bool InvalidAudience { get; init; }
            public TimeSpan LifeTime { get; set; } = TimeSpan.FromHours(1);

            public AuthConfiguration(params string[] roles)
            {
                Roles = roles;
            }

            public AuthConfiguration()
            {
            }
        }
    }
}