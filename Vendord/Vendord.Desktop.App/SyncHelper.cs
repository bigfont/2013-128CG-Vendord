namespace Vendord.Desktop.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using RAPI = System.Devices; // Remote API Managed Code Wrapper
    using System.IO;
    using Microsoft.Synchronization;
    using Microsoft.Synchronization.Data.SqlServerCe;
    using Microsoft.Synchronization.Data;
    using Vendord.SmartDevice.Shared;
    using System.Data.SqlServerCe; // Compact Edition
    using System.Data.SqlClient; // Full Edition

    public class Sync
    {
        // rapi        
        private const string REMOTE_DEVICE_DB_SYNC_SCOPE = "REMOTE_DEVICE_DB_SYNC_SCOPE";

        // rapi
        private RAPI.RemoteDeviceManager mgr;
        private string remoteDatabase_FullPath;
        private string remoteDatabase_LocalCopy_FullPath;

        // sync status
        public enum SyncResult
        {
            Disconnected,
            Complete
        }

        // ctor
        public Sync()
        {

        }

        private void CopyProductsFromITRetailDBToDesktopDB()
        {
            SqlDataReader reader;
            SqlCommand command;
            VendordDatabase.Product product;

            product = new VendordDatabase.Product();

            // insert all products from IT Retail
            using (SqlConnection conn = new SqlConnection(Constants.IT_RETAIL_DATABASE_CONNECTION_STRING))
            {
                conn.Open();
                command = new SqlCommand(@"SELECT Name, UPC, VendorName FROM Product", conn);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    product = new VendordDatabase.Product()
                    {
                        Name = Convert.ToString(reader["Name"]),
                        UPC = Convert.ToString(reader["UPC"]),
                        VendorName = Convert.ToString(reader["VendorName"])
                    };

                    product.UpsertIntoDB(new VendordDatabase());
                }
            }
        }

        private void SetRemoteDeviceDatabaseNames(RAPI.RemoteDevice remoteDevice)
        {
            string rapiApplicationData;
            string rapiApplicationDataStore;

            rapiApplicationData = remoteDevice.GetFolderPath(RAPI.SpecialFolder.ApplicationData);
            rapiApplicationDataStore = Path.Combine(rapiApplicationData, Constants.APPLICATION_NAME);
            remoteDatabase_FullPath = Path.Combine(rapiApplicationDataStore, Constants.APPLICATION_DATABASE_NAME);
            remoteDatabase_LocalCopy_FullPath = IOHelpers.AddSuffixToFilePath(Constants.VendordDatabaseFullPath, Constants.REMOTE_COPY_FLAG);
        }

        private void CopyDatabaseFromDeviceToDesktop(RAPI.RemoteDevice remoteDevice)
        {
            // does the device have a database
            if (RAPI.RemoteFile.Exists(remoteDevice, remoteDatabase_FullPath))
            {
                // yup, so copy it to the desktop
                RAPI.RemoteFile.CopyFileFromDevice(remoteDevice, remoteDatabase_FullPath, remoteDatabase_LocalCopy_FullPath, true);
            }
        }

        private SqlCeSyncProvider CreateProviderToSyncCeDatabases(string sqlCeDatabaseFullPath, string scopeName, string[] tableNames)
        {
            string connString;
            SqlCeConnection connection;
            SqlCeSyncProvider provider;
            DbSyncScopeDescription scopeDesc;
            DbSyncTableDescription tableDesc;
            SqlCeSyncScopeProvisioning provisioning;
            SqlCeSyncScopeDeprovisioning deprovisioning;

            connString = VendordDatabase.GenerateSqlCeConnString(sqlCeDatabaseFullPath);
            connection = new SqlCeConnection(connString);
            provisioning = new SqlCeSyncScopeProvisioning(connection);
            deprovisioning = new SqlCeSyncScopeDeprovisioning(connection);

            provider = new SqlCeSyncProvider(scopeName, connection);

            // delete the scope if it exists
            // this might be a performance hit
            if (provisioning.ScopeExists(scopeName))
            {
                deprovisioning.DeprovisionScope(scopeName);
            }

            // create the scope
            scopeDesc = new DbSyncScopeDescription(scopeName);
            foreach (string tableName in tableNames)
            {
                tableDesc = SqlCeSyncDescriptionBuilder.GetDescriptionForTable(tableName, connection);
                scopeDesc.Tables.Add(tableDesc);
            }

            provisioning.PopulateFromScopeDescription(scopeDesc);
            provisioning.Apply();

            return provider;
        }

        private void SyncSqlCeDatabases(string sqlCeDatabaseFullPath, string scopeName, string[] tableNames)
        {
            SyncOrchestrator orchestrator;
            SqlCeSyncProvider localProvider;
            SqlCeSyncProvider copyProvider;

            if (File.Exists(sqlCeDatabaseFullPath) && File.Exists(Constants.VendordDatabaseFullPath))
            {
                // setup providers 
                localProvider = CreateProviderToSyncCeDatabases(Constants.VendordDatabaseFullPath, scopeName, tableNames);
                copyProvider = CreateProviderToSyncCeDatabases(sqlCeDatabaseFullPath, scopeName, tableNames);

                // setup the orchestrator
                orchestrator = new SyncOrchestrator();
                orchestrator.LocalProvider = localProvider;
                orchestrator.RemoteProvider = copyProvider;
                orchestrator.Direction 
                    = SyncDirectionOrder.DownloadAndUpload; 
                    // the sync DOWNLOADS and then UPLOADS.
                    // DOWNLOADS are for order, ergo all order CRUD should happen on the mobile
                    // UPLOADS are for products, ergo all product CRUD should happen on the desktop                

                orchestrator.StateChanged += new EventHandler<SyncOrchestratorStateChangedEventArgs>(orchestrator_StateChanged);
                orchestrator.SessionProgress += new EventHandler<SyncStagedProgressEventArgs>(orchestrator_SessionProgress);

                // giver
                orchestrator.Synchronize();
            }
        }

        private void CleanUpDatabases()
        {
            VendordDatabase db = new VendordDatabase();
            db.EmptyTrash();

            VendordDatabase db_remote = new VendordDatabase(remoteDatabase_LocalCopy_FullPath);
            db_remote.EmptyTrash();
        }

        private void orchestrator_SessionProgress(object sender, SyncStagedProgressEventArgs e)
        {
            decimal percentComplete;
            SyncProviderPosition position;
            SessionProgressStage stage;

            percentComplete = e.CompletedWork / e.TotalWork;

            position = e.ReportingProvider;

            stage = e.Stage;
        }

        private void orchestrator_StateChanged(object sender, SyncOrchestratorStateChangedEventArgs e)
        {
            SyncOrchestratorState newState, oldState;

            newState = e.NewState;
            oldState = e.OldState;
        }

        private void CopyDatabaseBackToDevice(RAPI.RemoteDevice remoteDevice)
        {
            if (File.Exists(remoteDatabase_LocalCopy_FullPath))
            {
                RAPI.RemoteFile.CopyFileToDevice(remoteDevice, remoteDatabase_LocalCopy_FullPath, remoteDatabase_FullPath, true);
            }
        }

        public SyncResult MergeDesktopAndDeviceDatabases()
        {
            SyncResult result;
            string[] tablesToSync;
            mgr = new RAPI.RemoteDeviceManager();
            RAPI.RemoteDevice remoteDevice = mgr.Devices.FirstConnectedDevice;
            if (remoteDevice != null && remoteDevice.Status == RAPI.DeviceStatus.Connected)
            {
                tablesToSync = new string[] { "tblOrder", "tblProduct", "tblOrder_Product" };

                SetRemoteDeviceDatabaseNames(remoteDevice);
                CopyDatabaseFromDeviceToDesktop(remoteDevice);
                SyncSqlCeDatabases(remoteDatabase_LocalCopy_FullPath, REMOTE_DEVICE_DB_SYNC_SCOPE, tablesToSync);
                CleanUpDatabases();
                CopyDatabaseBackToDevice(remoteDevice);
                result = SyncResult.Complete;
            }
            else
            {
                result = SyncResult.Disconnected;
            }
            return result;
        }

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
            }
            return result;
        }
    }
}
