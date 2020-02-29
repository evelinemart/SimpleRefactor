using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace SimpleRefactor.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SimpleRefactorSwitchCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;
        private const string DISABLE = "Disable SimpleRefactor";
        private const string ENABLE = "Enable SimpleRefactor";

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("835014ea-170d-421e-bc34-ad08bf024ae6");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleRefactorSwitchCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SimpleRefactorSwitchCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.Text = GeneralSettings.Default.IsSimpleRefacorEnabled ? DISABLE : ENABLE;
            menuItem.Checked = GeneralSettings.Default.IsSimpleRefacorEnabled;
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SimpleRefactorSwitchCommand Instance
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
            // Switch to the main thread - the call to AddCommand in SimpleRefactorSwitchCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SimpleRefactorSwitchCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            GeneralSettings.Default.IsSimpleRefacorEnabled = !GeneralSettings.Default.IsSimpleRefacorEnabled;
            GeneralSettings.Default.Save();
            var command = sender as OleMenuCommand;
            command.Text = GeneralSettings.Default.IsSimpleRefacorEnabled ? DISABLE : ENABLE;
            command.Checked = GeneralSettings.Default.IsSimpleRefacorEnabled;
        }
    }
}
