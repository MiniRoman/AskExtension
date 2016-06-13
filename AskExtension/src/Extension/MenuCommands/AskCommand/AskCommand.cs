//------------------------------------------------------------------------------
// <copyright file="AskCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Timers;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = System.IServiceProvider;

namespace RallyExtension.MenuCommands.AskCommand
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AskCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f22621ca-53ba-4956-b3f6-5d6268020d5b");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="AskCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private AskCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                //var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);

                var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += menuCommand_BeforeQueryStatus;

                commandService.AddCommand(menuItem);
            }

            //IVsTextManager txtMgr = (IVsTextManager)this.ServiceProvider.GetService(typeof(SVsTextManager));
            //IVsTextView vTextView = null;
            //int mustHaveFocus = 1;
            //txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);
            //IVsUserData userData = vTextView as IVsUserData;
            //if (userData == null)
            //{
            //    return null;
            //}
            //else
            //{

            //    IWpfTextViewHost viewHost;
            //    object holder;
            //    Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
            //    userData.GetData(ref guidViewHost, out holder);
            //    viewHost = (IWpfTextViewHost)holder;
            //    return viewHost;
            //}
        }

        private void menuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            // get the menu that fired the event
            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                // start by assuming that the menu will not be shown
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                IVsTextManager txtMgr = (IVsTextManager)this.ServiceProvider.GetService(typeof(SVsTextManager));
                IVsTextView vTextView = null;
                int mustHaveFocus = 1;
                txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);
                IDataObject dataObject = new OleDataObject();
                vTextView.GetSelectionDataObject(out dataObject);

                var selectedText = "";
                vTextView.GetSelectedText(out selectedText);

                if(selectedText == null || selectedText.Equals(""))
                    return;
             //   IVsHierarchy hierarchy = null;
             //   uint itemid = VSConstants.VSITEMID_NIL;

                //   if (!IsSingleProjectItemSelection(out hierarchy, out itemid)) return;
                // Get the file path
                //   string itemFullPath = null;
                //   ((IVsProject)hierarchy).GetMkDocument(itemid, out itemFullPath);
                //   var transformFileInfo = new FileInfo(itemFullPath);

                // then check if the file is named 'web.config'
                //  bool isWebConfig = string.Compare("web.config", transformFileInfo.Name, StringComparison.OrdinalIgnoreCase) == 0;

                // if not leave the menu hidden
                //  if (!isWebConfig) return;

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AskCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new AskCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param> 
        void OnWindowClosing(Window Window)
        {
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                "Test",
                "Po",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var webBrowserService = ServiceProvider.GetService(typeof(IVsWebBrowsingService)) as IVsWebBrowsingService;
            var webBrowserUser = Package.GetGlobalService(typeof(IVsWebBrowserUser)) as IVsWebBrowserUser;
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            IVsWindowFrame ppFrame;
            //https://desktop-vs.open.collab.net/ds/viewMessage.do?dsMessageId=98485&dsForumId=730
            Guid guidPropertyBrowser = new Guid(ToolWindowGuids80.WebBrowserWindow);
            IVsWebBrowser ppBrowser;
            webBrowserService.CreateWebBrowser(
                //http://i1.blogs.msdn.com/b/robgruen/archive/2005/11/23/496508.aspx
                (uint)(__VSCREATEWEBBROWSER.VSCWB_StartCustom | __VSCREATEWEBBROWSER.VSCWB_ForceNew | __VSCREATEWEBBROWSER.VSCWB_AutoShow),
                ref guidPropertyBrowser,
                "Test",
                "http://www.allegro.pl",
                webBrowserUser,
                out ppBrowser,
                out ppFrame);

            var frameNotifyService = ServiceProvider.GetService(typeof(IVsWindowFrameNotify)) as IVsWindowFrameNotify;
            uint cookie;
            (ppFrame as IVsWindowFrame2).Advise(frameNotifyService, out cookie);
            var ggg = new Guid(ToolWindowGuids80.WebBrowserWindow);//__VSFPROPID.VSFPROPID_ViewHelper;
            IntPtr interf;
            ppFrame.QueryViewInterface(ref ggg, out interf);
            WindowEvents _windowEvents;
            object browserWindow;
            // EnvDTE.Window _editorWindow=null;
            ppFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ExtWindowObject, out browserWindow);
            _windowEvents = dte.Events.get_WindowEvents((browserWindow as Window));
            _windowEvents.WindowClosing += OnWindowClosing;
            _windowEvents.WindowMoved += (window, top, left, width, height) => OnWindowClosing(window);
            Timer tmr = new Timer();
            tmr.Interval = 100; // 0.1 second
            ElapsedEventHandler timerHandler = (o, args) =>
            {
                object urlObject;
                ppBrowser.GetDocumentInfo((uint) __VSWBDOCINFOINDEX.VSWBDI_DocURL, out urlObject);
                var url = urlObject as string;

                tmr.Enabled = false;
                tmr = null;
            };
            tmr.Elapsed += timerHandler;
          //  tmr.Tick += timerHandler; // We'll write it in a bit
            tmr.Start(); // The countdown is launched!




            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "AskCommand";

            IVsTextManager txtMgr = (IVsTextManager)this.ServiceProvider.GetService(typeof(SVsTextManager));
            IVsTextView vTextView = null;
            int mustHaveFocus = 1;
            txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);
            vTextView.GetSelectedText(out title);
            IVsUserData userData = vTextView as IVsUserData;


            DTE2 dte2 = (DTE2) this.ServiceProvider.GetService(typeof (DTE));
    OutputWindow outputWindow = dte2.ToolWindows.OutputWindow;

            OutputWindowPane outputWindowPane = outputWindow.OutputWindowPanes.Add("A New Pane");
            outputWindowPane.OutputString("Some Text");


            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
