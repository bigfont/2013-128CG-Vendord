﻿[module:
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
                    this.SetRemoteDeviceDatabaseNames(remoteDevice);
                    this.CopyDatabaseFromDeviceToDesktop(remoteDevice);

                    // Instantiate the connections
                    localConn = new SqlCeConnection(Database.GenerateSqlCeConnString(Constants.DatabaseFullPath));
                    remoteConn = new SqlCeConnection(Database.GenerateSqlCeConnString(this.remoteDatabaseLocalCopyFullPath));

                    // Describe the scope
                    tablesToSync = new string[] { "tblOrder", "tblProduct", "tblOrderProduct" };
                    scopeName = "OrdersAndProducts";
                    localScopeDesc = this.DescribeTheScope(tablesToSync, scopeName, localConn);
                    remoteScopeDesc = this.DescribeTheScope(tablesToSync, scopeName, remoteConn);

                    // Provision the nodes
                    this.ProvisionNode(localScopeDesc, localConn);
                    this.ProvisionNode(remoteScopeDesc, remoteConn);

                    // Set sync options
                    orchestrator = SetSyncOptions(localScopeDesc, localConn, remoteConn);

                    // Sync
                    SyncTheNodes(orchestrator);

                    // Clean up
                    remoteConn.Close();
                    localConn.Close();
                    this.CleanUpDatabases();
                    this.CopyDatabaseBackToDevice(remoteDevice);

                    // success!
                    result = SyncResult.Complete;
                }
                catch (System.InvalidOperationException e)
                {
                    IOHelpers.LogException(e);
                }
                catch (System.Devices.RapiException e)
                { 
                
                }
            }            

            return result;
        }

        private void CopyProductsFromITRetailDBToDesktopDB()
        {
            SqlDataReader reader;
            SqlCommand command;
            Product product;

            product = new Product();

            // insert all products from IT Retail
            using (SqlConnection conn = new SqlConnection(Constants.ItRetailDatabaseConnectionString))
            {
                conn.Open();
                command = new SqlCommand(@"SELECT Name, UPC, VendorName FROM Product", conn);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    product = new Product()
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
            this.remoteDatabaseFullPath = Path.Combine(rapiApplicationDataStore, Constants.ApplicationDatabaseName);
            this.remoteDatabaseLocalCopyFullPath = IOHelpers.AddSuffixToFilePath(Constants.DatabaseFullPath, Constants.RemoteCopyFlag);
        }

        private void CopyDatabaseFromDeviceToDesktop(RAPI.RemoteDevice remoteDevice)
        {
            // does the device have a database
            if (RAPI.RemoteFile.Exists(remoteDevice, this.remoteDatabaseFullPath))
            {
                // yup, so copy it to the desktop
                RAPI.RemoteFile.CopyFileFromDevice(remoteDevice, this.remoteDatabaseFullPath, this.remoteDatabaseLocalCopyFullPath, true);
            }
            else
            {
                RAPI.RemoteFile.Create(remoteDevice, this.remoteDatabaseFullPath);
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

        private SyncOrchestrator SetSyncOptions(DbSyncScopeDescription scopeDesc, SqlCeConnection localConn, SqlCeConnection remoteConn)
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

        private void CleanUpDatabases()
        {
            Database db = new Database();
            db.EmptyTrash();

            Database db_remote = new Database(this.remoteDatabaseLocalCopyFullPath);
            db_remote.EmptyTrash();
        }

        private void CopyDatabaseBackToDevice(RAPI.RemoteDevice remoteDevice)
        {
            if (File.Exists(this.remoteDatabaseLocalCopyFullPath) & RAPI.RemoteFile.Exists(remoteDevice, this.remoteDatabaseFullPath))
            {
                RAPI.RemoteFile.CopyFileToDevice(remoteDevice, this.remoteDatabaseLocalCopyFullPath, this.remoteDatabaseFullPath, true);
            }
        }
    }
}
