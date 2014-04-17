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

    class SyncCommandsViewModel : WorkspaceViewModel
    {
        ReadOnlyCollection<CommandViewModel> _commands;
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

        public SyncCommandsViewModel()
        {
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
                    string message = Environment.NewLine + e.UserState.ToString();
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
        }

        private void ImportXmlProducts()
        {
            XmlProductsBackgroundWorker bw = new XmlProductsBackgroundWorker(ProgressChanged, RunWorkerCompleted);
            bw.Run();
        }

        private void ImportXmlVendors()
        {
            XmlVendorsBackgroundWorker bw = new XmlVendorsBackgroundWorker(ProgressChanged, RunWorkerCompleted);
            bw.Run();
        }

        private void SyncDbOrders()
        {
            SyncDbOrdersBackgroundWorker bw = new SyncDbOrdersBackgroundWorker(this.ProgressChanged, RunWorkerCompleted);
            bw.Run();
        }

        private void SyncDbProductsVendorsDepartments()
        {
            SyncDbProductsVendorsDepartmentsBackgroundWorker bw = new SyncDbProductsVendorsDepartmentsBackgroundWorker(this.ProgressChanged, RunWorkerCompleted);
            bw.Run();
        }

        #region Private Helpers

        private const int maxRecentlyImportedItems = 15;
        private void AddToRecentlyImportedItems(string s)
        {
            if (RecentlyImportedItems.Count() > maxRecentlyImportedItems)
            {
                RecentlyImportedItems.RemoveAt(0);
            }
            RecentlyImportedItems.Add(s);
        }

        #endregion
    }
}
