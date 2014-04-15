using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Vendord.Desktop.WPF.App.Properties;
using Vendord.Sync;
using System.ComponentModel;

namespace Vendord.Desktop.WPF.App.ViewModel
{
    class SyncCommandsViewModel : WorkspaceViewModel
    {
        ReadOnlyCollection<CommandViewModel> _commands;

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
            base.DisplayName = Strings.SyncCommandsViewModel_DisplayName;
        }

        private List<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>
            {
                new CommandViewModel(
                    Strings.SyncCommandsViewModel_ImportXmlProducts,
                    new RelayCommand(param => this.ImportXmlProducts())),

                new CommandViewModel(
                    Strings.SyncCommandsViewModel_ImportXmlVendors,
                    new RelayCommand(param => this.ImportXmlVendors())),

                new CommandViewModel(
                    Strings.SyncCommandsViewModel_SyncDbOrders,
                    new RelayCommand(param => this.SyncDbOrders())),

                new CommandViewModel(
                    Strings.SyncCommandsViewModel_SyncDbProductsVendorsDepartments,
                    new RelayCommand(param => this.SyncDbProductsVendorsDepartments()))
            };
        }

        private void ImportXmlProducts()
        {
            XmlSync sync = new XmlSync();
            BackgroundWorker worker = null;
            string filePath = @"C:\Users\Shaun\SkyDrive\Documents\Work\BigFont\Clients\2013-124CG\DataToImport\my-products-small.xml";
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
            string filePath = @"C:\Users\Shaun\SkyDrive\Documents\Work\BigFont\Clients\2013-124CG\DataToImport\my-vendors-small.xml";
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
