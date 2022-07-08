using System;
using System.Text;
using Kirpichyov.FriendlyJwt.Constants;
using Kirpichyov.FriendlyJwt.Contracts;
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
                ValidAlgorithms = new[] { authConfiguration.SecurityAlgorithm },
                ClockSkew = TimeSpan.Zero
            };

            tokenValidationParameters.ValidateIssuer = authConfiguration.HasIssuer;
            if (tokenValidationParameters.ValidateIssuer)
            {
                tokenValidationParameters.ValidIssuer = authConfiguration.Issuer;
            }

            tokenValidationParameters.ValidateAudience = authConfiguration.HasAudience;
            if (tokenValidationParameters.ValidateAudience)
            {
                tokenValidationParameters.ValidAudience = authConfiguration.Audience;
            }

            postSetupDelegate?.Invoke(tokenValidationParameters);
            mvcBuilder.Services.AddSingleton<ITokenValidationParametersProvider, TokenValidationParametersProvider>(
                _ => new TokenValidationParametersProvider(tokenValidationParameters)
            );

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
                    options.TokenValidationParameters.RoleClaimType = PayloadDataKeys.UserRole;
                    options.TokenValidationParameters.NameClaimType = PayloadDataKeys.UserName;
                });

            return mvcBuilder;
        }

        private static void ValidateJwtAuthConfigurationAndThrow(JwtAuthConfiguration authConfiguration)
        {
            if (string.IsNullOrWhiteSpace(authConfiguration.Secret))
            {
                throw new ArgumentException("Secret can't be null or empty.", nameof(authConfiguration.Secret));
            }

            if (string.IsNullOrWhiteSpace(authConfiguration.SecurityAlgorithm))
            {
                throw new ArgumentException("Security algorithm can't be null or empty.");
            }
        }
    }
}