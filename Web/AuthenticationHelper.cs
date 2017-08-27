using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace GrantPreConsentWebApp
{
    internal class AuthenticationHelper
    {
        public static string TokenForUser;

        /// <summary>
        /// Async task to acquire token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> AcquireTokenAsyncForUser()
        {
            return TokenForUser;
        }

        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> InitializeTokenForUser(string authorizationCode)
        {
            AuthenticationContext ac = new AuthenticationContext("https://login.microsoftonline.com/common");
            
            ClientCredential authorizationClientCredential = new ClientCredential(GlobalConstants.BootstrapClientId, GlobalConstants.BootstrapClientSecret);
            
            var userAuthnResult = await ac.AcquireTokenByAuthorizationCodeAsync(authorizationCode,
                new Uri("http://localhost:5000"), authorizationClientCredential);

            TokenForUser = userAuthnResult.AccessToken;

            return TokenForUser;
        }
    }
}
