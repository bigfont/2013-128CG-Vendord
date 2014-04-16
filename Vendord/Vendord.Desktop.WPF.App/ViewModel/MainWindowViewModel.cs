using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Data;
using System.ComponentModel;
using System.Diagnostics;
using Vendord.Desktop.WPF.App.Properties;
using Vendord.Desktop.WPF.App.DataAccess;

namespace Vendord.Desktop.WPF.App.ViewModel
{
    /// <summary>
    /// The ViewModel for the application's main window.
    /// </summary>
    public class MainWindowViewModel : WorkspaceViewModel
    {
        #region Fields

        ReadOnlyCollection<CommandViewModel> _commands;
        readonly Repository _repository;
        ObservableCollection<CommandViewModel> _subCommands;
        ObservableCollection<WorkspaceViewModel> _workspaces;

        #endregion // Fields

        #region Constructor

        public MainWindowViewModel()
        {
            base.DisplayName = Strings.MainWindowViewModel_DisplayName;

            _repository = new Repository();
        }

        #endregion // Constructor

        #region Commands

        /// <summary>
        /// Returns a read-only list of commands 
        /// that the UI can display and execute.
        /// </summary>
        public ReadOnlyCollection<CommandViewModel> Commands
        {
            get
            {
                if (_commands == null)
                {
                    List<CommandViewModel> cmds = this.CreateCommands();
                    _commands = new ReadOnlyCollection<CommandViewModel>(cmds);
                }
                return _commands;
            }
        }

        public ObservableCollection<CommandViewModel> SubCommands
        {
            get
            {
                if (_subCommands == null)
                {
                    _subCommands = new ObservableCollection<CommandViewModel>();
                    ////_subCommands.CollectionChanged += this.OnSubCommandsChanged;
                }
                return _subCommands;
            }
        }

        object _subCommandSet;
        public object SubCommandSet
        {
            get
            {
                return _subCommandSet;
            }
            set
            {
                if (_subCommandSet != value)
                {
                    _subCommandSet = value;
                    base.OnPropertyChanged("SubCommandSet");
                }
            }
        }

        List<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>
            {
                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_Sync,
                    new RelayCommand(param => this.ShowSyncOptions())),

                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_Orders,
                    new RelayCommand(param => this.ShowAllOrders()))
            };
        }

        #endregion // Commands

        #region Workspaces

        /// <summary>
        /// Returns the collection of available workspaces to display.
        /// A 'workspace' is a ViewModel that can request to be closed.
        /// </summary>
        public ObservableCollection<WorkspaceViewModel> Workspaces
        {
            get
            {
                if (_workspaces == null)
                {
                    _workspaces = new ObservableCollection<WorkspaceViewModel>();
                    _workspaces.CollectionChanged += this.OnWorkspacesChanged;
                }
                return _workspaces;
            }
        }

        void OnWorkspacesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.NewItems)
                    workspace.RequestClose += this.OnWorkspaceRequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.OldItems)
                    workspace.RequestClose -= this.OnWorkspaceRequestClose;
        }

        void OnWorkspaceRequestClose(object sender, EventArgs e)
        {
            WorkspaceViewModel workspace = sender as WorkspaceViewModel;
            workspace.Dispose();
            this.Workspaces.Remove(workspace);
        }

        #endregion // Workspaces

        #region Private Helpers

        void ShowSyncOptions()
        {
            SyncCommandsViewModel commands = new SyncCommandsViewModel();
            SetActiveSubCommands(commands.DisplayName, commands);
        }

        void ShowAllOrders()
        {
            AllOrdersViewModel workspace =
                this.Workspaces.FirstOrDefault(vm => vm is AllOrdersViewModel)
                as AllOrdersViewModel;

            if (workspace == null)
            {
                workspace = new AllOrdersViewModel(_repository);
                this.Workspaces.Add(workspace);
            }

            this.SetActiveWorkspace(workspace);
            this.SetActiveSubCommands(workspace.DisplayName, workspace.Commands.ToList());
        }

        void SetActiveWorkspace(WorkspaceViewModel workspace)
        {
            Debug.Assert(this.Workspaces.Contains(workspace));

            ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.Workspaces);
            if (collectionView != null)
                collectionView.MoveCurrentTo(workspace);
        }

        void SetActiveSubCommands(string commandSet, List<CommandViewModel> commands)
        {
            SubCommands.Clear();
            SubCommandSet = commandSet;
            commands.ForEach(c =>
            {
                SubCommands.Add(c);
            });
        }

        #endregion // Private Helpers
    }
}
