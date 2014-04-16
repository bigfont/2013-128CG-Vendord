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

    class SyncCommandsViewModel : WorkspaceViewModel
    {
        ReadOnlyCollection<CommandViewModel> _commands;
        private int _currentProgress;
        private string _recentlyImportedItems;

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

        public string RecentlyImportedItems
        {
            get 
            {
                if (this._recentlyImportedItems == null)
                {
                    this._recentlyImportedItems = string.Empty;
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

        private void ImportXmlProducts()
        {
            try
            {
                ProgressChangedEventHandler progressChanged = new ProgressChangedEventHandler((s, e) =>
                {
                    ProgressChangedEventArgs args = e as ProgressChangedEventArgs;
                    if (args != null)
                    {
                        if(args.ProgressPercentage > 0)
                        {
                            CurrentProgress = args.ProgressPercentage;
                        }
                        if (args.UserState != null)
                        {                            
                            string message = Environment.NewLine + args.UserState.ToString();
                            RecentlyImportedItems = RecentlyImportedItems.Insert(0, message);
                        }                        
                    }
                    
                });

                XmlProductsBackgroundWorker bw = new XmlProductsBackgroundWorker(progressChanged);
                bw.Run();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }
        }        

        private void ImportXmlVendors()
        {
            XmlSync sync = new XmlSync();
            BackgroundWorker worker = null;
#if ImportTestData
            string filePath = @"C:\Users\Shaun\SkyDrive\Documents\Work\BigFont\Clients\2013-124CG\DataToImport\my-vendors-small.xml";
#else
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Vendord\DataToImport\vendors.xml");
#endif
            int totalRecords = 0;
            int insertedRecords = 0;
            try
            {
                SyncResult result =
                    sync.PullVendorsFromItRetailXmlBackup(
                        worker,
                        filePath,
                        ref totalRecords,
                        ref insertedRecords);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        private void SyncDbOrders()
        {
            DbSync sync = new DbSync();
            try
            {
                SyncResult result = sync.SyncDesktopAndDeviceDatabases(ScopeName.SyncOrders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        private void SyncDbProductsVendorsDepartments()
        {
            DbSync sync = new DbSync();
            try
            {
                SyncResult result = sync.SyncDesktopAndDeviceDatabases(ScopeName.SyncProductsVendorsAndDepts);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }
        }
    }

    public class XmlProductsBackgroundWorker
    {
        private static BackgroundWorker bWorker;

        public XmlProductsBackgroundWorker(ProgressChangedEventHandler ProgressChanged)
        {
            bWorker = new BackgroundWorker();
            bWorker.WorkerReportsProgress = true;
            bWorker.DoWork += new DoWorkEventHandler(DoWork);
            bWorker.ProgressChanged += ProgressChanged;
        }

        public void Run()
        {
            bWorker.RunWorkerAsync();
            bWorker.ReportProgress(5);
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            XmlSync sync = new XmlSync();
            BackgroundWorker worker = sender as BackgroundWorker;
#if ImportTestData
            string filePath = @"C:\Users\Shaun\SkyDrive\Documents\Work\BigFont\Clients\2013-124CG\DataToImport\my-products-small.xml";
#else
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Vendord\DataToImport\products.xml");
#endif
            int totalRecords = 0;
            int insertedRecords = 0;

            SyncResult result = sync.PullProductsFromItRetailXmlBackup(
                worker,
                filePath,
                ref totalRecords,
                ref insertedRecords);
        }
    }
}
