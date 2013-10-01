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

    public class DatabaseSync
    {
        // rapi
        private const string REMOTE_DEVICE_TEMP_COPY = "_REMOTE_DEVICE_TEMP_COPY";
        private const string REMOTE_DEVICE_DB_SYNC_SCOPE = "REMOTE_DEVICE_DB_SYNC_SCOPE";

        // rapi
        private RAPI.RemoteDeviceManager mgr;
        private string rapiDatabase_Path;
        private string rapiDatabaseLocalCopy_Path;

        // sync status
        public SyncResultMessage SyncDisconnected;
        public SyncResultMessage SyncComplete;
        public class SyncResultMessage
        {
            public string Caption;
            public string Message;
            public SyncResultMessage(string message, string caption)
            {
                this.Caption = caption;
                this.Message = message;
            }
        }

        // ctor
        public DatabaseSync()
        {
            SyncComplete = new SyncResultMessage("Success", "The sync is complete.");
            SyncDisconnected = new SyncResultMessage("Disconnected", "The device is disconnected. Please connect it and try again.");
        }

        private void CopyProductsFromITRetailDBToDesktopDB()
        {
            SqlDataReader reader;
            SqlCommand command;
            VendordDatabase.Product product;

            // delete all existing products
            product = new VendordDatabase.Product();
            product.DeleteAll();

            // insert all products from IT Retail
            using (SqlConnection conn = new SqlConnection(Constants.IT_RETAIL_DATABASE_CONNECTION_STRING))
            {
                conn.Open();
                command = new SqlCommand(@"SELECT Name, UPC FROM Product", conn);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    product = new VendordDatabase.Product()
                    {
                        Name = Convert.ToString(reader["Name"]),
                        UPC = Convert.ToString(reader["UPC"])
                    };

                    product.UpsertIntoDB();
                }
            }
        }

        private void SetRemoteDeviceDatabaseNames(RAPI.RemoteDevice remoteDevice)
        {
            string rapiApplicationData;
            string rapiApplicationDataStore;

            rapiApplicationData = remoteDevice.GetFolderPath(RAPI.SpecialFolder.ApplicationData);
            rapiApplicationDataStore = Path.Combine(rapiApplicationData, Constants.APPLICATION_NAME);
            rapiDatabase_Path = Path.Combine(rapiApplicationDataStore, Constants.APPLICATION_DATABASE_NAME);
            rapiDatabaseLocalCopy_Path = IOHelpers.AddSuffixToFilePath(Constants.VendordDatabaseFullPath, REMOTE_DEVICE_TEMP_COPY);
        }

        private void CopyDatabaseFromDeviceToDesktop(RAPI.RemoteDevice remoteDevice)
        {
            // does the device have a database
            if (RAPI.RemoteFile.Exists(remoteDevice, rapiDatabase_Path))
            {
                // yup, so copy it to the desktop
                RAPI.RemoteFile.CopyFileFromDevice(remoteDevice, rapiDatabase_Path, rapiDatabaseLocalCopy_Path, true);
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
                orchestrator.Direction = SyncDirectionOrder.UploadAndDownload;

                orchestrator.StateChanged += new EventHandler<SyncOrchestratorStateChangedEventArgs>(orchestrator_StateChanged);
                orchestrator.SessionProgress += new EventHandler<SyncStagedProgressEventArgs>(orchestrator_SessionProgress);

                // giver
                orchestrator.Synchronize();
            }
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
            if (File.Exists(rapiDatabaseLocalCopy_Path))
            {
                RAPI.RemoteFile.CopyFileToDevice(remoteDevice, rapiDatabaseLocalCopy_Path, rapiDatabase_Path, true);
            }
        }

        public SyncResultMessage SyncDesktopAndDeviceDatabases()
        {
            mgr = new RAPI.RemoteDeviceManager();
            RAPI.RemoteDevice remoteDevice = mgr.Devices.FirstConnectedDevice;
            if (remoteDevice != null && remoteDevice.Status == RAPI.DeviceStatus.Connected)
            {
                SetRemoteDeviceDatabaseNames(remoteDevice);
                CopyDatabaseFromDeviceToDesktop(remoteDevice);
                SyncSqlCeDatabases(rapiDatabaseLocalCopy_Path, REMOTE_DEVICE_DB_SYNC_SCOPE, new string[] { "OrderSession", "Product", "OrderSession_Product" });
                CopyDatabaseBackToDevice(remoteDevice);
                return SyncComplete;
            }
            else
            {
                return SyncDisconnected;
            }
        }

        public SyncResultMessage SyncDesktopAndITRetailDatabase()
        {
            CopyProductsFromITRetailDBToDesktopDB();
            return SyncComplete;
        }
    }
}
