using System;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServerCe;
using Vendord.SmartDevice.Shared;

[module:
    SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.Desktop.App
{
    // Full Edition
    // Compact Edition
    using RAPI = System.Devices; // Remote API Managed Code Wrapper

    public class Sync
    {
        // rapi       
        public enum SyncResult
        {
            Disconnected,
            Complete
        }

        private string remoteDatabaseFullPath;
        private string remoteDatabaseLocalCopyFullPath;

        // ctor

        // sync status

        public SyncResult PullProductsFromITRetailDatabase()
        {
            SyncResult result;
            try
            {
                CopyProductsFromITRetailDBToDesktopDB();
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
            string[] tablesToSync;
            string scopeName;
            DbSyncScopeDescription localScopeDesc;
            DbSyncScopeDescription remoteScopeDesc;
            SqlCeConnection localConn;
            SqlCeConnection remoteConn;
            SyncOrchestrator orchestrator;
            SyncResult result;
            RAPI.RemoteDeviceManager mgr;
            RAPI.RemoteDevice remoteDevice;

            // assume the worste
            result = SyncResult.Disconnected;

            // get the remote device
            mgr = new RAPI.RemoteDeviceManager();
            remoteDevice = mgr.Devices.FirstConnectedDevice;
            if (remoteDevice != null && remoteDevice.Status == RAPI.DeviceStatus.Connected)
            {
                // we assume that we have a connected device now but sometimes it disconnected
                try
                {
                    // Get the remote database
                    SetRemoteDeviceDatabaseNames(remoteDevice);
                    CopyDatabaseFromDeviceToDesktop(remoteDevice);

                    // Instantiate the connections
                    localConn = new SqlCeConnection(Database.GenerateSqlCeConnString(Constants.DatabaseFullPath));
                    remoteConn = new SqlCeConnection(Database.GenerateSqlCeConnString(remoteDatabaseLocalCopyFullPath));

                    // Describe the scope
                    tablesToSync = new[] {"tblOrder", "tblProduct", "tblOrderProduct"};
                    scopeName = "OrdersAndProducts";
                    localScopeDesc = DescribeTheScope(tablesToSync, scopeName, localConn);
                    remoteScopeDesc = DescribeTheScope(tablesToSync, scopeName, remoteConn);

                    // Provision the nodes
                    ProvisionNode(localScopeDesc, localConn);
                    ProvisionNode(remoteScopeDesc, remoteConn);

                    // Set sync options
                    orchestrator = SetSyncOptions(localScopeDesc, localConn, remoteConn);

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
            }

            return result;
        }

        private void CopyProductsFromITRetailDBToDesktopDB()
        {
            // insert all products from IT Retail
            using (var conn = new SqlConnection(Constants.ItRetailDatabaseConnectionString))
            {
                conn.Open();
                var command = new SqlCommand(@"SELECT Name, UPC, VendorName FROM Product", conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var product = new Product
                    {
                        Name = Convert.ToString(reader["Name"]),
                        UPC = Convert.ToString(reader["UPC"]),
                        VendorName = Convert.ToString(reader["VendorName"])
                    };

                    product.UpsertIntoDB(new Database());
                }
            }
        }

        private void SetRemoteDeviceDatabaseNames(RAPI.RemoteDevice remoteDevice)
        {
            string rapiApplicationData;
            string rapiApplicationDataStore;

            rapiApplicationData = remoteDevice.GetFolderPath(RAPI.SpecialFolder.ApplicationData);
            rapiApplicationDataStore = Path.Combine(rapiApplicationData, Constants.ApplicationName);
            remoteDatabaseFullPath = Path.Combine(rapiApplicationDataStore, Constants.ApplicationDatabaseName);
            remoteDatabaseLocalCopyFullPath = IOHelpers.AddSuffixToFilePath(Constants.DatabaseFullPath,
                Constants.RemoteCopyFlag);
        }

        private void CopyDatabaseFromDeviceToDesktop(RAPI.RemoteDevice remoteDevice)
        {
            // does the device have a database
            if (RAPI.RemoteFile.Exists(remoteDevice, remoteDatabaseFullPath))
            {
                // yup, so copy it to the desktop
                RAPI.RemoteFile.CopyFileFromDevice(remoteDevice, remoteDatabaseFullPath, remoteDatabaseLocalCopyFullPath,
                    true);
            }
            else
            {
                RAPI.RemoteFile.Create(remoteDevice, remoteDatabaseFullPath);
            }
        }

        private void CleanUpDatabases()
        {
            var db = new Database();
            db.EmptyTrash();

            var db_remote = new Database(remoteDatabaseLocalCopyFullPath);
            db_remote.EmptyTrash();
        }

        private void CopyDatabaseBackToDevice(RAPI.RemoteDevice remoteDevice)
        {
            if (File.Exists(remoteDatabaseLocalCopyFullPath) &
                RAPI.RemoteFile.Exists(remoteDevice, remoteDatabaseFullPath))
            {
                RAPI.RemoteFile.CopyFileToDevice(remoteDevice, remoteDatabaseLocalCopyFullPath, remoteDatabaseFullPath,
                    true);
            }
        }

        #region MS Sync Framework

        private DbSyncScopeDescription DescribeTheScope(string[] tablesToSync, string scopeName,
            SqlCeConnection localConn)
        {
            DbSyncScopeDescription scopeDesc;
            DbSyncTableDescription tableDesc;

            // create a scope description object
            scopeDesc = new DbSyncScopeDescription();
            scopeDesc.ScopeName = scopeName;

            // connect to the local version of the database

            // add each table to the scope without any filtering
            foreach (string tableName in tablesToSync)
            {
                // get the table descriptions from the database
                tableDesc = SqlCeSyncDescriptionBuilder.GetDescriptionForTable(tableName, localConn);
                scopeDesc.Tables.Add(tableDesc);
            }

            return scopeDesc;
        }

        private void ProvisionNode(DbSyncScopeDescription scopeDesc, SqlCeConnection conn)
        {
            SqlCeSyncScopeProvisioning ceConfig;

            ceConfig = new SqlCeSyncScopeProvisioning(conn);
            if (!ceConfig.ScopeExists(scopeDesc.ScopeName))
            {
                ceConfig.PopulateFromScopeDescription(scopeDesc);
                ceConfig.Apply();
            }
        }

        private SyncOrchestrator SetSyncOptions(DbSyncScopeDescription scopeDesc, SqlCeConnection localConn,
            SqlCeConnection remoteConn)
        {
            SqlCeSyncProvider localProvider;
            SqlCeSyncProvider remoteProvider;
            SyncOrchestrator orchestrator;

            localProvider = new SqlCeSyncProvider();
            localProvider.ScopeName = scopeDesc.ScopeName;
            localProvider.Connection = localConn;

            remoteProvider = new SqlCeSyncProvider();
            remoteProvider.ScopeName = scopeDesc.ScopeName;
            remoteProvider.Connection = remoteConn;

            orchestrator = new SyncOrchestrator();
            orchestrator.LocalProvider = localProvider;
            orchestrator.RemoteProvider = remoteProvider;
            orchestrator.Direction = SyncDirectionOrder.DownloadAndUpload;

            return orchestrator;
        }

        private void SyncTheNodes(SyncOrchestrator orchestrator)
        {
            orchestrator.Synchronize();
        }

        #endregion
    }
}