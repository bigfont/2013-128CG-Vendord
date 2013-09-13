
/*=======================================================================================

OpenNETCF.Desktop.Communication.RAPI
OpenNETCF.Desktop.Communication.RAPIException

Copyright © 2003-2006, OpenNETCF.org

This library is free software; you can redistribute it and/or modify it under 
the terms of the OpenNETCF.org Shared Source License.

This library is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
FITNESS FOR A PARTICULAR PURPOSE. See the OpenNETCF.org Shared Source License 
for more details.

You should have received a copy of the OpenNETCF.org Shared Source License 
along with this library; if not, email licensing@opennetcf.org to request a copy.

If you wish to contact the OpenNETCF Advisory Board to discuss licensing, please 
email licensing@opennetcf.org.

For general enquiries, email enquiries@opennetcf.org or visit our website at:
http://www.opennetcf.org

=======================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace OpenNETCF.Desktop.Communication
{
    /// <summary>
    /// Windows CE Remote API functions
    /// </summary>
    public interface IRAPI : IDisposable
    {
        /// <summary>
        /// Exposes access to MicroSoft ActiveSync methods and events
        /// </summary>
        ActiveSync ActiveSync { get; }
        /// <summary>
        /// Used to get performance statistics for a .NET Compact Framework application on a connected device
        /// <seealso cref="CFPerformanceMonitor"/><seealso cref="PerformanceStatistics"/>
        /// </summary>
        CFPerformanceMonitor CFPerformanceMonitor { get; }
        /// <summary>
        /// Connects synchronously to the remote device
        /// </summary>
        void Connect();
        /// <summary>
        /// Connect asynchronously to the remote device with a timeout of 0 seconds
        /// </summary>
        /// <param name="WaitForInit">If true the method blocks until RAPI Initializes or throws an error. If false the contructor does not block and the RAPIConnected event signals successful device connection.</param>
        void Connect(bool WaitForInit);
        /// <summary>
        /// Connect asynchronously to the remote device with a timeout of 0 seconds
        /// </summary>
        /// <param name="WaitForInit">If true the method blocks until RAPI Initializes or throws an error. If false the contructor does not block and the RAPIConnected event signals successful device connection.</param>
        /// <param name="TimeoutSeconds">Asynchronous connections can be set to timeout after a set number of seconds. Synchronous connection wait infinitely by default (and underlying RAPI design). For asynchronous connections, a timeout value of <b>-1</b> is infinite.</param>
        void Connect(bool WaitForInit, int TimeoutSeconds);
        /// <summary>
        /// Connected Property
        /// </summary>
        bool Connected { get; }
        /// <summary>
        /// Copy a device file to the connected PC
        /// </summary>
        /// <param name="LocalFileName">Name of destination file on PC</param>
        /// <param name="RemoteFileName">Name of source file on device</param>
        void CopyFileFromDevice(string LocalFileName, string RemoteFileName);
        /// <summary>
        /// Copy a device file to the connected PC
        /// </summary>
        /// <param name="LocalFileName">Name of destination file on PC</param>
        /// <param name="RemoteFileName">Name of source file on device</param>
        /// <param name="Overwrite">Overwrites existing file on the device if <b>true</b>, fails if <b>false</b></param>
        void CopyFileFromDevice(string LocalFileName, string RemoteFileName, bool Overwrite);
        /// <summary>
        /// This function copies an existing device file to a new device file.
        /// </summary>
        /// <param name="ExistingFileName"></param>
        /// <param name="NewFileName"></param>
        /// <param name="Overwrite">Overwrites existing file on the device if <b>true</b>, fails if <b>false</b></param>
        void CopyFileOnDevice(string ExistingFileName, string NewFileName, bool Overwrite);
        /// <summary>
        /// This function copies an existing device file to a new device file.
        /// </summary>
        /// <param name="SourceFile">Name of source file to copy</param>
        /// <param name="DestinationFile">Name of new, destination file</param>
        void CopyFileOnDevice(string SourceFile, string DestinationFile);
        /// <summary>
        /// Copy a device file to the connected PC
        /// </summary>
        /// <param name="LocalFileName">Name of destination file on PC</param>
        /// <param name="RemoteFileName">Name of source file on device</param>
        /// <param name="Overwrite">Overwrites existing file on the device if <b>true</b>, fails if <b>false</b></param>
        void CopyFileToDevice(string LocalFileName, string RemoteFileName, bool Overwrite);
        /// <summary>
        /// Copy a PC file to a connected device
        /// </summary>
        /// <param name="LocalFileName">Name of source file on PC</param>
        /// <param name="RemoteFileName">Name of destination file on device</param>
        void CopyFileToDevice(string LocalFileName, string RemoteFileName);
        /// <summary>
        /// Creates a directory on the connected device
        /// </summary>
        /// <param name="PathName"></param>
        void CreateDeviceDirectory(string PathName);
        /// <summary>
        /// Creates a shortcut
        /// </summary>
        /// <param name="ShortcutName">The fully qualifed path name, including the .lnk extension, of the shortcut to create</param>
        /// <param name="Target">Target path of the shortcut limited to 256 characters (use quoted string when target includes spaces)</param>
        /// <example>The following statement creates a shortcut on the remote desktop for the Smart Device Authentication Utility:
        /// <code>CreateDeviceShortcut("\\windows\\desktop\\.Net Debug.lnk", "\\windows\\sdauthutildevice.exe");</code>
        /// </example>
        void CreateDeviceShortcut(string ShortcutName, string Target);
        /// <summary>
        /// Launch a process of the connected device
        /// </summary>
        /// <param name="FileName">Name of application to launch</param>
        /// <param name="CommandLine">Command line parameters to pass to application</param>
        void CreateProcess(string FileName, string CommandLine);
        /// <summary>
        /// Launch a process of the connected device
        /// </summary>
        /// <param name="FileName">Name of application to launch</param>
        void CreateProcess(string FileName);
        /// <summary>
        /// Delete a file on the connected device
        /// </summary>
        /// <param name="FileName">File to delete</param>
        void DeleteDeviceFile(string FileName);
        /// <summary>
        /// Determines whether a file exists on the connected remote device
        /// </summary>
        /// <param name="RemoteFileName">Fully qualified path to the file or path on the device to test</param>
        /// <returns><b>true</b> if the file or directory exists, <b>false</b> if it does not</returns>
        bool DeviceFileExists(string RemoteFileName);
        /// <summary>
        /// Indicates whether ActiveSync currently has a connected device or not
        /// </summary>
        bool DevicePresent { get; }
        /// <summary>
        /// Desktop equivalent of DMProcessConfigXML. Works similar to RapiConfig
        /// </summary>
        /// <param name="configXml">XML provisioning document</param>
        /// <param name="result">Resulting configuration document. Might contain error information</param>
        /// <returns>0 - success, or an HRESULT on error</returns>
        int DeviceProcessConfigXml(string configXml, out string result);
        /// <summary>
        /// Disconnect from device
        /// </summary>
        void Disconnect();
        /// <summary>
        /// Provides an ArrayList of FileInformation classes matching the criteria provided in the FileName parameter
        /// </summary>
        /// <param name="FileName">Long pointer to a null-terminated string that specifies a valid directory or path and filename, which can contain wildcard characters (* and ?).</param>
        /// <returns>An array of FileInformation objects</returns>
        FileList EnumFiles(string FileName);
        /// <summary>
        /// This function retrieves device-specific information about a connected device.
        /// </summary>
        /// <param name="CapabiltyToGet">Capabilty to query</param>
        /// <returns>Value reported for capability</returns>
        int GetDeviceCapabilities(DeviceCaps CapabiltyToGet);
        /// <summary>
        /// Get the attributes of a file on the connected device
        /// </summary>
        /// <param name="FileName">Name of file to retrieve attributes of</param>
        /// <returns>Attributes for given file name</returns>
        RAPI.RAPIFileAttributes GetDeviceFileAttributes(string FileName);
        /// <summary>
        /// Get the size, in bytes, of a file on the connected device
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        long GetDeviceFileSize(string FileName);
        /// <summary>
        /// Get a RAPIFileTime structure for the specified file
        /// </summary>
        /// <param name="FileName">Name of the file to check</param>
        /// <param name="DesiredTime">A RAPIFileTime</param>
        /// <returns>The DateTime for the specified value</returns>
        DateTime GetDeviceFileTime(string FileName, RAPI.RAPIFileTime DesiredTime);
        /// <summary>
        /// Retrieves the memory status of the connected device
        /// </summary>
        /// <param name="ms"></param>
        void GetDeviceMemoryStatus(out MEMORYSTATUS ms);
        /// <summary>
        /// Given a path to a shortcut, returns the full path to the shortcut's target
        /// </summary>
        /// <param name="shortcutPath">Path to the shortcut</param>
        /// <returns>Path to the target</returns>
        string GetDeviceShortcutTarget(string shortcutPath);
        /// <summary>
        /// This function fills in a STORE_INFORMATION structure with the size of the object store and the amount of free space currently in the object store
        /// </summary>
        /// <param name="StoreInfo"></param>
        void GetDeviceStoreInformation(out STORE_INFORMATION StoreInfo);
        /// <summary>
        /// Gets the path to a system folder
        /// </summary>
        /// <param name="Folder"></param>
        /// <returns></returns>
        string GetDeviceSystemFolderPath(SystemFolders Folder);
        /// <summary>
        /// Gets info about the connected device
        /// </summary>
        /// <param name="pSI">SYSTEM_INFO structure populated by the call</param>
        void GetDeviceSystemInfo(out SYSTEM_INFO pSI);
        /// <summary>
        /// This function fills in a SYSTEM_POWER_STATUS_EX structure
        /// </summary>
        /// <param name="PowerStatus"></param>
        void GetDeviceSystemPowerStatus(out SYSTEM_POWER_STATUS_EX PowerStatus);
        /// <summary>
        /// This function obtains extended information about the version of the operating system of the connected device.
        /// </summary>
        /// <param name="VersionInfo"></param>
        void GetDeviceVersion(out OSVERSIONINFO VersionInfo);
        /// <summary>
        /// Method for calling non-stream-interface custom RAPI functions
        /// </summary>
        /// <param name="DLLPath">Device path to custom RAPI library</param>
        /// <param name="FunctionName">Exported name of custom RAPI function</param>
        /// <param name="InputData">Data to send to the custom RAPI function</param>
        /// <param name="OutputData">Data received from the custom RAPI function</param>
        /// <returns>The hresult from the invoked dll function</returns>
        int Invoke(string DLLPath, string FunctionName, byte[] InputData, out byte[] OutputData);
        /// <summary>
        /// Moves/renames an existing device file
        /// </summary>
        /// <param name="ExistingFileName">Name of existing file</param>
        /// <param name="NewFileName">New name to use for file</param>
        void MoveDeviceFile(string ExistingFileName, string NewFileName);
        /// <summary>
        /// Event fired when a connection is made in asynchronous mode
        /// </summary>
        event RAPIConnectedHandler RAPIConnected;
        /// <summary>
        /// Event fired when a connection is lost
        /// </summary>
        event RAPIConnectedHandler RAPIDisconnected;
        /// <summary>
        /// Removes a directory from the connected device.
        /// </summary>
        /// <param name="PathName">Directory to remove</param>
        /// <param name="Recurse">If <b>true</b> the call will recursively delete any subfolders and files as well, including hidden, read-only and system files</param>
        void RemoveDeviceDirectory(string PathName, bool Recurse);
        /// <summary>
        /// Removes an empty directory from the connected device
        /// </summary>
        /// <param name="PathName">Directory to remove</param>
        void RemoveDeviceDirectory(string PathName);
        /// <summary>
        /// Set the attributes for a file on the connected device
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Attributes"></param>
        void SetDeviceFileAttributes(string FileName, RAPI.RAPIFileAttributes Attributes);
        /// <summary>
        /// Modified a FileTime for the specified file
        /// </summary>
        /// <param name="FileName">File to modify</param>
        /// <param name="DesiredTime">Time to modify</param>
        /// <param name="NewTime">New time to set</param>
        void SetDeviceFileTime(string FileName, RAPI.RAPIFileTime DesiredTime, DateTime NewTime);
        /// <summary>
        /// Provides an IEnumerable of FileInformation classes matching the criteria provided in the FileName parameter
        /// </summary>
        /// <param name="fileName">Long pointer to a null-terminated string that specifies a valid directory or path and filename, which can contain wildcard characters (* and ?).</param>
        /// <returns>An IEnumerable FileInformation objects</returns>
        [ComVisible(false)]
        IEnumerable<FileInformation> EnumerateFiles(string fileName);
    }
}
