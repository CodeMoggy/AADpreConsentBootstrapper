# About this sample
This sample produces a .NET Core 2.0 web application that launches on `http://localhost:5000` during local builds. Users visiting the website will be presented with a link redirects the user to a sign-in page for their Microsoft work account. *Note: the end-user is expected to sign-in using admin accounts only; programmatic consent cannot be performed by non-admin accounts.*

Upon successful authentication, the admin will be prompted to consent to the bootstrap application. Once confirmed, the bootstrap application will access the tenant directory on behalf of the admin user and configure the necessary service principles & their permissions automatically based on the configuration in `GraphRequests.cs`.

# Prerequisites
This .NET 2.0 application requires that the [.NET Core 2.0 SDK](https://github.com/dotnet/core/blob/master/release-notes/download-archives/2.0.0-download.md) is installed, as well as either [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2017](https://www.visualstudio.com/downloads/). Dependencies, such as the ADAL library, will be downloaded automatically on first build.

# Configuration
1. Setup the AAD applications for your solution(s)
2. Add a new Web/API type AAD application for bootstrap.
    1. Under *Required Permissions* blade, add the *Sign in and read user profile* user permission from both the *Windows Azure Active Directory* and *Microsoft Graph* APIs, as well as the *Access directory as the sign-in user* permission from the *Windows Azure Active Directory* API.
    2. Under the *Keys* blade, add a new key (client secret) and press Save. Note down its value.
    3. Under the *Reply URLs* blade, add the desired URL (if you plan to test locally, use `http://localhost:5000`).
3. Copy the bootstrap application information (App ID, client secret and reply URL) into the `Constants.cs`
4. Configure the `List<OAuthGrant> grants` variable in `GraphRequests.cs` with one `OAuthGrant` entry for each of your solution's AAD applications that you will need the user to pre-consent to.
    * The documentation for the [AAD Graph](https://msdn.microsoft.com/en-us/library/azure/ad/graph/howto/azure-ad-graph-api-permission-scopes) and the [MS Graph](https://developer.microsoft.com/en-us/graph/docs/concepts/permissions_reference) maintain a comprehensive reference of their permission scope names for delegated (user) permissions
    * Application permissions must be referenced by their GUIDs, which can be found by logging in to the [AAD Graph Explorer](https://graphexplorer.azurewebsites.net/) and executing the following queries:
        * AD Graph: `https://graph.windows.net/myorganization/servicePrincipals?$filter=appId eq '00000002-0000-0000-c000-000000000000'&api-version=1.6`
        * MS Graph: `https://graph.windows.net/myorganization/servicePrincipals?$filter=appId eq '00000003-0000-0000-c000-000000000000'&api-version=1.6`

# Building
1. Open `Web.csproj.sln` in Visual Studio 2017 or VS Code
2. Press Run
