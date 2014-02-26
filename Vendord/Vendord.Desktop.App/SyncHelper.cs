using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServerCe;
using Vendord.SmartDevice.Linked;

[module:
    SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.Desktop.App
{
    // Full Edition
    // Compact Edition
    using RAPI = System.Devices;
    using System.Data.OleDb;
    using System.Xml.Linq;
    using System.ComponentModel; // Remote API Managed Code Wrapper

    public class Sync
    {
        // rapi       
        public enum SyncResult
        {
            Disconnected,
            Complete
        }

        private string _remoteDatabaseFullPath;
        private string _remoteDatabaseLocalCopyFullPath;

        public SyncResult PullProductsFromItRetailDatabase(BackgroundWorker worker, ref int totalRecords, ref int insertedRecords)
        {
            SyncResult result;
            try
            {
                CopyProductsFromItRetailMsAccessBackupFilesToDesktopDb(worker, ref totalRecords, ref insertedRecords);
                result = SyncResult.Complete;
            }
            catch (SqlException ex)
            {
                result = SyncResult.Disconnected;
                IOHelpers.LogException(ex);
            }

            return result;
        }

        public SyncResult MergeDesktopAndDeviceDatabases()
        {
            // assume the worste
            var result = SyncResult.Disconnected;

            // get the remote device
            var mgr = new RAPI.RemoteDeviceManager();
            var remoteDevice = mgr.Devices.FirstConnectedDevice;
            if (remoteDevice == null || remoteDevice.Status != RAPI.DeviceStatus.Connected) return result;
            // we assume that we have a connected device now but sometimes it disconnected
            try
            {
                // Get the remote database
                SetRemoteDeviceDatabaseNames(remoteDevice);
                CopyDatabaseFromDeviceToDesktop(remoteDevice);

                // Instantiate the connections
                var localConn = new SqlCeConnection(Database.GenerateSqlCeConnString(Constants.VendordMainDatabaseFullPath));
                var remoteConn = new SqlCeConnection(Database.GenerateSqlCeConnString(_remoteDatabaseLocalCopyFullPath));

                // Describe the scope
                var tablesToSync = new[] { "tblOrder", "tblProduct", "tblOrderProduct" };
                const string scopeName = "OrdersAndProducts";
                var localScopeDesc = DescribeTheScope(tablesToSync, scopeName, localConn);
                var remoteScopeDesc = DescribeTheScope(tablesToSync, scopeName, remoteConn);

                // Provision the nodes
                ProvisionNode(localScopeDesc, localConn);
                ProvisionNode(remoteScopeDesc, remoteConn);

                // Set sync options
                var orchestrator = SetSyncOptions(localScopeDesc, localConn, remoteConn);

                // Sync
                SyncTheNodes(orchestrator);

                // Clean up
                remoteConn.Close();
                localConn.Close();
                CleanUpDatabases();
                CopyDatabaseBackToDevice(remoteDevice);

                // success!
                result = SyncResult.Complete;
            }
            catch (InvalidOperationException e)
            {
                IOHelpers.LogException(e);
            }
            catch (RAPI.RapiException)
            {
            }

            return result;
        }

        private List<Product> GetProductListFromMsAccessXmlBackup()
        {
            XElement productsXml = XElement.Load("products.xml");
            var query =
                from p in productsXml.Descendants("Products")
                select new Product()
                {
                    Upc = (string)p.Element("upc"),
                    Name = (string)p.Element("description"),
                    Price = (decimal?)p.Element("normal_price"),
                    VendorId = (int?)p.Element("vendor"), 
                    VendorName = ""
                };
            List<Product> products = query.ToList<Product>();
            return products;
        }

        private void CopyProductsFromItRetailMsAccessBackupFilesToDesktopDb(BackgroundWorker worker, ref int totalRecords, ref int insertedRecords)
        {
            List<Product> products = GetProductListFromMsAccessXmlBackup();

            insertedRecords = 0;
            totalRecords = products.Count();
            DateTime progressReportTime = DateTime.MinValue;

            foreach (Product p in products)
            {                
                p.UpsertIntoDb(new Database());
                insertedRecords++;

                // if five seconds have passed, make the worker report progress
                double timeSinceLastReport = DateTime.Now.Subtract(progressReportTime).TotalSeconds;
                if (timeSinceLastReport >= 1.0)
                {                    
                    worker.ReportProgress(100 * insertedRecords / totalRecords);
                    progressReportTime = DateTime.Now;
                }
            }
        }

        private void SetRemoteDeviceDatabaseNames(RAPI.RemoteDevice remoteDevice)
        {
            var rapiApplicationData = remoteDevice.GetFolderPath(RAPI.SpecialFolder.ApplicationData);
            var rapiApplicationDataStore = Path.Combine(rapiApplicationData, Constants.ApplicationName);
            _remoteDatabaseFullPath = Path.Combine(rapiApplicationDataStore, Constants.ApplicationDatabaseName);
            _remoteDatabaseLocalCopyFullPath = IOHelpers.AddSuffixToFilePath(Constants.VendordMainDatabaseFullPath,
                Constants.RemoteCopyFlag);
        }

        private void CopyDatabaseFromDeviceToDesktop(RAPI.RemoteDevice remoteDevice)
        {
            // does the device have a database
            if (RAPI.RemoteFile.Exists(remoteDevice, _remoteDatabaseFullPath))
            {
                // yup, so copy it to the desktop
                RAPI.RemoteFile.CopyFileFromDevice(remoteDevice, _remoteDatabaseFullPath, _remoteDatabaseLocalCopyFullPath,
                    true);
            }
            else
            {
                RAPI.RemoteFile.Create(remoteDevice, _remoteDatabaseFullPath);
            }
        }

        private void CleanUpDatabases()
        {
            var db = new Database();
            db.EmptyTrash();

            var dbRemote = new Database(_remoteDatabaseLocalCopyFullPath);
            dbRemote.EmptyTrash();
        }

        private void CopyDatabaseBackToDevice(RAPI.RemoteDevice remoteDevice)
        {
            if (File.Exists(_remoteDatabaseLocalCopyFullPath) &
                RAPI.RemoteFile.Exists(remoteDevice, _remoteDatabaseFullPath))
            {
                RAPI.RemoteFile.CopyFileToDevice(remoteDevice, _remoteDatabaseLocalCopyFullPath, _remoteDatabaseFullPath,
                    true);
            }
        }

        #region MS Sync Framework

        private static DbSyncScopeDescription DescribeTheScope(IEnumerable<string> tablesToSync, string scopeName,
            SqlCeConnection localConn)
        {
            // create a scope description object
            var scopeDesc = new DbSyncScopeDescription { ScopeName = scopeName };

            // connect to the local version of the database

            // add each table to the scope without any filtering
            foreach (var tableDesc in tablesToSync.Select(tableName => SqlCeSyncDescriptionBuilder.GetDescriptionForTable(tableName, localConn)))
            {
                scopeDesc.Tables.Add(tableDesc);
            }

            return scopeDesc;
        }

        private static void ProvisionNode(DbSyncScopeDescription scopeDesc, SqlCeConnection conn)
        {
            var ceConfig = new SqlCeSyncScopeProvisioning(conn);
            if (ceConfig.ScopeExists(scopeDesc.ScopeName)) return;
            ceConfig.PopulateFromScopeDescription(scopeDesc);
            ceConfig.Apply();
        }

        private static SyncOrchestrator SetSyncOptions(DbSyncScopeDescription scopeDesc, IDbConnection localConn,
            IDbConnection remoteConn)
        {
            var localProvider = new SqlCeSyncProvider { ScopeName = scopeDesc.ScopeName, Connection = localConn };

            var remoteProvider = new SqlCeSyncProvider { ScopeName = scopeDesc.ScopeName, Connection = remoteConn };

            var orchestrator = new SyncOrchestrator
            {
                LocalProvider = localProvider,
                RemoteProvider = remoteProvider,
                Direction = SyncDirectionOrder.DownloadAndUpload
            };

            return orchestrator;
        }

        private static void SyncTheNodes(SyncOrchestrator orchestrator)
        {
            orchestrator.Synchronize();
        }

        #endregion
    }
}