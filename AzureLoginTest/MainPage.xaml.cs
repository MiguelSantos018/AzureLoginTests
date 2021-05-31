using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Prompt = Microsoft.Identity.Client.Prompt;
using RestSharp;
using Newtonsoft.Json;
using AzureLoginTest.Models;
using System.IdentityModel.Tokens.Jwt;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AzureLoginTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    //Set the scope for API call to user.read
    public sealed partial class MainPage : Page
    {

        //Set the scope for API call to user.read
        private string[] scopes = new string[] { "user.read" };

        // The MSAL Public client app
        private static IPublicClientApplication PublicClientApp;
        private static IPublicClientApplication TST;
        private static IPublicClientApplication Dev04;

        private static string MSGraphURL = "https://graph.microsoft.com/v1.0/";
        private AuthenticationResult authResult;

        private string TST04Token;
        private string Dev04Token;

        public MainPage()
        {
            this.InitializeComponent();

            PublicClientApplicationOptions options = new PublicClientApplicationOptions()
            {
                ClientId = "616273da-2291-4317-ab85-dbc77fa13ff6",
                TenantId = "f9a47b46-b524-421a-b61b-49a806ba758b"
            };

            PublicClientApp = PublicClientApplicationBuilder
                .CreateWithApplicationOptions(options)
                .WithUseCorporateNetwork(false)
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .Build();

            Dev04 = PublicClientApplicationBuilder
                        .Create("e7a3379d-6ae1-405e-8ba5-42eab52f7758")
                        .WithAuthority(new Uri("https://login.windows.net/farfetch.com"))
                        .WithUseCorporateNetwork(false)
                        .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                        .Build();

            TST = PublicClientApplicationBuilder
                        .Create("f6226650-7ad4-472d-87ba-01529dcef71a")
                        .WithAuthority(new Uri("https://login.windows.net/farfetch.com"))
                        .WithUseCorporateNetwork(false)
                        .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                        .Build();
        }

        /// <summary>
        /// Call AcquireTokenAsync - to acquire a token requiring user to sign in
        /// </summary>
        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Sign in user using MSAL and obtain an access token for Microsoft Graph
                GraphServiceClient graphClient = await SignInAndInitializeGraphServiceClient(scopes);

                // Call the /me endpoint of Graph
                User graphUser = await graphClient.Me.Request().GetAsync();

                // Go back to the UI thread to make changes to the UI
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResultText.Text = "Display Name: " + graphUser.DisplayName + "\nBusiness Phone: " + graphUser.BusinessPhones.FirstOrDefault()
                                      + "\nGiven Name: " + graphUser.GivenName + "\nid: " + graphUser.Id
                                      + "\nUser Principal Name: " + graphUser.UserPrincipalName;
                    DisplayBasicTokenInfo(authResult);
                    this.SignOutButton.Visibility = Visibility.Visible;
                });
            }
            catch (MsalException msalEx)
            {
                await DisplayMessageAsync($"Error Acquiring Token:{System.Environment.NewLine}{msalEx}");
            }
            catch (Exception ex)
            {
                await DisplayMessageAsync($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                return;
            }
        }
        /// <summary>
        /// Signs in the user and obtains an access token for Microsoft Graph
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns> Access Token</returns>
        private async Task<string> SignInUserAndGetTokenUsingMSAL(string[] scopes, string Option = "BrownsTest")
        {
            string Token = string.Empty;
            this.authResult = null;

            switch (Option)
            {
                case "BrownsTest":
                    // It's good practice to not do work on the UI thread, so use ConfigureAwait(false) whenever possible.
                    IEnumerable<IAccount> accounts = await PublicClientApp.GetAccountsAsync().ConfigureAwait(false);
                    IAccount firstAccount = accounts.FirstOrDefault();
                    AuthenticationResult authResult = null;
                    if (firstAccount != null)
                        authResult = await PublicClientApp.AcquireTokenSilent(scopes, firstAccount)
                                                          .ExecuteAsync();

                    if (authResult == null)
                        authResult = await PublicClientApp.AcquireTokenInteractive(scopes)
                                                          .ExecuteAsync()
                                                          .ConfigureAwait(false);

                    this.authResult = authResult;
                    break;
                case "Dev04":
                    scopes = new string[]
                    {
                        $"https://ffpt-dev-043476774f499ba27edevaos.cloudax.dynamics.com/AX.FullAccess".ToString(),
                        $"https://ffpt-dev-043476774f499ba27edevaos.cloudax.dynamics.com/CustomService.FullAccess".ToString(),
                        $"https://ffpt-dev-043476774f499ba27edevaos.cloudax.dynamics.com/Odata.FullAccess".ToString()
                    };
                    IEnumerable<IAccount> accountsDev = await Dev04.GetAccountsAsync().ConfigureAwait(false);
                    IAccount firstAccountDev = accountsDev.FirstOrDefault();
                    AuthenticationResult dev04AuthResult = null;
                    if (firstAccountDev != null)
                        dev04AuthResult = await Dev04.AcquireTokenSilent(scopes, firstAccountDev)
                                                          .ExecuteAsync();

                    if (dev04AuthResult == null)
                        dev04AuthResult = await Dev04.AcquireTokenInteractive(scopes)
                                                         .ExecuteAsync()
                                                      .ConfigureAwait(false);
                    Token = dev04AuthResult?.AccessToken ?? string.Empty;
                    Dev04Token = dev04AuthResult?.AccessToken ?? string.Empty;

                    this.authResult = dev04AuthResult;
                    break;
                case "TST":
                    scopes = new string[]
                    {
                        $"https://ffd365-tst-04.sandbox.operations.dynamics.com/AX.FullAccess".ToString(),
                        $"https://ffd365-tst-04.sandbox.operations.dynamics.com/CustomService.FullAccess".ToString(),
                        $"https://ffd365-tst-04.sandbox.operations.dynamics.com/Odata.FullAccess".ToString()
                    };
                    IEnumerable<IAccount> accountsTST = await TST.GetAccountsAsync().ConfigureAwait(false);
                    IAccount firstAccountTST = accountsTST.FirstOrDefault();
                    AuthenticationResult tst04AuthResult = null;
                    if (firstAccountTST != null)
                        tst04AuthResult = await TST.AcquireTokenSilent(scopes, firstAccountTST)
                                                          .ExecuteAsync();

                    if (tst04AuthResult == null)
                        tst04AuthResult = await TST.AcquireTokenInteractive(scopes)
                                                               .ExecuteAsync()
                                                      .ConfigureAwait(false);
                    Token = tst04AuthResult?.AccessToken ?? string.Empty;
                    TST04Token = tst04AuthResult?.AccessToken ?? string.Empty;

                    this.authResult = tst04AuthResult;
                    break;
            }
            return Token;
        }

        /// <summary>
        /// Sign in user using MSAL and obtain a token for Microsoft Graph
        /// </summary>
        /// <returns>GraphServiceClient</returns>
        private async Task<GraphServiceClient> SignInAndInitializeGraphServiceClient(string[] scopes, string Option = "BrownsTest")
        {
            GraphServiceClient graphClient = new GraphServiceClient(MSGraphURL,
                new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SignInUserAndGetTokenUsingMSAL(scopes, Option));
                }));

            return await Task.FromResult(graphClient);
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<IAccount> accounts = await PublicClientApp.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                await PublicClientApp.RemoveAsync(firstAccount).ConfigureAwait(false);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResultText.Text = "User has signed out";
                    this.CallGraphButton.Visibility = Visibility.Visible;
                    this.SignOutButton.Visibility = Visibility.Collapsed;
                });
            }
            catch (MsalException ex)
            {
                ResultText.Text = $"Error signing out user: {ex.Message}";
            }
        }

        /// <summary>
        /// Display basic information contained in the token. Needs to be called from the UI thread.
        /// </summary>
        private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoText.Text = "";
            if (authResult != null)
            {
                TokenInfoText.Text += $"Date: {DateTime.Now.ToString("HH:mm:ss")}" + Environment.NewLine;
                TokenInfoText.Text += $"User Name: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
            }
        }

        /// <summary>
        /// Display basic information contained in the token. Needs to be called from the UI thread.
        /// </summary>
        private void DisplayDevBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoDevText.Text = "";
            if (authResult != null)
            {
                TokenInfoDevText.Text += $"Date: {DateTime.Now.ToString("HH:mm:ss")}" + Environment.NewLine;
                TokenInfoDevText.Text += $"User Name: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoDevText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
            }
        }

        /// <summary>
        /// Display basic information contained in the token. Needs to be called from the UI thread.
        /// </summary>
        private void DisplayTSTBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoTSTText.Text = "";
            if (authResult != null)
            {
                TokenInfoTSTText.Text += $"Date: {DateTime.Now.ToString("HH:mm:ss")}" + Environment.NewLine;
                TokenInfoTSTText.Text += $"User Name: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoTSTText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
            }
        }

        /// <summary>
        /// Displays a message in the ResultText. Can be called from any thread.
        /// </summary>
        private async Task DisplayMessageAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    ResultText.Text = message;
                });
        }

        /// <summary>
        /// Displays a message in the ResultText. Can be called from any thread.
        /// </summary>
        private async Task DisplayDevMessageAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    ResultDevText.Text = message;
                });
        }

        /// <summary>
        /// Displays a message in the ResultText. Can be called from any thread.
        /// </summary>
        private async Task DisplayTSTMessageAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    ResultTSTText.Text = message;
                });
        }

        private async void DevButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //// Sign in user using MSAL and obtain an access token for Microsoft Graph
                //GraphServiceClient graphClient = await SignInAndInitializeGraphServiceClient(scopes, "Dev04");

                //// Call the /me endpoint of Graph
                //User graphUser = await graphClient.Me.Request().GetAsync();

                Dev04Token = string.Empty;
                var Token = await SignInUserAndGetTokenUsingMSAL(scopes, "Dev04");

                // Go back to the UI thread to make changes to the UI
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //ResultDevText.Text = "Display Name: " + graphUser.DisplayName + "\nBusiness Phone: " + graphUser.BusinessPhones.FirstOrDefault()
                    //                  + "\nGiven Name: " + graphUser.GivenName + "\nid: " + graphUser.Id
                    //                  + "\nUser Principal Name: " + graphUser.UserPrincipalName;
                    ResultDevText.Text = $"Token:\n{Token}";
                    DisplayDevBasicTokenInfo(authResult);
                    this.SignOutDevButton.Visibility = Visibility.Visible;
                    this.UserInfoDevButton.Visibility = Visibility.Visible;
                });
            }
            catch (MsalException msalEx)
            {
                await DisplayDevMessageAsync($"Error Acquiring Token:{Environment.NewLine}{msalEx}");
            }
            catch (Exception ex)
            {
                await DisplayDevMessageAsync($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                return;
            }
        }

        private async void TSTButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Sign in user using MSAL and obtain an access token for Microsoft Graph
                //GraphServiceClient graphClient = await SignInAndInitializeGraphServiceClient(scopes, "TST");
                // Call the /me endpoint of Graph
                //User graphUser = await graphClient.Me.Request().GetAsync();

                TST04Token = string.Empty;
                var Token = await SignInUserAndGetTokenUsingMSAL(scopes, "TST");

                // Go back to the UI thread to make changes to the UI
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //ResultTSTText.Text = "Display Name: " + graphUser.DisplayName + "\nBusiness Phone: " + graphUser.BusinessPhones.FirstOrDefault()
                    //                  + "\nGiven Name: " + graphUser.GivenName + "\nid: " + graphUser.Id
                    //                  + "\nUser Principal Name: " + graphUser.UserPrincipalName;
                    ResultTSTText.Text = $"Token:\n{Token}";
                    DisplayTSTBasicTokenInfo(authResult);
                    this.SignOutTSTButton.Visibility = Visibility.Visible;
                    this.UserInfoTSTButton.Visibility = Visibility.Visible;
                });
            }
            catch (MsalException msalEx)
            {
                await DisplayTSTMessageAsync($"Error Acquiring Token:{System.Environment.NewLine}{msalEx}");
            }
            catch (Exception ex)
            {
                await DisplayTSTMessageAsync($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                return;
            }
        }

        private async void SignOutButtonTST_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<IAccount> accounts = await TST.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                await TST.RemoveAsync(firstAccount).ConfigureAwait(false);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResultTSTText.Text = "User has signed out";
                    this.SignOutTSTButton.Visibility = Visibility.Collapsed;
                    this.UserInfoTSTButton.Visibility = Visibility.Collapsed;
                });
            }
            catch (MsalException ex)
            {
                ResultText.Text = $"Error signing out user: {ex.Message}";
            }
        }

        private async void SignOutButtonDev_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<IAccount> accounts = await Dev04.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                await Dev04.RemoveAsync(firstAccount).ConfigureAwait(false);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResultDevText.Text = "User has signed out";
                    this.SignOutDevButton.Visibility = Visibility.Collapsed;
                    this.UserInfoDevButton.Visibility = Visibility.Collapsed;
                });
            }
            catch (MsalException ex)
            {
                ResultText.Text = $"Error signing out user: {ex.Message}";
            }
        }

        private async void UserInfoButtonTST_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var actionResult = await GetUserInfo("TST");
                if (actionResult?.Status ?? false)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        ResultTSTText.Text = "User Info Requested";
                        DisplayInfoUser(actionResult, TokenInfoTSTText);
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayTSTMessageAsync($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                return;
            }
        }

        public async Task<ActionStatus> GetUserInfo(string option)
        {
            string Token = await SignInUserAndGetTokenUsingMSAL(scopes, option);
            string ApiUriString = string.Empty;

            switch (option)
            {
                case "Dev04":
                    ApiUriString = "https://ffpt-dev-043476774f499ba27edevaos.cloudax.dynamics.com/api/services/fukbuysheetservicegroup/fukbuysheetservice/";
                    break;
                case "TST":
                    ApiUriString = "https://ffd365-tst-04.sandbox.operations.dynamics.com/api/services/fukbuysheetservicegroup/fukbuysheetservice/";
                    break;
            }

            var handler = new JwtSecurityTokenHandler();
            var tokenJWT = handler.ReadJwtToken(Token);

            Models.Action Data = new Models.Action()
            {
                ApiAction = "getWorker",
                _company = "BRO",
                _networkAlias = tokenJWT?.Payload["upn"]?.ToString()
            };

            var restClient = new RestClient($"{ApiUriString}{Data.ApiAction}");
            restClient.Timeout = -1;

            var request = new RestRequest(Data.ApiMethod);
            request.AddHeader("Authorization", $"Bearer {Token}");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(Data), ParameterType.RequestBody);
            IRestResponse response = restClient.Execute(request);

            return JsonConvert.DeserializeObject<ActionStatus>(response.Content);
        }

        private void DisplayInfoUser(ActionStatus actionStatus, TextBox result)
        {
            result.Text = "";
            if (actionStatus != null)
            {
                result.Text += $"Date: {DateTime.Now.ToString("HH:mm:ss")}" + Environment.NewLine;
                result.Text += $"Personal Number: {actionStatus.PersonalNumber}" + Environment.NewLine;
                result.Text += $"Company: BRO" + Environment.NewLine;
            }
        }

        private async void UserInfoButtonDev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var actionResult = await GetUserInfo("Dev04");
                if (actionResult?.Status ?? false)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        ResultDevText.Text = "User Info Requested";
                        DisplayInfoUser(actionResult, TokenInfoDevText);
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayDevMessageAsync($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                return;
            }
        }
    }
}
