using System;
using System.Text;
using Kirpichyov.FriendlyJwt.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Kirpichyov.FriendlyJwt.DependencyInjection
{
    public static class MvcBuilderExtensions
    {
        /// <summary>
        /// Configures the JWT authentication.
        /// </summary>
        public static IMvcBuilder AddFriendlyJwtAuthentication(
            this IMvcBuilder mvcBuilder,
            Action<JwtAuthConfiguration> setupDelegate,
            Action<TokenValidationParameters> postSetupDelegate = null)
        {
            var authConfiguration = new JwtAuthConfiguration();
            setupDelegate?.Invoke(authConfiguration);

            ValidateJwtAuthConfigurationAndThrow(authConfiguration);
            
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                RequireExpirationTime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authConfiguration.Secret)),
                ValidAlgorithms = new[] { TokenValidation.HmacSha256 },
                ClockSkew = TimeSpan.Zero
            };

            if (authConfiguration.HasIssuer)
            {
                tokenValidationParameters.ValidateIssuer = true;
                tokenValidationParameters.ValidIssuer = authConfiguration.Issuer;
            }
            if (authConfiguration.HasAudience)
            {
                tokenValidationParameters.ValidateAudience = true;
                tokenValidationParameters.ValidAudience = authConfiguration.Audience;
            }

            postSetupDelegate?.Invoke(tokenValidationParameters);
            mvcBuilder.Services.AddSingleton(tokenValidationParameters);
            
            mvcBuilder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = authConfiguration.RequireHttpsMetadata;
                    options.SaveToken = true;
                    options.TokenValidationParameters = tokenValidationParameters;
                });

            return mvcBuilder;
        }

        private static void ValidateJwtAuthConfigurationAndThrow(JwtAuthConfiguration authConfiguration)
        {
            if (string.IsNullOrWhiteSpace(authConfiguration.Secret))
            {
                throw new ArgumentException("Secret can't be null or empty.", nameof(authConfiguration.Secret));
            }
        }
    }
}