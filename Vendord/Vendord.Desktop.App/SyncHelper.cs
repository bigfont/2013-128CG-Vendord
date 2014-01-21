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

        private string _remoteDatabaseFullPath;
        private string _remoteDatabaseLocalCopyFullPath;

        public SyncResult PullProductsFromItRetailDatabase()
        {
            SyncResult result;
            try
            {
                CopyProductsFromItRetailDbToDesktopDb();
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
                var localConn = new SqlCeConnection(Database.GenerateSqlCeConnString(Constants.DatabaseFullPath));
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

        private static void CopyProductsFromItRetailDbToDesktopDb()
        {
            // insert all products from IT Retail
            using (var conn = new SqlConnection(Constants.ItRetailDatabaseConnectionString))
            {
                conn.Open();
                var command = new SqlCommand(@"SELECT Name, UPC, VendorName FROM Product", conn);
                var reader = command.ExecuteReader();
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
            var rapiApplicationData = remoteDevice.GetFolderPath(RAPI.SpecialFolder.ApplicationData);
            var rapiApplicationDataStore = Path.Combine(rapiApplicationData, Constants.ApplicationName);
            _remoteDatabaseFullPath = Path.Combine(rapiApplicationDataStore, Constants.ApplicationDatabaseName);
            _remoteDatabaseLocalCopyFullPath = IOHelpers.AddSuffixToFilePath(Constants.DatabaseFullPath,
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