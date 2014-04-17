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

namespace Vendord.Sync
{
    using RAPI = System.Devices;
    using System.Data.OleDb;
    using System.Xml.Linq;
    using System.ComponentModel;
    using System.Devices; // Remote API Managed Code Wrapper

    public class DbSync
    {
        // rapi       
        private string _remoteDatabaseFullPath;
        private string _remoteDatabaseLocalCopyFullPath;

        public SyncResult SyncDesktopAndDeviceDatabases(ScopeName scopeName, EventHandler<SyncStagedProgressEventArgs> progressChanged)
        {
            // assume the worste
            var result = SyncResult.Disconnected;

            // get the remote device
            var remoteDevice = GetRemoteDevice();
            if (remoteDevice == null || remoteDevice.Status != RAPI.DeviceStatus.Connected)
            {
                return result;
            }

            // we assume that we have a connected device now but sometimes it disconnected
            try
            {
                // Get the remote database
                SetRemoteDeviceDatabaseNames(remoteDevice);
                if (TryCopyDatabaseFromDeviceToDesktop(remoteDevice))
                {
                    // Instantiate the connections
                    var localConn = new SqlCeConnection(Database.GenerateSqlCeConnString(Constants.VendordMainDatabaseFullPath));
                    var remoteConn = new SqlCeConnection(Database.GenerateSqlCeConnString(_remoteDatabaseLocalCopyFullPath));

                    // Provision the nodes
                    SyncFrameworkWrapper.AddAllScopesToAllNodes(localConn, remoteConn);

                    // Set sync options
                    var orchestrator = SyncFrameworkWrapper.SetSyncOptions(scopeName, localConn, remoteConn, progressChanged);

                    // Sync
                    SyncFrameworkWrapper.SyncTheNodes(orchestrator);

                    // Clean up
                    remoteConn.Close();
                    localConn.Close();
                    CleanUpDatabases();
                    CopyDatabaseBackToDevice(remoteDevice);

                    // success!
                    result = SyncResult.Complete;
                }
                else
                {
                    result = SyncResult.NoRemoteDatabase;
                }
            }
            catch (InvalidOperationException e)
            {
                IOHelpers.LogException(e);
            }
            catch (RAPI.RapiException e)
            {
                IOHelpers.LogException(e);
            }
            finally
            {
                remoteDevice.Dispose();
            }

            return result;
        }

        private RemoteDevice GetRemoteDevice()
        {
            // get the remote device
            var mgr = new RAPI.RemoteDeviceManager();
            RemoteDevice remoteDevice = mgr.Devices.FirstConnectedDevice;
            return remoteDevice;
        }

        private void SetRemoteDeviceDatabaseNames(RAPI.RemoteDevice remoteDevice)
        {
            var rapiApplicationData = remoteDevice.GetFolderPath(RAPI.SpecialFolder.MyDocuments);
            var rapiApplicationDataStore = Path.Combine(rapiApplicationData, Constants.ApplicationName);
            _remoteDatabaseFullPath = Path.Combine(rapiApplicationDataStore, Constants.ApplicationDatabaseName);
            _remoteDatabaseLocalCopyFullPath = IOHelpers.AddSuffixToFilePath(Constants.VendordMainDatabaseFullPath,
                Constants.RemoteCopyFlag);
        }

        private bool TryCopyDatabaseFromDeviceToDesktop(RAPI.RemoteDevice remoteDevice)
        {
            bool result = false;

            // does the device have a database
            if (RAPI.RemoteFile.Exists(remoteDevice, _remoteDatabaseFullPath))
            {
                // yup, so copy it to the desktop
                RAPI.RemoteFile.CopyFileFromDevice(remoteDevice, _remoteDatabaseFullPath, _remoteDatabaseLocalCopyFullPath,
                    true);
                result = true;
            }

            return result;
        }

        private void CleanUpDatabases()
        {
            var db = new Database();
            db.EmptyTrash();

            var dbRemote = new Database(_remoteDatabaseLocalCopyFullPath);
            dbRemote.EmptyTrash();
        }

        private void CopyDatabaseBackToDevice(RemoteDevice remoteDevice)
        {
            if (File.Exists(_remoteDatabaseLocalCopyFullPath))
            {
                RAPI.RemoteFile.CopyFileToDevice(remoteDevice, _remoteDatabaseLocalCopyFullPath, _remoteDatabaseFullPath, true);
            }
        }
    }
}