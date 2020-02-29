using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using SimpleRefactor.Logic;
using SimpleRefactor.Structs;
using Task = System.Threading.Tasks.Task;

namespace SimpleRefactor.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DocumentCodeSpanCommand
    {
        private static FullInitializationConverter fullInitializationConverter => new FullInitializationConverter();
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6047a655-8170-4b3c-978c-360dbfb13fc1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentCodeSpanCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private DocumentCodeSpanCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static DocumentCodeSpanCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
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
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in DocumentCodeSpanCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new DocumentCodeSpanCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void Execute(object sender, EventArgs e)
        {
            try
            {
                TextViewSelection selection = await GetSelectionAsync(ServiceProvider);
                string activeDocumentPath = await GetActiveDocumentFilePathAsync(ServiceProvider);
                if (fullInitializationConverter.IsSimplyInitializationBlock(selection))
                {
                    //convert to full initialization
                    string text = fullInitializationConverter.ConvertToFullInitialization(selection);
                }
            }
            catch
            {
                //show error
            }
        }

        private async Task<TextViewSelection> GetSelectionAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider)
        {
            var service = await serviceProvider.GetServiceAsync(typeof(SVsTextManager));
            var textManager = service as IVsTextManager2;
            IVsTextView view;
            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);

            view.GetSelection(out int startLine, out int startColumn, out int endLine, out int endColumn);//end could be before beginning
            var start = new TextViewPosition(startLine, startColumn);
            var end = new TextViewPosition(endLine, endColumn);

            view.GetSelectedText(out string selectedText);

            TextViewSelection selection = new TextViewSelection(start, end, selectedText);
            return selection;
        }

        private async Task<string> GetActiveDocumentFilePathAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 applicationObject = await serviceProvider.GetServiceAsync(typeof(DTE)) as EnvDTE80.DTE2;
            Assumes.Present(applicationObject);
            return applicationObject.ActiveDocument.FullName;
        }
    }
}
