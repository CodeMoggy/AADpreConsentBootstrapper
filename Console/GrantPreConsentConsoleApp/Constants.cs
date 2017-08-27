namespace GrantPreConsentConsoleApp
{
    /// <summary>
    /// Contains the AAD information relevant for this bootstrap console application
    /// Replace the the BootstrapClientId and RedirectUrl with your AAD details 
    /// </summary>
    internal class GlobalConstants
    {
        // your information
        public const string BootstrapClientId = "<Add your Application ID here>"; // Native AAD application
        public const string RedirectUrl = "<Add your Redirect URL here>";

        // do not change
        public const string GraphTenantName = "myorganization";
        public const string AuthString = "https://login.microsoftonline.com/common/";
        public const string AADGraphResourceUrl = "https://graph.windows.net";
        public const string MSFTGraphResourceUrl = "https://graph.microsoft.com";
        public const string MSFTGraphDisplayName = "Microsoft Graph";
        public const string AADGraphDisplayName = "Windows Azure Active Directory";
        public const string MSFTGraphAppId = "00000003-0000-0000-c000-000000000000";
        public const string AADGraphAppId = "00000002-0000-0000-c000-000000000000";
    }
}