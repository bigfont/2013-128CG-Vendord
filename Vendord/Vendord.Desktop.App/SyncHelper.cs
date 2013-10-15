[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.Desktop.App
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient; // Full Edition
    using System.Data.SqlServerCe; // Compact Edition
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Synchronization;
    using Microsoft.Synchronization.Data;
    using Microsoft.Synchronization.Data.SqlServerCe;
    using Vendord.SmartDevice.Shared;
    using RAPI = System.Devices; // Remote API Managed Code Wrapper

    public class Sync
    {
        // rapi
        private RAPI.RemoteDeviceManager mgr;
        private string remoteDatabaseFullPath;
        private string remoteDatabaseLocalCopyFullPath;

        // ctor
        public Sync()
        {
        }

        // sync status
        public enum SyncResult
        {
            Disconnected,
            Complete
        }

        public SyncResult PullProductsFromITRetailDatabase()
        {
            SyncResult result;
            try
            {
                this.CopyProductsFromITRetailDBToDesktopDB();
                result = SyncResult.Complete;
            }
            catch (SqlException ex)
            {
                result = SyncResult.Disconnected;
            }

            return result;
        }

        public SyncResult MergeDesktopAndDeviceDatabases()
        {
            string[] tablesToSync;
            string scopeName;
            string objectPrefix;
            DbSyncScopeDescription scopeDesc;
            SqlCeConnection localConn;
            SqlCeConnection remoteConn;
            SyncOrchestrator orchestrator;
            SyncResult result;

            this.mgr = new RAPI.RemoteDeviceManager();
            RAPI.RemoteDevice remoteDevice = this.mgr.Devices.FirstConnectedDevice;
            if (remoteDevice != null && remoteDevice.Status == RAPI.DeviceStatus.Connected)
            {
                // Get the remote database
                this.SetRemoteDeviceDatabaseNames(remoteDevice);
                this.CopyDatabaseFromDeviceToDesktop(remoteDevice);

                // Describe the scope
                tablesToSync = new string[] { "tblOrder", "tblProduct", "tblOrder_Product" };
                scopeName = "OrdersAndProducts";
                localConn = new SqlCeConnection(VendordDatabase.GenerateSqlCeConnString(Constants.VendordDatabaseFullPath));
                scopeDesc = this.DescribeTheScope(tablesToSync, scopeName, localConn);

                // Provision the nodes
                objectPrefix = "Sync";
                this.ProvisionNode(scopeDesc, localConn, objectPrefix);

                remoteConn = new SqlCeConnection(VendordDatabase.GenerateSqlCeConnString(this.remoteDatabaseLocalCopyFullPath));
                this.ProvisionNode(scopeDesc, remoteConn, objectPrefix);

                // Set sync options
                orchestrator = SetSyncOptions(scopeDesc, localConn, remoteConn, objectPrefix);

                // Sync
                SyncTheNodes(orchestrator);

                // Clean up
                remoteConn.Close();
                localConn.Close();
                this.CleanUpDatabases();
                this.CopyDatabaseBackToDevice(remoteDevice);
                result = SyncResult.Complete;
            }
            else
            {
                result = SyncResult.Disconnected;
            }

            return result;
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
            this.remoteDatabaseFullPath = Path.Combine(rapiApplicationDataStore, Constants.APPLICATION_DATABASE_NAME);
            this.remoteDatabaseLocalCopyFullPath = IOHelpers.AddSuffixToFilePath(Constants.VendordDatabaseFullPath, Constants.REMOTE_COPY_FLAG);
        }

        private void CopyDatabaseFromDeviceToDesktop(RAPI.RemoteDevice remoteDevice)
        {
            // does the device have a database
            if (RAPI.RemoteFile.Exists(remoteDevice, this.remoteDatabaseFullPath))
            {
                // yup, so copy it to the desktop
                RAPI.RemoteFile.CopyFileFromDevice(remoteDevice, this.remoteDatabaseFullPath, this.remoteDatabaseLocalCopyFullPath, true);
            }
        }

        #region MS Sync Framework

        private DbSyncScopeDescription DescribeTheScope(string[] tablesToSync, string scopeName, SqlCeConnection localConn)
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

        private void ProvisionNode(DbSyncScopeDescription scopeDesc, SqlCeConnection conn, string objectPrefix)
        {
            SqlCeSyncScopeProvisioning provisioning;            
            provisioning = new SqlCeSyncScopeProvisioning(conn, scopeDesc);            
            provisioning.ObjectPrefix = objectPrefix;
            provisioning.SetCreateTableDefault(DbSyncCreationOption.Skip);
            if (!provisioning.ScopeExists(scopeDesc.ScopeName))
            {                
                provisioning.Apply();
            }            
        }

        private SyncOrchestrator SetSyncOptions(DbSyncScopeDescription scopeDesc, SqlCeConnection localConn, SqlCeConnection remoteConn, string objectPrefix)
        {
            SqlCeSyncProvider localProvider;
            SqlCeSyncProvider remoteProvider;
            SyncOrchestrator orchestrator;

            localProvider = new SqlCeSyncProvider(scopeDesc.ScopeName, localConn, objectPrefix);
            remoteProvider = new SqlCeSyncProvider(scopeDesc.ScopeName, remoteConn, objectPrefix);

            orchestrator = new SyncOrchestrator();
            orchestrator.LocalProvider = localProvider;
            orchestrator.RemoteProvider = remoteProvider;            
            orchestrator.Direction = SyncDirectionOrder.DownloadAndUpload;

            orchestrator.StateChanged += new EventHandler<SyncOrchestratorStateChangedEventArgs>(this.Orchestrator_StateChanged);
            orchestrator.SessionProgress += new EventHandler<SyncStagedProgressEventArgs>(this.Orchestrator_SessionProgress);

            return orchestrator;
        }

        private void SyncTheNodes(SyncOrchestrator orchestrator)
        {
            orchestrator.Synchronize();
        }

        #endregion

        private void CleanUpDatabases()
        {
            VendordDatabase db = new VendordDatabase();
            db.EmptyTrash();

            VendordDatabase db_remote = new VendordDatabase(this.remoteDatabaseLocalCopyFullPath);
            db_remote.EmptyTrash();
        }

        private void Orchestrator_SessionProgress(object sender, SyncStagedProgressEventArgs e)
        {
            decimal percentComplete;
            SyncProviderPosition position;
            SessionProgressStage stage;

            percentComplete = e.CompletedWork / e.TotalWork;

            position = e.ReportingProvider;

            stage = e.Stage;
        }

        private void Orchestrator_StateChanged(object sender, SyncOrchestratorStateChangedEventArgs e)
        {
            SyncOrchestratorState newState, oldState;

            newState = e.NewState;
            oldState = e.OldState;
        }

        private void CopyDatabaseBackToDevice(RAPI.RemoteDevice remoteDevice)
        {
            if (File.Exists(this.remoteDatabaseLocalCopyFullPath))
            {
                RAPI.RemoteFile.CopyFileToDevice(remoteDevice, this.remoteDatabaseLocalCopyFullPath, this.remoteDatabaseFullPath, true);
            }
        }
    }
}
