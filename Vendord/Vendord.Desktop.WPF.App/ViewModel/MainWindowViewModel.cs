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

        List<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>
            {
                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_Sync,
                    new RelayCommand(param => this.ShowSyncOptions())),

                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_Orders,
                    new RelayCommand(param => this.ShowAllOrders())),

                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_Products,
                    new RelayCommand(param => this.ShowAllProducts())),

                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_OrderProducts,
                    new RelayCommand(param => this.ShowAllOrderProducts()))
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
            SyncCommandsViewModel workspace =
                this.Workspaces.FirstOrDefault(vm => vm is SyncCommandsViewModel)
                as SyncCommandsViewModel;

            if (workspace == null)
            {
                workspace = new SyncCommandsViewModel();
                this.Workspaces.Add(workspace);
            }

            this.SetActiveWorkspace(workspace);
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
        }

        void ShowAllProducts()
        {
            AllProductsViewModel workspace =
                this.Workspaces.FirstOrDefault(vm => vm is AllProductsViewModel)
                as AllProductsViewModel;

            if (workspace == null)
            {
                workspace = new AllProductsViewModel(_repository);
                this.Workspaces.Add(workspace);
            }

            this.SetActiveWorkspace(workspace);
        }

        void ShowAllOrderProducts()
        {
            AllOrderProductsViewModel workspace =
                this.Workspaces.FirstOrDefault(vm => vm is AllOrderProductsViewModel)
                as AllOrderProductsViewModel;

            if (workspace == null)
            {
                workspace = new AllOrderProductsViewModel(_repository);
                this.Workspaces.Add(workspace);
            }

            this.SetActiveWorkspace(workspace);
        }

        void SetActiveWorkspace(WorkspaceViewModel workspace)
        {
            Debug.Assert(this.Workspaces.Contains(workspace));

            ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.Workspaces);
            if (collectionView != null)
                collectionView.MoveCurrentTo(workspace);
        }

        #endregion // Private Helpers
    }
}
