using Kirpichyov.FriendlyJwt.RefreshTokenUtilities;

namespace Kirpichyov.FriendlyJwt.Contracts
{
    public interface IJwtTokenVerifier
    {
        /// <summary>
        /// Performs the issued token verification.
        /// </summary>
        /// <param name="token">Issued token.</param>
        /// <param name="tokenIdPayloadKey">Custom token id payload key.</param>
        /// <param name="userIdPayloadKey">Custom user id payload key.</param>
        /// <returns><see cref="JwtVerificationResult"/></returns>
        /// <remarks>
        ///     If custom payload keys was not provided, then default ones from
        ///     <see cref="Constants.PayloadDataKeys"/> will be used.
        /// </remarks>
        JwtVerificationResult Verify(string token, string tokenIdPayloadKey = null, string userIdPayloadKey = null);
    }
}