using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using AskExtension.Properties;
using Extension.StackOverflow.Common;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;

namespace AskExtension.Core
{
    [Export]
    class AuthenticationService
    {
        [Import]
        private IAuthentication _authentication;

        private string _collectionPath = @"AskExtension";

        private string GetKey()
        {
            var encryptedToken = GetTokenFromSettings();

            if (encryptedToken == null)
                return "";
            var decryptedToken = System.Text.Encoding.UTF8.GetString(ProtectedData.Unprotect(encryptedToken, null,DataProtectionScope.CurrentUser));
            return decryptedToken;
        }

        private byte[] GetTokenFromSettings()
        {
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var userSettingsStore = settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);

            if (!userSettingsStore.CollectionExists(_collectionPath))
                return null;
            var encryptedToken = userSettingsStore.GetMemoryStream(_collectionPath, "ApiKey").ToArray();
            return encryptedToken;

        }

        private void SetTokenInSettings(byte[] token)
        {
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            if (!userSettingsStore.CollectionExists(_collectionPath))
                userSettingsStore.CreateCollection(_collectionPath);
            userSettingsStore.SetMemoryStream(_collectionPath, "ApiKey", new MemoryStream(token));
        }

        private void SetKey(string token)
        {
            var encryptedToken = ProtectedData.Protect(System.Text.Encoding.UTF8.GetBytes(token), null, DataProtectionScope.CurrentUser);
            SetTokenInSettings(encryptedToken);
        }

        private static string GetUrlToAuthenticate()
        {
            return Settings.Default["StackExchangeApiAuthUrl"] +
                   "?client_id=" + Settings.Default["StackExchangeApiApplicationId"] +
                   "&redirect_uri=" + Settings.Default["StackExchangeApiAuthSuccessUrl"] +
                   "&scope=" + Settings.Default["StackExchangeApiRequestedScope"];
        }

        public Task<bool> Authorize()
        {
            var webBrowserService = ServiceProvider.GlobalProvider.GetService(typeof(IVsWebBrowsingService)) as IVsWebBrowsingService;
            var webBrowserUser = Package.GetGlobalService(typeof(IVsWebBrowserUser)) as IVsWebBrowserUser;
            //https://desktop-vs.open.collab.net/ds/viewMessage.do?dsMessageId=98485&dsForumId=730
            var guidPropertyBrowser = new Guid(ToolWindowGuids80.WebBrowserWindow);
            IVsWebBrowser ppBrowser;
            var isAuthorized = false;
            var didFinishAuth = new Mutex(true);
            
            if (webBrowserService != null)
            {
                IVsWindowFrame ppFrame;
                webBrowserService.CreateWebBrowser(
                    //http://i1.blogs.msdn.com/b/robgruen/archive/2005/11/23/496508.aspx
                    (uint)(__VSCREATEWEBBROWSER.VSCWB_StartCustom | __VSCREATEWEBBROWSER.VSCWB_ForceNew | __VSCREATEWEBBROWSER.VSCWB_AutoShow),
                    ref guidPropertyBrowser,
                    "Test",
                    GetUrlToAuthenticate(),
                    webBrowserUser,
                    out ppBrowser,
                    out ppFrame);

                
                var tmr = new DispatcherTimer
                {
                    Interval = new TimeSpan(0, 0, 0, 0, 300)
                };
                EventHandler timerHandler = (o, args) =>
                {
                    isAuthorized = IntervalHandler(ppBrowser, tmr);
                    if (isAuthorized)
                        didFinishAuth.ReleaseMutex();
                };
                tmr.Tick += timerHandler;
                tmr.Start();
            }
            var task = new Task<bool>(() =>
            {
                using (didFinishAuth)
                {
                    return isAuthorized;
                }
            });
            task.Start();
            return task;
        }

        public bool IsAuthorized()
        {
            return false && !string.IsNullOrWhiteSpace(GetKey());
        }

        private bool IntervalHandler(IVsWebBrowser browser, DispatcherTimer tmr)
        {
            IVsWebBrowsingService webBrowserService = ServiceProvider.GlobalProvider.GetService(typeof(IVsWebBrowsingService)) as IVsWebBrowsingService;
            if (webBrowserService != null)
            {
                IVsWebBrowser ppBrowser;
                IVsWindowFrame ppFrame;
                webBrowserService.GetFirstWebBrowser(Guid.Empty, out ppFrame, out ppBrowser);
                if (ppFrame.IsVisible() == VSConstants.S_FALSE)
                {
                    tmr.Stop();
                    tmr.IsEnabled = false;
                }
                object urlObject;
                browser.GetDocumentInfo((uint)__VSWBDOCINFOINDEX.VSWBDI_DocURL, out urlObject);
                var url = urlObject as string;
                Debug.WriteLine(url);
                if (url != null && url.Contains(ConstValues.Params.AccessToken))
                {
                    SetKey(_authentication.GetTokenBasedOnUrl(url));
                    ppFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
                    VsShellUtilities.ShowMessageBox(
                        ServiceProvider.GlobalProvider,
                        "Plugin has been authorized!",
                        "Success",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    tmr.Stop();
                    tmr.IsEnabled = false;
                    return true;
                }
            }
            return false;
        }
    }
}