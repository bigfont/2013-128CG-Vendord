////#define ImportTestData

namespace Vendord.Desktop.WPF.App.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Input;
    using System.Collections.ObjectModel;
    using Vendord.Desktop.WPF.App.Properties;
    using Vendord.Sync;
    using System.ComponentModel;
    using System.IO;
    using Vendord.Desktop.WPF.App.BackgroundWorkers;
    using Vendord.Desktop.WPF.App.DataAccess;

    class SyncCommandsViewModel : WorkspaceViewModel
    {
        private enum SyncTargets
        {
            XmlProducts,
            XmlVendors,
            DbOrders,
            DbProductsVendorsDepartments
        }

        private SyncTargets _currentlySyncing;
        private readonly Repository _repository;
        private ReadOnlyCollection<CommandViewModel> _commands;
        private int _currentProgress;
        private ObservableCollection<string> _recentlyImportedItems;

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

        public int CurrentProgress
        {
            get { return this._currentProgress; }
            private set
            {
                if (this._currentProgress != value)
                {
                    this._currentProgress = value;
                    this.OnPropertyChanged("CurrentProgress");
                }
            }
        }

        public ObservableCollection<string> RecentlyImportedItems
        {
            get
            {
                if (this._recentlyImportedItems == null)
                {
                    this._recentlyImportedItems = new ObservableCollection<string>();
                }
                return this._recentlyImportedItems;
            }
            private set
            {
                if (this._recentlyImportedItems != value)
                {
                    this._recentlyImportedItems = value;
                    this.OnPropertyChanged("RecentlyImportedItems");
                }
            }
        }

        public SyncCommandsViewModel(Repository repository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            this._repository = repository;

            DisplayName = "Sync";
        }

        private List<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>
            {
                new CommandViewModel(
                    Strings.SyncCommandsViewModel_Command_ImportXmlProducts,
                    new RelayCommand(param => this.ImportXmlProducts())),

                new CommandViewModel(
                    Strings.SyncCommandsViewModel_Command_ImportXmlVendors,
                    new RelayCommand(param => this.ImportXmlVendors())),

                new CommandViewModel(
                    Strings.SyncCommandsViewModel_Command_SyncDbOrders,
                    new RelayCommand(param => this.SyncDbOrders())),

                new CommandViewModel(
                    Strings.SyncCommandsViewModel_Command_SyncDbProductsVendorsDepartments,
                    new RelayCommand(param => this.SyncDbProductsVendorsDepartments()))
            };
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e != null)
            {
                if (e.ProgressPercentage > 0)
                {
                    CurrentProgress = e.ProgressPercentage;
                }
                if (e.UserState != null)
                {
                    string message = e.UserState.ToString();
                    AddToRecentlyImportedItems(message);
                }
            }
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string message;
            if (e.Cancelled)
            {
                message = "Cancelled";
            }
            else if (e.Error != null)
            {
                message = e.Error.Message;
            }
            else if (e.Result != null)
            {
                message = e.Result.ToString();
            }
            else
            {
                message = "Complete";
            }
            AddToRecentlyImportedItems(message);
            RefreshRepository();
        }

        private void RefreshRepository()
        {
            switch (_currentlySyncing)
            {
                case SyncTargets.DbOrders:
                    {
                        _repository.ReloadOrderProducts();
                        _repository.ReloadOrders();                        
                        break;
                    }
                case SyncTargets.DbProductsVendorsDepartments:
                    {
                        _repository.ReloadProducts();
                        _repository.ReloadVendors();
                        _repository.ReloadDepartments();
                        break;
                    }
                case SyncTargets.XmlProducts:
                    {
                        _repository.ReloadProducts();
                        break;
                    }
                case SyncTargets.XmlVendors:
                    {
                        _repository.ReloadVendors();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void ImportXmlProducts()
        {
            _currentlySyncing = SyncTargets.XmlProducts;
            XmlProductsBackgroundWorker bw = new XmlProductsBackgroundWorker(ProgressChanged, RunWorkerCompleted);
            bw.Run();
        }

        private void ImportXmlVendors()
        {
            _currentlySyncing = SyncTargets.XmlVendors;
            XmlVendorsBackgroundWorker bw = new XmlVendorsBackgroundWorker(ProgressChanged, RunWorkerCompleted);
            bw.Run();
        }

        private void SyncDbOrders()
        {
            _currentlySyncing = SyncTargets.DbOrders;
            SyncDbOrdersBackgroundWorker bw = new SyncDbOrdersBackgroundWorker(this.ProgressChanged, RunWorkerCompleted);
            bw.Run();
        }

        private void SyncDbProductsVendorsDepartments()
        {
            _currentlySyncing = SyncTargets.DbProductsVendorsDepartments;
            SyncDbProductsVendorsDepartmentsBackgroundWorker bw = new SyncDbProductsVendorsDepartmentsBackgroundWorker(this.ProgressChanged, RunWorkerCompleted);
            bw.Run();
        }

        #region Private Helpers

        private const int maxRecentlyImportedItems = 15;
        private void AddToRecentlyImportedItems(string message)
        {
            message += Environment.NewLine;
            if (RecentlyImportedItems.Count() > maxRecentlyImportedItems)
            {
                RecentlyImportedItems.RemoveAt(0);
            }
            RecentlyImportedItems.Add(message);
        }

        #endregion
    }
}
