using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Threading;
using AskExtension.Properties;
using EnvDTE;
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

        private string GetUrlToAuthenticate()
        {
            return Settings.Default["StackExchangeApiAuthUrl"] +
                   "?client_id=" + Settings.Default["StackExchangeApiApplicationId"] +
                   "&redirect_uri=" + Settings.Default["StackExchangeApiAuthSuccessUrl"] +
                   "&scope=" + Settings.Default["StackExchangeApiRequestedScope"];
        }

        public void Authorize()
        {
            var webBrowserService = ServiceProvider.GlobalProvider.GetService(typeof(IVsWebBrowsingService)) as IVsWebBrowsingService;
            IVsWindowFrame ppFrame;
            var webBrowserUser = Package.GetGlobalService(typeof(IVsWebBrowserUser)) as IVsWebBrowserUser;
            //https://desktop-vs.open.collab.net/ds/viewMessage.do?dsMessageId=98485&dsForumId=730
            Guid guidPropertyBrowser = new Guid(ToolWindowGuids80.WebBrowserWindow);
            IVsWebBrowser ppBrowser;
            webBrowserService.CreateWebBrowser(
                //http://i1.blogs.msdn.com/b/robgruen/archive/2005/11/23/496508.aspx
                (uint)(__VSCREATEWEBBROWSER.VSCWB_StartCustom | __VSCREATEWEBBROWSER.VSCWB_ForceNew | __VSCREATEWEBBROWSER.VSCWB_AutoShow),
                ref guidPropertyBrowser,
                "Test",
                GetUrlToAuthenticate(),
                webBrowserUser,
                out ppBrowser,
                out ppFrame);

            var tmr = new DispatcherTimer();
            tmr.Interval = new TimeSpan(0, 0, 0, 0, 300);
            EventHandler timerHandler = (o, args) =>
            {
                webBrowserService = IntervalHandler(ppBrowser, tmr);
            };
            tmr.Tick += timerHandler;
            tmr.Start();
            
        }

        public bool IsAuthorized()
        {
            return !GetKey().IsNullOrEmpty();
        }

        private IVsWebBrowsingService IntervalHandler(IVsWebBrowser browser, DispatcherTimer tmr)
        {
            IVsWebBrowser ppBrowser;
            IVsWindowFrame ppFrame;
            IVsWebBrowsingService webBrowserService = ServiceProvider.GlobalProvider.GetService(typeof(IVsWebBrowsingService)) as IVsWebBrowsingService;
            webBrowserService.GetFirstWebBrowser(Guid.Empty, out ppFrame, out ppBrowser);
            if (ppFrame.IsVisible() == VSConstants.S_FALSE)
            {
                tmr.Stop();
                tmr.IsEnabled = false;
            }
            var user = Package.GetGlobalService(typeof(IVsWebBrowserUser)) as IVsWebBrowserUser;
            object urlObject;
            browser.GetDocumentInfo((uint)__VSWBDOCINFOINDEX.VSWBDI_DocURL, out urlObject);
            var url = urlObject as string;
            Debug.WriteLine(url);
            if (url != null && url.Contains(ConstValues.Params.AccessToken))
            {
                SetKey(_authentication.GetTokenBasedOnUrl(url));
                tmr.Stop();
                tmr.IsEnabled = false;
            }
            return webBrowserService;
        }
    }
}