//Copyright (c) CodeMoggy. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace GrantPreConsentConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Request.GrantConsents().Wait();
            Program.WriteInfo($"\nCompleted at {DateTime.Now.ToUniversalTime()} \nPress Any Key to Exit.");
            Console.ReadKey();
        }

        public static string ExtractErrorMessage(Exception exception)
        {
            List<string> errorMessages = new List<string>();


            string tabs = "\n";
            while (exception != null)
            {
                string requestIdLabel = "requestId";
                if (exception is DataServiceClientException &&
                    exception.Message.Contains(requestIdLabel))
                {
                    Dictionary<string, object> odataError =
                        new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(exception.Message);
                    odataError = (Dictionary<string, object>)odataError["odata.error"];
                    errorMessages.Insert(0, "\nRequest ID: " + odataError[requestIdLabel]);
                    errorMessages.Insert(1, "Date: " + odataError["date"]);
                }

                tabs += "    ";
                errorMessages.Add(tabs + exception.Message);
                exception = exception.InnerException;
            }

            return string.Join("\n", errorMessages);
        }

        public static void WriteError(string output, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(output, args);
            Console.ResetColor();
        }

        public static void WriteInfo(string output, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(output, args);
            Console.ResetColor();
        }
    }
}