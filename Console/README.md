# About this sample
This sample produces a console application (exe) that when run, will launch a prompt for the end-user to sign-in to their Microsoft work account. *Note: the end-user is expected to sign-in using admin accounts only; programmatic consent cannot be performed by non-admin accounts.*

Upon successful authentication, the admin will be prompted to consent to the bootstrap application. Once confirmed, the bootstrap application will access the tenant directory on behalf of the admin user and configure the necessary service principles & their permissions automatically based on the configuration in `GraphRequests.cs`.

# Prerequisites
This console application requires that the .NET Framework (tested with v4.5) and [Visual Studio](https://www.visualstudio.com/downloads/) are installed. Dependencies, such as the ADAL library, will be downloaded automatically on first build.

# Configuration
1. Setup the AAD applications for your solution(s)
2. Add a new Native type AAD application for bootstrap.
    1. Under *Required Permissions* blade, add the *Sign in and read user profile* user permission from both the *Windows Azure Active Directory* and *Microsoft Graph* APIs, as well as the *Access directory as the sign-in user* permission from the *Windows Azure Active Directory* API.
    2. Under the *Reply URLs* blade, add the desired URL (if you plan to test locally, use `http://localhost:5000`).
3. Copy the bootstrap application information (App ID and redirect URI) into the `Constants.cs`
4. Configure the `List<OAuthGrant> grants` variable in `Requests.cs` with one `OAuthGrant` entry for each of your solution's AAD applications that you will need the user to pre-consent to.
    * The documentation for the [AAD Graph](https://msdn.microsoft.com/en-us/library/azure/ad/graph/howto/azure-ad-graph-api-permission-scopes) and the [MS Graph](https://developer.microsoft.com/en-us/graph/docs/concepts/permissions_reference) maintain a comprehensive reference of their permission scope names for delegated (user) permissions
    * Application permissions must be referenced by their GUIDs, which can be found by logging in to the [AAD Graph Explorer](https://graphexplorer.azurewebsites.net/) and executing the following queries:
        * AD Graph: `https://graph.windows.net/myorganization/servicePrincipals?$filter=appId eq '00000002-0000-0000-c000-000000000000'&api-version=1.6`
        * MS Graph: `https://graph.windows.net/myorganization/servicePrincipals?$filter=appId eq '00000003-0000-0000-c000-000000000000'&api-version=1.6`

# Building
1. Open `GrantPreConsentConsoleApp.sln` in Visual Studio
2. Press Run