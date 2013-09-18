namespace Vendord.Desktop.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using RAPI = System.Devices; // Remote API Managed Code Wrapper
    using System.IO;
    using Vendord.SmartDevice.DAL;

    public class SyncUtil
    {                
        private RAPI.RemoteDeviceManager mgr;
        private void CopyDatabaseFromDeviceToDesktop(RAPI.RemoteDevice remoteDevice)
        {
            string deviceAppDataDirectoryName;     
            string deviceFileName;
            string desktopAppDataDirectoryName;            
            string desktopDatabaseName;
            string desktopFileName;

            // device file name
            deviceAppDataDirectoryName
                = Path.Combine(remoteDevice.GetFolderPath(RAPI.SpecialFolder.ApplicationData), Constants.APPLICATION_NAME);                       

            deviceFileName
                = Path.Combine(deviceAppDataDirectoryName, Constants.DATABASE_NAME);

            // desktop file name
            desktopAppDataDirectoryName
                = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.APPLICATION_NAME);

            desktopDatabaseName
                = Constants.DATABASE_NAME.Insert(Constants.DATABASE_NAME.LastIndexOf('.'), Constants.REMOTE_FLAG);

            desktopFileName
                = Path.Combine(desktopAppDataDirectoryName, desktopDatabaseName);

            // does the device have the file
            if (RAPI.RemoteFile.Exists(remoteDevice, deviceFileName))
            {
                // yup, so copy it to the desktop
                RAPI.RemoteFile.CopyFileFromDevice(remoteDevice, deviceFileName, desktopFileName, true);
            }
        }

        private void SyncSqlCeDatabases()
        {
            
        }

        private void CopyDatabaseToDevice()
        { 
            
        }

        public void SyncDesktopAndDeviceDatabases()
        {
            mgr = new RAPI.RemoteDeviceManager();
            RAPI.RemoteDevice remoteDevice = mgr.Devices.FirstConnectedDevice;
            CopyDatabaseFromDeviceToDesktop(remoteDevice);
            SyncSqlCeDatabases();
        }
    }
}
