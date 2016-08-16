//------------------------------------------------------------------------------
// <copyright file="AskCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Globalization;
using AskExtension.Core;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = System.IServiceProvider;

namespace AskExtension.MenuCommands.AskCommand
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

        [Import]
        public AuthenticationService _authenticationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AskCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private AskCommand(Package package)
        {
            ExtensionMefContainer.Service.SatisfyImportsOnce(this);

            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            var commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandId = new CommandID(CommandSet, CommandId);

                var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandId);
                menuItem.BeforeQueryStatus += menuCommand_BeforeQueryStatus;

                commandService.AddCommand(menuItem);
            }
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

                var txtMgr = (IVsTextManager)ServiceProvider.GetService(typeof(SVsTextManager));
                IVsTextView vTextView;
                const int mustHaveFocus = 1;
                txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);
                IDataObject dataObject;
                vTextView.GetSelectionDataObject(out dataObject);

                var selectedText = "";
                vTextView.GetSelectedText(out selectedText);

                if (selectedText == null || selectedText.Equals(""))
                    return;

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
        private IServiceProvider ServiceProvider => this.package;

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
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;

            if (_authenticationService.IsAuthorized())
            {

            }
            else
            {
                _authenticationService.Authorize();
            }

            var message = $"Inside {GetType().FullName}.MenuItemCallback()";
            string title;

            var txtMgr = (IVsTextManager)this.ServiceProvider.GetService(typeof(SVsTextManager));
            IVsTextView vTextView = null;
            const int mustHaveFocus = 1;
            txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);
            vTextView.GetSelectedText(out title);
            var userData = vTextView as IVsUserData;


            var dte2 = (DTE2)this.ServiceProvider.GetService(typeof(DTE));
            var outputWindow = dte2.ToolWindows.OutputWindow;

            var outputWindowPane = outputWindow.OutputWindowPanes.Add("A New Pane");
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
