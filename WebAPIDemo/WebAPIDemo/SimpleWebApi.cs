using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebAPIDemo
{
    static class SimpleWebApi
    {
        //TODO: Uncomment then substitute your correct Dynamics 365 organization service 
        // address for either CRM Online or on-premise (end with a forward-slash).
        private static string serviceUrl = "https://fapple.crm.dynamics.com/";   // CRM Online
        //private static string serviceUrl = "https://<organization name>.<domain name>/";   // CRM IFD
        //private statics string serviceUrl = "http://myserver/myorg/";        // CRM on-premises

        //TODO: For an on-premises deployment, set your organization credentials here. (If
        // online or IFD, you can you can disregard or set to null.)
        private static string userAccount = null;//"<user-account>";  //CRM user account
        private static string domain = null;//"<server-domain>";  //CRM server domain

        //TODO: For CRM Online or IFD deployments, substitute your app registration values  
        // here. (If on-premise, you can disregard or set to null.)
        private static string clientId = "3e78606b-88f1-4f5f-8f7e-1962f55b0d8d";     //e.g. "e5cf0024-a66a-4f16-85ce-99ba97a24bb2"
        //string redirectUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();
        private static string redirectUrl = "http://localhost/WebAPIDemo";  //e.g. "http://localhost/SdkSample"

        static public void Main(string[] args)
        {
            //One message handler for OAuth authentication, and the other for Windows integrated 
            // authentication.  (Assumes that HTTPS protocol only used for CRM Online.)
            HttpMessageHandler messageHandler;
            if (serviceUrl.StartsWith("https://"))
            {
                messageHandler = new OAuthMessageHandler(serviceUrl, clientId, redirectUrl,
                         new HttpClientHandler());
            }
            else
            {
                //Prompt for user account password required for on-premise credentials.  (Better
                // approach is to use the SecureString class here.)
                Console.Write("Please enter the password for account {0}: ", userAccount);
                string password = Console.ReadLine().Trim();
                NetworkCredential credentials = new NetworkCredential(userAccount, password, domain);
                messageHandler = new HttpClientHandler() { Credentials = credentials };
            }
            try
            {
                //Create an HTTP client to send a request message to the CRM Web service.
                using (HttpClient httpClient = new HttpClient(messageHandler))
                {
                    //Specify the Web API address of the service and the period of time each request 
                    // has to execute.
                    httpClient.BaseAddress = new Uri(serviceUrl);
                    httpClient.Timeout = new TimeSpan(0, 2, 0);  //2 minutes

                    //Send the WhoAmI request to the Web API using a GET request. 
                    var response = httpClient.GetAsync("api/data/v8.1/WhoAmI",
                            HttpCompletionOption.ResponseHeadersRead).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        //Get the response content and parse it.
                        JObject body = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        Guid userId = (Guid)body["UserId"];
                        Console.WriteLine("Your system user ID is: {0}", userId);
                    }
                    else
                    {
                        Console.WriteLine("The request failed with a status of '{0}'",
                               response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
                throw;
            }
            finally
            {
                Console.WriteLine("Press <Enter> to exit the program.");
                Console.ReadLine();
            }
        }

        /// <summary> Displays exception information to the console. </summary>
        /// <param name="ex">The exception to output</param>
        private static void DisplayException(Exception ex)
        {
            Console.WriteLine("The application terminated with an error.");
            Console.WriteLine(ex.Message);
            while (ex.InnerException != null)
            {
                Console.WriteLine("\t* {0}", ex.InnerException.Message);
                ex = ex.InnerException;
            }
        }
    }

    /// <summary>
    ///Custom HTTP message handler that uses OAuth authentication thru ADAL.
    /// </summary>
    class OAuthMessageHandler : DelegatingHandler
    {
        private AuthenticationHeaderValue authHeader;

        public OAuthMessageHandler(string serviceUrl, string clientId, string redirectUrl,
                HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            // Obtain the Azure Active Directory Authentication Library (ADAL) authentication context.
            AuthenticationParameters ap = AuthenticationParameters.CreateFromResourceUrlAsync(
                    new Uri(serviceUrl + "api/data/")).Result;
            AuthenticationContext authContext = new AuthenticationContext(ap.Authority, false);
            //Note that an Azure AD access token has finite lifetime, default expiration is 60 minutes.
            var userCredential = new UserPasswordCredential("friyank@Fapple.onmicrosoft.com","dell@123");
            //Task<AuthenticationResult> authResult = authContext.AcquireTokenAsync(serviceUrl, clientId, new Uri(redirectUrl), new PlatformParameters(PromptBehavior.Auto, null));
            Task<AuthenticationResult> authResult = authContext.AcquireTokenAsync(serviceUrl, clientId,userCredential);
            authHeader = new AuthenticationHeaderValue("Bearer", authResult.Result.AccessToken);
        }

        protected override Task<HttpResponseMessage> SendAsync(
                 HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Headers.Authorization = authHeader;
            return base.SendAsync(request, cancellationToken);
        }
    }
}
