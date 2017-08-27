//Copyright (c) CodeMoggy. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace GrantPreConsentConsoleApp
{
    /// <summary>
    /// Responsible for signing in a tenant
    /// </summary>
    internal class AuthenticationHelper
    {
        public static string AADTokenForUser;
        public static string GraphTokenForUser;

        /// <summary>
        /// Async task to acquire token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> AcquireAADTokenAsyncForUser()
        {
            if (AADTokenForUser == null)
            {
                var result = await GetTokenForUser(GlobalConstants.AADGraphResourceUrl);
                AADTokenForUser = result.AccessToken;

                Program.WriteInfo($"Hello {result.UserInfo.GivenName} {result.UserInfo.FamilyName}, welcome to the AAD pre-consent console app\n");
            }

            return AADTokenForUser;
        }

        public static async Task<string> AcquireGraphTokenAsyncForUser()
        {
            if (GraphTokenForUser == null)
            {
                var result = await GetTokenForUser(GlobalConstants.MSFTGraphResourceUrl);
                GraphTokenForUser = result.AccessToken;
            }

            return GraphTokenForUser;
        }

        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        private static async Task<AuthenticationResult> GetTokenForUser(string resource)
        {
            var redirectUri = new Uri(GlobalConstants.RedirectUrl);

            AuthenticationContext authenticationContext = new AuthenticationContext(GlobalConstants.AuthString, false);
            AuthenticationResult userAuthnResult = await authenticationContext.AcquireTokenAsync(resource,
                GlobalConstants.BootstrapClientId, redirectUri, new PlatformParameters(PromptBehavior.RefreshSession));

            return userAuthnResult;
        }
    }
}