using System;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;

namespace Kirpichyov.FriendlyJwt.DependencyInjection
{
    public class JwtAuthConfiguration
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Secret { get; set; }
        public string SecurityAlgorithm { get; set; } = SecurityAlgorithms.HmacSha256;
        public bool RequireHttpsMetadata { get; set; }

        public bool HasIssuer => !string.IsNullOrWhiteSpace(Issuer);
        public bool HasAudience => !string.IsNullOrWhiteSpace(Audience);

        /// <summary>
        /// Binds the options object to <see cref="JwtAuthConfiguration"/>.
        /// </summary>
        /// <param name="optionsObject">Object to obtain configuration from.</param>
        /// <typeparam name="TOptions">Configuration source object type.</typeparam>
        /// <returns>Created <see cref="JwtAuthConfiguration"/>.</returns>
        /// <exception cref="ArgumentNullException">In case if <paramref name="optionsObject"/> is null.</exception>
        /// <remarks>The only public properties can be used for binding.</remarks>
        public static JwtAuthConfiguration Bind<TOptions>(TOptions optionsObject)
            where TOptions : class
        {
            string GetCurrentValueAsString(PropertyInfo propertyInfo)
            {
                return propertyInfo.GetValue(optionsObject)?.ToString();
            }

            bool GetCurrentValueAsBool(PropertyInfo propertyInfo)
            {
                var rawValue = propertyInfo.GetValue(optionsObject);

                if (rawValue is null)
                {
                    return false;
                }

                if (rawValue is bool boolean)
                {
                    return boolean;
                }
                
                if (bool.TryParse(rawValue.ToString(), out var convertedBoolean))
                {
                    return convertedBoolean;
                }

                return false;
            }

            if (optionsObject is null)
            {
                throw new ArgumentNullException(nameof(optionsObject));
            }
            
            var getters = optionsObject.GetType().GetProperties();
            var jwtConfiguration = new JwtAuthConfiguration();

            foreach (var propertyInfo in getters)
            {
                switch (propertyInfo.Name)
                {
                    case nameof(Issuer):
                        jwtConfiguration.Issuer = GetCurrentValueAsString(propertyInfo);
                        break;
                    case nameof(Audience):
                        jwtConfiguration.Audience = GetCurrentValueAsString(propertyInfo);
                        break;
                    case nameof(Secret):
                        jwtConfiguration.Secret = GetCurrentValueAsString(propertyInfo);
                        break;
                    case nameof(SecurityAlgorithm):
                        jwtConfiguration.SecurityAlgorithm = GetCurrentValueAsString(propertyInfo);
                        break;
                    case nameof(RequireHttpsMetadata):
                        jwtConfiguration.RequireHttpsMetadata = GetCurrentValueAsBool(propertyInfo);
                        break;
                }
            }

            return jwtConfiguration;
        }
    }
}