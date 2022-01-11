using Kirpichyov.FriendlyJwt.Constants;

namespace Kirpichyov.FriendlyJwt.Contracts
{
    /// <summary>
    /// Allows to retrieve the payload data stored in token.
    /// </summary>
    public interface IJwtTokenReader
    {
        /// <summary>
        /// Determines if the user is logged in.
        /// </summary>
        public bool IsLoggedIn { get; }
        
        /// <summary>
        /// Retrieves the user id using the default key <see cref="PayloadDataKeys.UserId"/>.
        /// If key is not present, property will contain the null value.
        /// </summary>
        public string UserId { get; }
        
        /// <summary>
        /// Retrieves the user email using the default key <see cref="PayloadDataKeys.UserEmail"/>.
        /// If key is not present, property will contain the null value.
        /// </summary>
        public string UserEmail { get; }
        
        /// <summary>
        /// Retrieves the user roles using the default key <see cref="PayloadDataKeys.UserRole"/>.
        /// If key is not present, property will contain the empty array.
        /// </summary>
        public string[] UserRoles { get; }
        
        /// <summary>
        /// Allows to retrieve the value from payload section.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <returns>Data value.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        ///     In case if payload does not contain the provided key.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     In case if user is not authenticated.
        /// </exception>
        public string GetPayloadValue(string key);
        
        /// <summary>
        /// Allows to retrieve the all values from payload section.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <returns>Data values if key is present, otherwise - empty array.</returns>
        /// <exception cref="System.InvalidOperationException">
        ///     In case if user is not authenticated.
        /// </exception>
        public string[] GetPayloadValues(string key);
        
        /// <summary>
        /// Allows to retrieve the value from payload section.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <returns>Data value or null (if key is not present).</returns>
        /// <exception cref="System.InvalidOperationException">
        ///     In case if user is not authenticated.
        /// </exception>
        public string GetPayloadValueOrDefault(string key);
        
        /// <summary>
        /// Allows to retrieve the all values from payload section.
        /// </summary>
        /// <returns>Tuple with payload data.</returns>
        /// <exception cref="System.InvalidOperationException">
        ///     In case if user is not authenticated.
        /// </exception>
        public (string Key, string Value)[] GetPayloadData();
        
        /// <summary>
        /// Indexer that allows to retrieve the value from payload section.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <exception cref="System.InvalidOperationException">
        ///     In case if user is not authenticated.
        /// </exception>
        public string this[string key] { get; }
    }
}