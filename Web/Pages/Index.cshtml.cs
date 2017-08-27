using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Application = Microsoft.Azure.ActiveDirectory.GraphClient.Application;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace GrantPreConsentWebApp.Pages
{
    public class IndexModel : PageModel
    {
        public async Task OnGet()
        {
            ViewData["BootstrapAppId"] = GlobalConstants.BootstrapClientId;
            ViewData["BootstrapResourceUrl"] = GlobalConstants.AADGraphResourceUrl;
            ViewData["RedirectUrl"] = GlobalConstants.RedirectUrl;
            bool codeWasSupplied = !string.IsNullOrWhiteSpace(this.Request.Query["code"]);
            if (codeWasSupplied) {
                await AuthenticationHelper.InitializeTokenForUser(this.Request.Query["code"]);

                Requests.ClearLog();
                await Requests.GrantConsents();

                ViewData["Output"] = Requests.GetLog();
                ViewData["Name"] = "User";
                ViewData["Files"] = "File List Here";
                ViewData["Users"] = "User List Here";
            }
        }
    }
}
