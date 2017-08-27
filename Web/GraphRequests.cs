#region

using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application = Microsoft.Azure.ActiveDirectory.GraphClient.Application;

#endregion

namespace GrantPreConsentWebApp
{
    internal class Requests
    {
        static StringBuilder logBuffer = new StringBuilder();

        private static void Log(string message) {
            Console.WriteLine(message);
            logBuffer.Append(message);
        }

        public static void ClearLog() {
            logBuffer = new StringBuilder();
        }

        public static string GetLog() {
            return logBuffer.ToString();
        }

        public static async Task GrantConsents()
        {

            ActiveDirectoryClient aadClient = null;

            try
            {
                Uri servicePointUri = new Uri(GlobalConstants.AADGraphResourceUrl);
                Uri serviceRoot = new Uri(servicePointUri, GlobalConstants.GraphTenantName);

                // create an Active Directory Client based on a signed in user
                aadClient = await Task.Run(() =>
                {
                    return new ActiveDirectoryClient(serviceRoot,
                        async () => await AuthenticationHelper.AcquireTokenAsyncForUser());
                });

                // The tenant must have service principals to the MS and AAD Graph APIs in order to proceed.
                // These are automatically created (JIT) by Azure AD when consenting to applications that use
                // permissions from those APIs; therefore, ensure your bootstrap app is created with at least
                // 1 permission (e.g. 'Sign in and read user profile') from both AD Graph and MS Graph.
                
                // Since it takes a few seconds for the SPs to be created JIT immediately after consent,
                // we wait 5 seconds for the dust to settle.
                Thread.Sleep(5000);

                var MSFTGraphPrincipal = await GetServicePrincipalAsync(aadClient, GlobalConstants.MSFTGraphAppId);
                if (MSFTGraphPrincipal == null)
                {
                    throw new Exception($"{GlobalConstants.MSFTGraphDisplayName} service principal is missing; please ensure you have consented to the bootstrap application and then try again.");
                }

                var AADGraphPrincipal = await GetServicePrincipalAsync(aadClient, GlobalConstants.AADGraphAppId);
                if (AADGraphPrincipal == null)
                {
                    throw new Exception($"{GlobalConstants.AADGraphDisplayName} service principal is missing; please ensure you have consented to the bootstrap application and then try again.");
                }

                // Add your application permissions here.
                // You should add a new OAuthGrant per service principle (resource API) and application
                List<OAuthGrant> grants = new List<OAuthGrant>() {
                    { new OAuthGrant{ Application = new Application {
                        AppId = "d006b85d-06c4-4324-9b4a-3bedab31e762",
                        DisplayName = "MyDelegateApp" },
                        ResourceServicePrincipal = AADGraphPrincipal,
                        DelegatedPermissions = "User.Read",
                        ApplicationPermissions = new List<string> { }
                    } },
                    { new OAuthGrant{ Application = new Application {
                        AppId = "d006b85d-06c4-4324-9b4a-3bedab31e762",
                        DisplayName = "MyDelegateApp" },
                        ResourceServicePrincipal = MSFTGraphPrincipal,
                        DelegatedPermissions = "Mail.Read",
                        ApplicationPermissions = new List<string> { }
                    } },
                    { new OAuthGrant{ Application = new Application {
                        AppId = "e92aedcf-9385-45db-85d2-b1c1529e9114",
                        DisplayName = "Multiconsent App-Only Permission" },
                        ResourceServicePrincipal = MSFTGraphPrincipal,
                        DelegatedPermissions = "",
                        ApplicationPermissions = new List<string> { "741f803b-c850-494e-b5df-cde7c675a1ca" } // Read and write all users' full profiles
                    } }
                };

                // Ensure the tenant does have a previously created Service Principal for each application.
                // This ensures the tenant consents to the latest scope of permissions required by YOUR AAD application.
                // Note, this is an additional loop to the create loop below because it may be the case that
                // an ApplicationServicePrincipal has permissions belonging more than one ResourceServicePrincipal
                foreach (var grant in grants)
                {
                    await CleanupPermissionGrantsAsync(aadClient, grant);
                };

                // Create a blank service principle, then grant it the required user (delegated) and application permissions
                foreach (var grant in grants)
                {
                    // create a service principal for all apps requiring consent
                    grant.ApplicationServicePrincipal = await CreateServicePrincipalAsync(aadClient, grant.Application);
                    if (grant.ApplicationServicePrincipal == null)
                    {
                        continue;
                    }

                    // pre-consent the permissions for the application
                    await AddDelegatedPermissionsGrantAsync(aadClient, grant);

                    // pre-consent to the application permissions for the application
                    await AddApplicationPermissionsGrantAsync(aadClient, grant);

                    Log($"\nService Principal created: {grant.ApplicationServicePrincipal.DisplayName}\n\tDelegated permissions: {grant.DelegatedPermissions}\n\tApplication permissions: {string.Join(" ", grant.ApplicationPermissions)}\n");
                }

                Log($"\nCompleted at {DateTime.Now.ToUniversalTime()}");
            }
            catch (Exception e)
            {
                Log(string.Format("Acquiring a token failed with the following error: {0}", e.Message));
                //TODO: Implement retry and back-off logic per the guidance given here:http://msdn.microsoft.com/en-us/library/dn168916.aspx
                return;
            }
        }

        #region Applications and Service Principals

        private static async Task CleanupPermissionGrantsAsync(IActiveDirectoryClient client, OAuthGrant grant)
        {
            IServicePrincipal servicePrincipal = await GetServicePrincipalAsync(client, grant.Application.AppId);
            if (servicePrincipal == null)
            {
                Log(string.Format("No existing service principal for app {0}", grant.Application.DisplayName));
                return;
            }
            Log(string.Format("Deleting existing service principal for app {0}", grant.Application.DisplayName));
            await servicePrincipal.DeleteAsync();
        }

        private static async Task AddDelegatedPermissionsGrantAsync(IActiveDirectoryClient client, OAuthGrant grant)
        {
            if (grant.DelegatedPermissions == "")
            {
                return;
            }
            try
            {
                // add the permissions
                await client.Oauth2PermissionGrants.AddOAuth2PermissionGrantAsync(new OAuth2PermissionGrant
                {
                    ClientId = grant.ApplicationServicePrincipal.ObjectId,
                    ConsentType = "AllPrincipals", // all users
                    ResourceId = grant.ResourceServicePrincipal.ObjectId,
                    Scope = grant.DelegatedPermissions,
                    ExpiryTime = new DateTime().AddYears(100) // when the grant expires
                });
            }
            catch (Exception e)
            {
                Log(string.Format("\nError adding Delegated Permissions for {0}: {1}", grant.Application.DisplayName, e.Message));
            }
        }

        private static async Task AddApplicationPermissionsGrantAsync(IActiveDirectoryClient client, OAuthGrant grant)
        {
            var token = await AuthenticationHelper.AcquireTokenAsyncForUser();
            var webClient = new HttpClient();
            webClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            foreach (var graphPermissionId in grant.ApplicationPermissions)
            {
                var uri = $"{GlobalConstants.AADGraphResourceUrl}/{GlobalConstants.GraphTenantName}/servicePrincipals/{grant.ApplicationServicePrincipal.ObjectId}/appRoleAssignments?api-version=1.6";
                string jsonBody = JsonConvert.SerializeObject(new
                {
                    id = graphPermissionId,
                    principalId = grant.ApplicationServicePrincipal.ObjectId,
                    principalType = "ServicePrincipal",
                    resourceId = grant.ResourceServicePrincipal.ObjectId
                });
                var response = await webClient.PostAsync(uri, new StringContent(jsonBody, Encoding.UTF8, "application/json"));
                Log(response.ToString());
            }
        }

        private static async Task<IServicePrincipal> GetServicePrincipalAsync(IActiveDirectoryClient client, string applicationId)
        {
            IPagedCollection<IServicePrincipal> servicePrincipals = null;
            try
            {
                servicePrincipals = await client.ServicePrincipals.ExecuteAsync();
            }
            catch (Exception e)
            {
                Log(string.Format("\nError getting Service Principal {0}", e.Message));
            }

            while (servicePrincipals != null)
            {
                List<IServicePrincipal> servicePrincipalsList = servicePrincipals.CurrentPage.ToList();
                IServicePrincipal servicePrincipal =
                    servicePrincipalsList.FirstOrDefault(x => x.AppId.Equals(applicationId));

                if (servicePrincipal != null)
                {
                    return servicePrincipal;
                }

                servicePrincipals = await servicePrincipals.GetNextPageAsync();
            }

            return null;
        }

        private static async Task<IServicePrincipal> CreateServicePrincipalAsync(IActiveDirectoryClient client, IApplication application)
        {
            IServicePrincipal servicePrincipal = null;

            if (application.AppId != null)
            {
                // does the Service Principal already exist?
                servicePrincipal = await GetServicePrincipalAsync(client, application.AppId);

                // if exists then return the found Service Principal
                if (servicePrincipal != null)
                    return servicePrincipal;

                // if not found then create a Service Principal for this tenant
                servicePrincipal = new ServicePrincipal()
                {
                    AccountEnabled = true,
                    AppId = application.AppId,
                    DisplayName = application.DisplayName
                };

                try
                {
                    await client.ServicePrincipals.AddServicePrincipalAsync(servicePrincipal);
                }
                catch (Exception e)
                {
                    Log(string.Format("\nError creating Service Principal: for {0}: {1}", application.DisplayName, e.Message));
                }
            }
            else
            {
                Log(string.Format("\nRefusing to create Service Principal for {0} because ApplicationID was not supplied", application.DisplayName));
            }


            return servicePrincipal;
        }

        #endregion
    }

    /// <summary>
    /// Object representation of the permissions required for an Application ServicePrincipal and Resource ServicePrincipal
    /// </summary>
    internal class OAuthGrant
    {
        /// <summary>
        /// Application object representing your AAD application. 
        /// You will need the ApplicationId and Display Name
        /// </summary>
        public IApplication Application { get; set; }

        /// <summary>
        /// The Service Principal to which the permission belongs to, e.g Microsoft Graph or AAD Graph
        /// </summary>
        public IServicePrincipal ResourceServicePrincipal { get; set; }

        /// <summary>
        /// The delegated scope or permissions required by YOUR AAD application. 
        /// If you have multiple permissions then delimit them using a space, e.g. "User.Read Mail.Send"
        /// </summary>
        public string DelegatedPermissions { get; set; }

        /// <summary>
        /// The app-only scope or permissions required by YOUR AAD application
        /// These should be represented as GUIDs
        /// </summary>
        public List<string> ApplicationPermissions { get; set; } = new List<string>();

        /// <summary>
        /// The ServicePrincipal object representing YOUR AAD application in the tenant's directory
        /// </summary>
        public IServicePrincipal ApplicationServicePrincipal { get; internal set; }
    }
}