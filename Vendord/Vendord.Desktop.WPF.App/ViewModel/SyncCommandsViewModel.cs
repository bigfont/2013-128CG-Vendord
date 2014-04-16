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

    class SyncCommandsViewModel : List<CommandViewModel>
    {
        ReadOnlyCollection<CommandViewModel> _commands;

        public virtual string DisplayName { get; protected set; }

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

        public SyncCommandsViewModel()
        {
            DisplayName = "Sync";
            this.AddRange(CreateCommands());
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
            XmlSync sync = new XmlSync();
            BackgroundWorker worker = null;
#if ImportTestData
            string filePath = @"C:\Users\Shaun\SkyDrive\Documents\Work\BigFont\Clients\2013-124CG\DataToImport\my-products-small.xml";
#else
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Vendord\DataToImport\products.xml");
#endif
            int totalRecords = 0;
            int insertedRecords = 0;
            try
            {
                SyncResult result =
                    sync.PullProductsFromItRetailXmlBackup(
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
}
