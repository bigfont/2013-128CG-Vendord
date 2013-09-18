namespace Vendord.Desktop.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using RAPI = System.Devices; // Remote API Managed Code Wrapper
    using System.IO;
    using Vendord.SmartDevice.DAL;
    using Microsoft.Synchronization;
    using Microsoft.Synchronization.Data.SqlServerCe;
    using System.Data.SqlServerCe;
    using Microsoft.Synchronization.Data;

    public class DatabaseSync
    {
        private RAPI.RemoteDeviceManager mgr;
        private string Device_AppData_DirectoryName;
        private string Device_RemoteDatabase_FileName;
        private string Desktop_AppData_DirectoryName;
        private string Desktop_RemoteDatabase_Copy_FileName;
        private string Desktop_LocalDatabase_FileName;

        private void SetDatabaseFileNames(RAPI.RemoteDevice remoteDevice)
        {
            //
            // device
            //
            Device_AppData_DirectoryName
                = Path.Combine(remoteDevice.GetFolderPath(RAPI.SpecialFolder.ApplicationData), Constants.APPLICATION_NAME);

            Device_RemoteDatabase_FileName
                = Path.Combine(Device_AppData_DirectoryName, Constants.DATABASE_NAME);

            //
            // desktop
            //
            Desktop_AppData_DirectoryName
                = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.APPLICATION_NAME);

            // remote copy
            Desktop_RemoteDatabase_Copy_FileName
                = Constants.DATABASE_NAME.Insert(Constants.DATABASE_NAME.LastIndexOf('.'), Constants.REMOTE_FLAG);

            Desktop_RemoteDatabase_Copy_FileName
                = Path.Combine(Desktop_AppData_DirectoryName, Desktop_RemoteDatabase_Copy_FileName);

            // local copy
            Desktop_LocalDatabase_FileName
                = Path.Combine(Desktop_AppData_DirectoryName, Constants.DATABASE_NAME);

        }

        private void CopyDatabaseFromDeviceToDesktop(RAPI.RemoteDevice remoteDevice)
        {
            // does the device have a database
            if (RAPI.RemoteFile.Exists(remoteDevice, Device_RemoteDatabase_FileName))
            {
                // yup, so copy it to the desktop
                RAPI.RemoteFile.CopyFileFromDevice(remoteDevice, Device_RemoteDatabase_FileName, Desktop_RemoteDatabase_Copy_FileName, true);
            }
        }

        private const string SCOPE_NAME = "sync_scope";

        private SqlCeSyncProvider CreateProvider(string databaseFileName)
        {
            string connString;
            SqlCeConnection connection;
            SqlCeSyncProvider provider;
            DbSyncScopeDescription scopeDesc;
            DbSyncTableDescription tableDesc;
            DbSyncTableDescription item;
            SqlCeSyncScopeProvisioning config;

            connString = String.Format(@"Data Source={0}", databaseFileName);
            connection = new SqlCeConnection(connString);
            config = new SqlCeSyncScopeProvisioning(connection);
            provider = new SqlCeSyncProvider(SCOPE_NAME, connection);

            if (!config.ScopeExists(SCOPE_NAME))
            {
                // add the OrderSession table to the scope
                scopeDesc = new DbSyncScopeDescription(SCOPE_NAME);
                tableDesc = SqlCeSyncDescriptionBuilder.GetDescriptionForTable("OrderSession", connection);
                scopeDesc.Tables.Add(tableDesc);

                // TODO add additional tables to the scope

                config.PopulateFromScopeDescription(scopeDesc);
                config.Apply();
            }

            return provider;
        }

        private void SyncSqlCeDatabases()
        {
            SyncOrchestrator orchestrator;
            SqlCeSyncProvider localProvider;
            SqlCeSyncProvider remoteCopyProvider;

            localProvider = CreateProvider(Desktop_LocalDatabase_FileName);
            remoteCopyProvider = CreateProvider(Desktop_RemoteDatabase_Copy_FileName);

            // setup the orchestrator
            orchestrator = new SyncOrchestrator();
            orchestrator.LocalProvider = localProvider;
            orchestrator.RemoteProvider = remoteCopyProvider;
            orchestrator.Direction = SyncDirectionOrder.UploadAndDownload;

            // giver
            orchestrator.Synchronize();
        }

        private void CopyDatabaseToDevice()
        {

        }

        public void SyncDesktopAndDeviceDatabases()
        {
            mgr = new RAPI.RemoteDeviceManager();
            RAPI.RemoteDevice remoteDevice = mgr.Devices.FirstConnectedDevice;
            SetDatabaseFileNames(remoteDevice);
            CopyDatabaseFromDeviceToDesktop(remoteDevice);
            SyncSqlCeDatabases();
        }
    }
}
