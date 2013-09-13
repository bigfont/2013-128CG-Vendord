
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace OpenNETCF.Desktop.Communication
{
    /// <summary>
    /// RapiConnectedHandler delegate
    /// </summary>
    public delegate void RAPIConnectedHandler();

    /// <summary>
    /// Windows CE Remote API functions
    /// </summary>
    public class RAPI : IRAPI
    {
        /// <summary>
        /// Event fired when a connection is made in asynchronous mode
        /// </summary>
        public event RAPIConnectedHandler RAPIConnected;
        /// <summary>
        /// Event fired when a connection is lost
        /// </summary>
        public event RAPIConnectedHandler RAPIDisconnected;

        private Thread m_initThread;
        private IntPtr m_hInitEvent = IntPtr.Zero;
        private int m_InitResult = 0;
        private bool m_connected = false;
        private bool m_killThread = false;
        private bool m_devicepresent = false;
        private RAPIINIT m_ri;
        private ActiveSync m_activesync;
        private int m_timeout = 0;
        private CFPerformanceMonitor m_perfmon;
        private bool disposed = false;
        private object thisLock = new object();
        private EFileCopyBufferSize m_FileCopyBufferSize = EFileCopyBufferSize.MaxSlice_4K;

        internal const int ERROR_NO_MORE_FILES = 18;
        private const short INVALID_HANDLE_VALUE = -1;
        private const short FILE_ATTRIBUTE_NORMAL = 0x80;

        /// <summary>
        /// RAPI object constructor
        /// </summary>
        public RAPI()
        {
            m_activesync = new ActiveSync();
            m_perfmon = new CFPerformanceMonitor(this);
            m_activesync.Disconnect += new DisconnectHandler(activesync_Disconnect);
            m_activesync.Active += new ActiveHandler(m_activesync_Active);
            m_activesync.BeginListen();
        }

        /// <summary>
        /// Object destructor
        /// </summary>
        ~RAPI()
        {
            Dispose(false); // Dispose only unmanaged resources.
        }

        /// <summary>
        /// Dispose code handling that properly handles 
        /// finalization code (implementation according to MSDN)
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    // Dispose managed resources					
                    // ...no implicit managed disposing needed so far
                }

                // Dispose unmanaged resources and stop threads and other components
                lock (thisLock) {
                    m_killThread = true;
                }

                if (m_activesync != null)
                    m_activesync.EndListen();

                if (m_connected) {
                    CeRapiUninit();
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        /// <summary>
        /// Exposes access to MicroSoft ActiveSync methods and events
        /// </summary>
        public ActiveSync ActiveSync
        {
            get { return m_activesync; }
        }

        /// <summary>
        /// Used to get performance statistics for a .NET Compact Framework application on a connected device
        /// <seealso cref="CFPerformanceMonitor"/><seealso cref="PerformanceStatistics"/>
        /// </summary>
        public CFPerformanceMonitor CFPerformanceMonitor
        {
            get { return m_perfmon; }
        }

        /// <summary>
        /// Connects synchronously to the remote device
        /// </summary>
        public void Connect()
        {
            Connect(true, 0);
        }

        /// <summary>
        /// Connect asynchronously to the remote device with a timeout of 0 seconds
        /// </summary>
        /// <param name="WaitForInit">If true the method blocks until RAPI Initializes or throws an error. If false the contructor does not block and the RAPIConnected event signals successful device connection.</param>
        public void Connect(bool WaitForInit)
        {
            Connect(WaitForInit, 0);
        }

        /// <summary>
        /// Connect asynchronously to the remote device with a timeout of 0 seconds
        /// </summary>
        /// <param name="WaitForInit">If true the method blocks until RAPI Initializes or throws an error. If false the contructor does not block and the RAPIConnected event signals successful device connection.</param>
        /// <param name="TimeoutSeconds">Asynchronous connections can be set to timeout after a set number of seconds. Synchronous connection wait infinitely by default (and underlying RAPI design). For asynchronous connections, a timeout value of <b>-1</b> is infinite.</param>
        public void Connect(bool WaitForInit, int TimeoutSeconds)
        {
            int ret = 0;
            m_timeout = TimeoutSeconds;

            if (WaitForInit) {
                ret = CeRapiInit();
                if (ret != 0) {
                    int e = CeRapiGetError();

                    Marshal.ThrowExceptionForHR(ret);
                }

                lock (thisLock) {
                    m_connected = true;
                }

                // throw the connected event
                OnRAPIConnected();

                return;
            }

            // non-blocking init call
            m_ri = new RAPIINIT();

            m_ri.cbSize = Marshal.SizeOf(m_ri);

            ret = CeRapiInitEx(ref m_ri);
            if (ret != 0) {
                Marshal.ThrowExceptionForHR(ret);
            }

            m_hInitEvent = m_ri.heRapiInit;

            // create a wait thread
            m_initThread = new Thread(new ThreadStart(InitThreadProc));

            // Start thread
            m_initThread.Start();
        }

        /// <summary>
        /// Disconnect from device
        /// </summary>
        public void Disconnect()
        {
            if (m_connected) {
                lock (thisLock) {
                    CeRapiUninit();
                    m_connected = false;
                }
            }
        }

        private void InitThreadProc()
        {
            uint ret = 0;
            int timeout = m_timeout * 4;
            bool infinitetimeout = (timeout < 0);

            // check for Init event 4 times / sec
            do {
                // check for abort command from Dispose()
                if (m_killThread) {
                    // clean up
                    CloseHandle(m_hInitEvent);

                    return;
                }

                // see if the event is set
                ret = WaitForSingleObject(m_ri.heRapiInit, 250);

                if ((ret == WAIT_FAILED) || (ret == WAIT_ABANDONED)) {
                    // clean up
                    CloseHandle(m_hInitEvent);

                    throw new RAPIException("Failed to Initialize RAPI");
                }

                if (!infinitetimeout) {
                    if (timeout-- < 0) {
                        // clean up
                        CloseHandle(m_hInitEvent);

                        throw new RAPIException("Timeout waiting for device connection");
                    }
                }
            } while (ret != WAIT_OBJECT_0);

            // check the hresult
            if (m_InitResult != 0) {
                Marshal.ThrowExceptionForHR(m_InitResult);
            }

            lock (thisLock) {
                m_connected = true;
            }

            // throw the connected event
            OnRAPIConnected();

            // clean up
            CloseHandle(m_hInitEvent);
        }

        private void OnRAPIConnected()
        {
            RAPIConnectedHandler handler = RAPIConnected;

            if (handler != null) {
                try {
                    handler.Invoke();
                }
                catch (Exception ex) {
                    throw new RAPIException("RAPIConnected event subscriber caused an exception.", ex);
                }
            }
        }

        private void OnRAPIDisconnected()
        {
            RAPIConnectedHandler handler = RAPIDisconnected;

            if (handler != null) {
                try {
                    handler.Invoke();
                }
                catch (Exception ex) {
                    throw new RAPIException("RAPIDisconnected event subscriber caused an exception.", ex);
                }
            }
        }

        /// <summary>
        /// Connected Property
        /// </summary>
        public bool Connected
        {
            get
            {
                return m_connected;
            }
        }

        /// <summary>
        /// Indicates whether ActiveSync currently has a connected device or not
        /// </summary>
        public bool DevicePresent
        {
            get
            {
                return m_devicepresent;
            }
        }

        /// <summary>
        /// Used to determine the buffer size to use for file copying - default is 4K
        /// </summary>
        public enum EFileCopyBufferSize
        {
            /// <summary>
            /// Maximum Slice is 4K
            /// </summary>
            MaxSlice_4K = 0x1000,
            /// <summary>
            /// Maximum Slice is 8K
            /// </summary>
            MaxSlice_8K = 0x2000,
            /// <summary>
            /// Maximum Slice is 16K
            /// </summary>
            MaxSlice_16K = 0x4000,
            /// <summary>
            /// Maximum Slice is 32K
            /// </summary>
            MaxSlice_32K = 0x8000,
            /// <summary>
            /// Maximum Slice is 64K
            /// </summary>
            MaxSlice_64K = 0x10000,
            /// <summary>
            /// Maximum Slice is 128K
            /// </summary>
            MaxSlice_128K = 0x20000,
            /// <summary>
            /// Maximum Slice is 256K
            /// </summary>
            MaxSlice_256K = 0x40000,
            /// <summary>
            /// Maximum Slice is 512K
            /// </summary>
            MaxSlice_512K = 0x80000
        }

        /// <summary>
        /// Indicates the buffer size to use for file copying - default is 4K
        /// </summary>
        public EFileCopyBufferSize FileCopyBufferSize
        {
            get { return m_FileCopyBufferSize; }
            set { m_FileCopyBufferSize = value; }
        }

        #region ------ RAPI File and directory management functions -------
        /// <summary>
        /// File Attributes
        /// </summary>
        public enum RAPIFileAttributes : int
        {
            /// <summary>
            /// File is read only
            /// </summary>
            ReadOnly = 0x0001,
            /// <summary>
            /// Hidden File
            /// </summary>
            Hidden = 0x0002,
            /// <summary>
            /// System File
            /// </summary>
            System = 0x0004,
            /// <summary>
            /// Directory
            /// </summary>
            Directory = 0x0010,
            /// <summary>
            /// Archive file
            /// </summary>
            Archive = 0x0020,
            /// <summary>
            /// File is in ROM
            /// </summary>
            InROM = 0x0040,
            /// <summary>
            /// Normal file
            /// </summary>
            Normal = 0x0080,
            /// <summary>
            /// Temporary directory
            /// </summary>
            Temporary = 0x0100,
            /// <summary>
            /// Sparse
            /// </summary>
            Sparse = 0x0200,
            /// <summary>
            /// Reparse point
            /// </summary>
            ReparsePt = 0x0400,
            /// <summary>
            /// Compressed file
            /// </summary>
            Compressed = 0x0800,
            /// <summary>
            /// Part of ROM module
            /// </summary>
            ROMModule = 0x2000
        }

        /// <summary>
        /// Time enumeration for querying FileTime
        /// </summary>
        public enum RAPIFileTime : short
        {
            /// <summary>
            /// Time file was created
            /// </summary>
            CreateTime = 1,
            /// <summary>
            /// Time of last modification
            /// </summary>
            LastModifiedTime = 2,
            /// <summary>
            /// Time of last access
            /// </summary>
            LastAccessTime = 3
        }
        // TODO: 
        // CeFindAllFiles 
        // CeSetFilePointer 
        // CeSetEndOfFile 

        /// <summary>
        /// Determines whether a file exists on the connected remote device
        /// </summary>
        /// <param name="RemoteFileName">Fully qualified path to the file or path on the device to test</param>
        /// <returns><b>true</b> if the file or directory exists, <b>false</b> if it does not</returns>
        public bool DeviceFileExists(string RemoteFileName)
        {
            // check for connection
            CheckConnection();

            uint attr = CeGetFileAttributes(RemoteFileName);

            if (attr == 0xffffffff)
                return false;

            return true;
        }

        /// <summary>
        /// Copy a device file to the connected PC
        /// </summary>
        /// <param name="LocalFileName">Name of destination file on PC</param>
        /// <param name="RemoteFileName">Name of source file on device</param>
        public void CopyFileFromDevice(string LocalFileName, string RemoteFileName)
        {
            CopyFileFromDevice(LocalFileName, RemoteFileName, false);
        }
        /// <summary>
        /// Copy a device file to the connected PC
        /// </summary>
        /// <param name="LocalFileName">Name of destination file on PC</param>
        /// <param name="RemoteFileName">Name of source file on device</param>
        /// <param name="Overwrite">Overwrites existing file on the device if <b>true</b>, fails if <b>false</b></param>
        public void CopyFileFromDevice(string LocalFileName, string RemoteFileName, bool Overwrite)
        {
            // check for connection
            CheckConnection();

            FileStream localFile;
            IntPtr remoteFile = IntPtr.Zero;
            int bytesread = 0;
            int create = Overwrite ? CREATE_ALWAYS : CREATE_NEW;
            byte[] buffer = new byte[(int)m_FileCopyBufferSize];  // default size 4k transfer buffer

            // open the remote (device) file
            remoteFile = CeCreateFile(RemoteFileName, GENERIC_READ, 0, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);

            // check for success
            if (remoteFile.ToInt64() == INVALID_HANDLE_VALUE) {
                throw new RAPIException("Could not open remote file");
            }

            // create the local file
            localFile = new FileStream(LocalFileName, Overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);

            // read data from remote file into buffer
            CeReadFile(remoteFile, buffer, (int)m_FileCopyBufferSize, ref bytesread, 0);
            while (bytesread > 0) {
                // write it into local file
                localFile.Write(buffer, 0, bytesread);

                // get more data
                if (!Convert.ToBoolean(CeReadFile(remoteFile, buffer, (int)m_FileCopyBufferSize, ref bytesread, 0))) {
                    CeCloseHandle(remoteFile);
                    localFile.Close();
                    throw new RAPIException("Failed to read device data");
                }
            }

            // close the remote file
            CeCloseHandle(remoteFile);

            localFile.Flush();

            // close the local file
            localFile.Close();
        }

        /// <summary>
        /// Copy a PC file to a connected device
        /// </summary>
        /// <param name="LocalFileName">Name of source file on PC</param>
        /// <param name="RemoteFileName">Name of destination file on device</param>
        public void CopyFileToDevice(string LocalFileName, string RemoteFileName)
        {
            CopyFileToDevice(LocalFileName, RemoteFileName, false);
        }

        /// <summary>
        /// Copy a PC file to a connected device
        /// </summary>
        /// <param name="LocalFileName">Name of source file on PC</param>
        /// <param name="RemoteFileName">Name of destination file on device</param>
        /// <param name="Overwrite">Overwrites existing file on the device if <b>true</b>, fails if <b>false</b></param>
        public void CopyFileToDevice(string LocalFileName, string RemoteFileName, bool Overwrite)
        {
            // check for connection
            CheckConnection();

            FileStream localFile;
            IntPtr remoteFile = IntPtr.Zero;
            int bytesread = 0;
            int byteswritten = 0;
            int filepos = 0;
            int create = Overwrite ? CREATE_ALWAYS : CREATE_NEW;
            byte[] buffer = new byte[(int)m_FileCopyBufferSize];  // default size 4k transfer buffer

            // create the remote file
            remoteFile = CeCreateFile(RemoteFileName, GENERIC_WRITE, 0, 0, create, FILE_ATTRIBUTE_NORMAL, 0);

            // check for success
            if (remoteFile.ToInt64() == INVALID_HANDLE_VALUE) {
                throw new RAPIException("Could not create remote file");
            }

            // open the local file
            localFile = new FileStream(LocalFileName, FileMode.Open, FileAccess.Read);

            // read 4k of data
            bytesread = localFile.Read(buffer, filepos, buffer.Length);
            while (bytesread > 0) {
                // move remote file pointer # of bytes read
                filepos += bytesread;

                // write our buffer to the remote file
                if (!Convert.ToBoolean(CeWriteFile(remoteFile, buffer, bytesread, ref byteswritten, 0))) { // check for success
                    CeCloseHandle(remoteFile);
                    throw new RAPIException("Could not write to remote file");
                }
                try {
                    // refill the local buffer
                    bytesread = localFile.Read(buffer, 0, buffer.Length);
                }
                catch (Exception) {
                    bytesread = 0;
                }
            }

            // close the local file
            localFile.Close();

            // close the remote file
            CeCloseHandle(remoteFile);

            // sync the date/times
            SetDeviceFileTime(RemoteFileName, RAPIFileTime.CreateTime, File.GetCreationTime(LocalFileName));
            SetDeviceFileTime(RemoteFileName, RAPIFileTime.LastAccessTime, DateTime.Now);
            SetDeviceFileTime(RemoteFileName, RAPIFileTime.LastModifiedTime, File.GetLastWriteTime(LocalFileName));
        }

        /// <summary>
        /// This function copies an existing device file to a new device file.
        /// </summary>
        /// <param name="SourceFile">Name of source file to copy</param>
        /// <param name="DestinationFile">Name of new, destination file</param>
        public void CopyFileOnDevice(string SourceFile, string DestinationFile)
        {
            CopyFileOnDevice(SourceFile, DestinationFile, false);
        }

        /// <summary>
        /// This function copies an existing device file to a new device file.
        /// </summary>
        /// <param name="ExistingFileName"></param>
        /// <param name="NewFileName"></param>
        /// <param name="Overwrite">Overwrites existing file on the device if <b>true</b>, fails if <b>false</b></param>
        public void CopyFileOnDevice(string ExistingFileName, string NewFileName, bool Overwrite)
        {
            CheckConnection();

            if (!Convert.ToBoolean(CeCopyFile(ExistingFileName, NewFileName, Convert.ToInt32(!Overwrite)))) {
                throw new RAPIException("Cannot copy file");
            }
        }

        /// <summary>
        /// Delete a file on the connected device
        /// </summary>
        /// <param name="FileName">File to delete</param>
        public void DeleteDeviceFile(string FileName)
        {
            CheckConnection();

            if (!Convert.ToBoolean(CeDeleteFile(FileName))) {
                throw new RAPIException("Could not delete file");
            }
        }

        /// <summary>
        /// Moves/renames an existing device file
        /// </summary>
        /// <param name="ExistingFileName">Name of existing file</param>
        /// <param name="NewFileName">New name to use for file</param>
        public void MoveDeviceFile(string ExistingFileName, string NewFileName)
        {
            CheckConnection();

            if (!Convert.ToBoolean(CeMoveFile(ExistingFileName, NewFileName))) {
                throw new RAPIException("Cannot move file");
            }
        }

        /// <summary>
        /// Get the attributes of a file on the connected device
        /// </summary>
        /// <param name="FileName">Name of file to retrieve attributes of</param>
        /// <returns>Attributes for given file name</returns>
        public RAPIFileAttributes GetDeviceFileAttributes(string FileName)
        {
            CheckConnection();

            uint ret = 0;
            ret = CeGetFileAttributes(FileName);
            if (ret == 0xFFFFFFFF) {
                throw new RAPIException("Could not get file attributes");
            }

            return (RAPIFileAttributes)ret;
        }
        /// <summary>
        /// Set the attributes for a file on the connected device
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Attributes"></param>
        public void SetDeviceFileAttributes(string FileName, RAPIFileAttributes Attributes)
        {
            CheckConnection();

            if (!Convert.ToBoolean(CeSetFileAttributes(FileName, (uint)Attributes))) {
                throw new RAPIException("Cannot set device file attributes");
            }
        }

        /// <summary>
        /// Removes a directory from the connected device.
        /// </summary>
        /// <param name="PathName">Directory to remove</param>
        /// <param name="Recurse">If <b>true</b> the call will recursively delete any subfolders and files as well, including hidden, read-only and system files</param>
        public void RemoveDeviceDirectory(string PathName, bool Recurse)
        {
            CheckConnection();

            if (!Recurse) {
                if (!Convert.ToBoolean(CeRemoveDirectory(PathName))) {
                    throw new RAPIException("Could not remove directory");
                }

                return;
            }

            FileList fi = null;

            StringBuilder wcPath = new StringBuilder(PathName);
            wcPath.Append("\\*");

            CheckConnection();

            fi = EnumFiles(wcPath.ToString());

            if (fi != null) {
                foreach (FileInformation fo in fi) {
                    if (fo.FileAttributes.ToString() == "16") {
                        StringBuilder svFullPath = new StringBuilder(PathName);
                        svFullPath.Append("\\");
                        svFullPath.Append(fo.FileName);
                        RemoveDeviceDirectory(svFullPath.ToString(), true);
                    }
                    else {
                        StringBuilder svFullPath = new StringBuilder(PathName);
                        svFullPath.Append("\\");
                        svFullPath.Append(fo.FileName);

                        RAPIFileAttributes Attribs;
                        Attribs = RAPIFileAttributes.Normal;
                        SetDeviceFileAttributes(svFullPath.ToString(), Attribs);

                        DeleteDeviceFile(svFullPath.ToString());
                    }
                }
                RemoveDeviceDirectory(PathName);
            }
            else {
                if (!Convert.ToBoolean(CeRemoveDirectory(PathName))) {
                    throw new RAPIException("Could not remove directory");
                }
            }
        }

        /// <summary>
        /// Given a path to a shortcut, returns the full path to the shortcut's target
        /// </summary>
        /// <param name="shortcutPath">Path to the shortcut</param>
        /// <returns>Path to the target</returns>
        public string GetDeviceShortcutTarget(string shortcutPath)
        {
            string targetPath = new string(' ', 255);
            if (!Convert.ToBoolean(CeSHGetShortcutTarget(shortcutPath, targetPath, 255))) {
                throw new RAPIException("Could not get target");
            }

            return targetPath.Trim();
        }

        /// <summary>
        /// Creates a shortcut
        /// </summary>
        /// <param name="ShortcutName">The fully qualifed path name, including the .lnk extension, of the shortcut to create</param>
        /// <param name="Target">Target path of the shortcut limited to 256 characters (use quoted string when target includes spaces)</param>
        /// <example>The following statement creates a shortcut on the remote desktop for the Smart Device Authentication Utility:
        /// <code>CreateDeviceShortcut("\\windows\\desktop\\.Net Debug.lnk", "\\windows\\sdauthutildevice.exe");</code>
        /// </example>
        public void CreateDeviceShortcut(string ShortcutName, string Target)
        {
            CheckConnection();

            if (!Convert.ToBoolean(CeSHCreateShortcut(ShortcutName, Target))) {
                throw new RAPIException("Could not create shortcut");
            }
        }

        /// <summary>
        /// Removes an empty directory from the connected device
        /// </summary>
        /// <param name="PathName">Directory to remove</param>
        public void RemoveDeviceDirectory(string PathName)
        {
            RemoveDeviceDirectory(PathName, false);
        }

        /// <summary>
        /// Creates a directory on the connected device
        /// </summary>
        /// <param name="PathName"></param>
        public void CreateDeviceDirectory(string PathName)
        {
            CheckConnection();

            if (!Convert.ToBoolean(CeCreateDirectory(PathName, 0))) {
                throw new RAPIException("Could not create directory");
            }
        }
        /// <summary>
        /// Get the size, in bytes, of a file on the connected device
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public long GetDeviceFileSize(string FileName)
        {
            CheckConnection();

            IntPtr hFile = IntPtr.Zero;
            uint lowsize = 0;
            uint highsize = 0;

            hFile = CeCreateFile(FileName, 0, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
            if (hFile.ToInt64() == INVALID_HANDLE_VALUE) {
                throw new RAPIException("Could not open remote file");
            }

            lowsize = CeGetFileSize(hFile, ref highsize);

            if (lowsize == uint.MaxValue) {
                CeCloseHandle(hFile);
                throw new RAPIException("Could not get file size");
            }

            CeCloseHandle(hFile);

            return lowsize + (highsize << 32);
        }

        /// <summary>
        /// Get a RAPIFileTime structure for the specified file
        /// </summary>
        /// <param name="FileName">Name of the file to check</param>
        /// <param name="DesiredTime">A RAPIFileTime</param>
        /// <returns>The DateTime for the specified value</returns>
        public DateTime GetDeviceFileTime(string FileName, RAPIFileTime DesiredTime)
        {
            CheckConnection();

            IntPtr hFile = IntPtr.Zero;
            long created = 0;
            long modified = 0;
            long accessed = 0;

            hFile = CeCreateFile(FileName, GENERIC_READ, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
            if (hFile.ToInt64() == INVALID_HANDLE_VALUE) {
                throw new RAPIException("Could not open remote file");
            }

            if (!Convert.ToBoolean(CeGetFileTime(hFile, ref created, ref accessed, ref modified))) {
                CeCloseHandle(hFile);
                throw new RAPIException("Could not get file time");
            }

            CeCloseHandle(hFile);

            switch (DesiredTime) {
                case RAPIFileTime.CreateTime:
                    return DateTime.FromFileTime(created);
                case RAPIFileTime.LastAccessTime:
                    return DateTime.FromFileTime(accessed);
                case RAPIFileTime.LastModifiedTime:
                    return DateTime.FromFileTime(modified);
                default:
                    throw new RAPIException("Invalid DesiredTime parameter");
            }
        }

        /// <summary>
        /// Modified a FileTime for the specified file
        /// </summary>
        /// <param name="FileName">File to modify</param>
        /// <param name="DesiredTime">Time to modify</param>
        /// <param name="NewTime">New time to set</param>
        public void SetDeviceFileTime(string FileName, RAPIFileTime DesiredTime, DateTime NewTime)
        {
            CheckConnection();

            IntPtr hFile = IntPtr.Zero;

            hFile = CeCreateFile(FileName, GENERIC_WRITE, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
            if (hFile.ToInt64() == INVALID_HANDLE_VALUE) {
                throw new RAPIException("Could not open remote file");
            }

            SYSTEMTIME st = new SYSTEMTIME(NewTime);
            long ft = (long)st;

            long empty = 0;
            switch (DesiredTime) {
                case RAPIFileTime.CreateTime:
                    if (!Convert.ToBoolean(CeSetFileTime(hFile, ref ft, ref empty, ref empty))) {
                        CeCloseHandle(hFile);
                        throw new RAPIException("Could not get file time");
                    }
                    break;
                case RAPIFileTime.LastAccessTime:
                    if (!Convert.ToBoolean(CeSetFileTime(hFile, ref empty, ref ft, ref empty))) {
                        CeCloseHandle(hFile);
                        throw new RAPIException("Could not get file time");
                    }
                    break;
                case RAPIFileTime.LastModifiedTime:
                    if (!Convert.ToBoolean(CeSetFileTime(hFile, ref empty, ref empty, ref ft))) {
                        CeCloseHandle(hFile);
                        throw new RAPIException("Could not get file time");
                    }
                    break;
                default:
                    throw new RAPIException("Invalid DesiredTime parameter");
            }

            CeCloseHandle(hFile);
        }

        /// <summary>
        /// Launch a process of the connected device
        /// </summary>
        /// <param name="FileName">Name of application to launch</param>
        /// <param name="CommandLine">Command line parameters to pass to application</param>
        public void CreateProcess(string FileName, string CommandLine)
        {
            CheckConnection();

            PROCESS_INFORMATION pi;

            if (CommandLine == null) {
                if (CeCreateProcess(FileName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out pi) == 0) {
                    int errnum = CeGetLastError();
                    throw new RAPIException("Error " + errnum.ToString("x") + ": Cannot Create Process");
                }
            }
            else {
                if (CeCreateProcess(FileName, CommandLine, IntPtr.Zero, IntPtr.Zero, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out pi) == 0) {
                    int errnum = CeGetLastError();
                    throw new RAPIException("Error " + errnum.ToString("x") + ": Cannot Create Process");
                }
            }
        }

        /// <summary>
        /// Launch a process of the connected device
        /// </summary>
        /// <param name="FileName">Name of application to launch</param>
        public void CreateProcess(string FileName)
        {
            CreateProcess(FileName, null);
        }

        /// <summary>
        /// Method for calling non-stream-interface custom RAPI functions
        /// </summary>
        /// <param name="DLLPath">Device path to custom RAPI library</param>
        /// <param name="FunctionName">Exported name of custom RAPI function</param>
        /// <param name="InputData">Data to send to the custom RAPI function</param>
        /// <param name="OutputData">Data received from the custom RAPI function</param>
        /// <returns>The hresult from the invoked dll function</returns>
        public int Invoke(string DLLPath, string FunctionName, byte[] InputData, out byte[] OutputData)
        {
            // RAPI memory management is non-intuitive
            // you must allocate the input variable with LocalAlloc and then RAPI will release them
            // you must also call LocalFree on the output buffer though you never call LocalAlloc

            CheckConnection();

            uint recvSize = 0;
            uint sendSize = 0;

            IntPtr recvData = IntPtr.Zero;
            IntPtr sendData = IntPtr.Zero;

            if (InputData != null) {
                sendSize = (uint)InputData.Length;

                // create a pointer to hold incoming data - RAPI will free this internally
                sendData = Marshal.AllocHGlobal(InputData.Length);

                // copy outgoing data to the pointer - too bad we don't have a memcpy fcn
                for (int i = 0; i < InputData.Length; i++) {
                    Marshal.WriteByte(sendData, i, InputData[i]);
                }
            }

            // call the RAPI function
            int hresult = CeRapiInvoke(DLLPath, FunctionName, sendSize, sendData, out recvSize, out recvData,
                                       IntPtr.Zero, 0);

            // Throw Exception if hresult contains error code
            if (hresult < 0) {
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hresult);
            }

            // allocate our managed array
            OutputData = new byte[recvSize];

            // copy the returned data only if there is any
            if (recvData != IntPtr.Zero && recvSize != 0) {
                // copy the returned data from unmanaged to managed memory
                Marshal.Copy(recvData, OutputData, 0, (int)recvSize);

                // RAPI called LocalAlloc on this internally so we must free it
                Marshal.FreeHGlobal(recvData);
            }

            return hresult;
        }

        /// <summary>
        /// Provides an ArrayList of FileInformation classes matching the criteria provided in the FileName parameter
        /// </summary>
        /// <param name="FileName">Long pointer to a null-terminated string that specifies a valid directory or path and filename, which can contain wildcard characters (* and ?).</param>
        /// <returns>An array of FileInformation objects</returns>
        public FileList EnumFiles(string FileName)
        {
            CheckConnection();

            FileList fl = new FileList();

            IntPtr hFile = IntPtr.Zero;

            FileInformation fi = new FileInformation();

            hFile = CeFindFirstFile(FileName, fi);

            if (hFile.ToInt64() != INVALID_HANDLE_VALUE) {
                fl.Add(fi);

                fi = new FileInformation();
                while (CeFindNextFile(hFile, fi) != 0) {
                    fl.Add(fi);
                    fi = new FileInformation();
                }

                CeFindClose(hFile);
            }

            return fl;
        }

        /// <summary>
        /// Provides an IEnumerable of FileInformation classes matching the criteria provided in the FileName parameter
        /// </summary>
        /// <param name="fileName">Long pointer to a null-terminated string that specifies a valid directory or path and filename, which can contain wildcard characters (* and ?).</param>
        /// <returns>An IEnumerable FileInformation objects</returns>
        [ComVisible(false)]
        public IEnumerable<FileInformation> EnumerateFiles(string fileName)
        {
            FileList fileList = EnumFiles(fileName);
            if (fileList != null) {
                for (int index = 0; index < fileList.Count; index++) {
                    yield return fileList[index];
                }
            }
        }

        /// <summary>
        /// Gets info about the connected device
        /// </summary>
        /// <param name="pSI">SYSTEM_INFO structure populated by the call</param>
        public void GetDeviceSystemInfo(out SYSTEM_INFO pSI)
        {
            CheckConnection();

            try {
                CeGetSystemInfo(out pSI);
            }
            catch (Exception) {
                throw new RAPIException("Error retrieving system info.");
            }
        }

        /// <summary>
        /// Gets the path to a system folder
        /// </summary>
        /// <param name="Folder"></param>
        /// <returns></returns>
        public string GetDeviceSystemFolderPath(SystemFolders Folder)
        {
            CheckConnection();

            StringBuilder path = new StringBuilder(260);

            if (!Convert.ToBoolean(CeGetSpecialFolderPath((int)Folder, 260, path))) {
                throw new RAPIException("Cannot get folder path!");
            }

            return path.ToString();
        }

        #endregion

        internal void CheckConnection()
        {
            if (!m_devicepresent) {
                throw new RAPIException("No connected device.");
            }
        }

        #region non-file related functions
        /// <summary>
        /// This function fills in a SYSTEM_POWER_STATUS_EX structure
        /// </summary>
        /// <param name="PowerStatus"></param>
        public void GetDeviceSystemPowerStatus(out SYSTEM_POWER_STATUS_EX PowerStatus)
        {
            CheckConnection();

            try {
                CeGetSystemPowerStatusEx(out PowerStatus, true);
            }
            catch (Exception) {
                throw new RAPIException("Error retrieving system power status.");
            }
        }

        /// <summary>
        /// This function fills in a STORE_INFORMATION structure with the size of the object store and the amount of free space currently in the object store
        /// </summary>
        /// <param name="StoreInfo"></param>
        public void GetDeviceStoreInformation(out STORE_INFORMATION StoreInfo)
        {
            CheckConnection();

            try {
                CeGetStoreInformation(out StoreInfo);
            }
            catch (Exception) {
                throw new RAPIException("Error retrieving store information.");
            }
        }

        /// <summary>
        /// This function obtains extended information about the version of the operating system of the connected device.
        /// </summary>
        /// <param name="VersionInfo"></param>
        public void GetDeviceVersion(out OSVERSIONINFO VersionInfo)
        {
            CheckConnection();

            bool b;

            VersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFO));

            b = CeGetVersionEx(out VersionInfo);

            if (!b) {
                throw new RAPIException("Error retrieving version information.", Marshal.GetLastWin32Error());
            }

        }

        /// <summary>
        /// Retrieves the memory status of the connected device
        /// </summary>
        /// <param name="ms"></param>
        public void GetDeviceMemoryStatus(out MEMORYSTATUS ms)
        {
            CheckConnection();

            CeGlobalMemoryStatus(out ms);
        }

        /// <summary>
        /// This function retrieves device-specific information about a connected device.
        /// </summary>
        /// <param name="CapabiltyToGet">Capabilty to query</param>
        /// <returns>Value reported for capability</returns>
        public int GetDeviceCapabilities(DeviceCaps CapabiltyToGet)
        {
            CheckConnection();

            return CeGetDesktopDeviceCaps((int)CapabiltyToGet);
        }

        /// <summary>
        /// Desktop equivalent of DMProcessConfigXML. Works similar to RapiConfig
        /// </summary>
        /// <param name="configXml">XML provisioning document</param>
        /// <param name="result">Resulting configuration document. Might contain error information</param>
        /// <returns>0 - success, or an HRESULT on error</returns>
        public int DeviceProcessConfigXml(string configXml, out string result)
        {
            result = configXml;
            IntPtr pOut = IntPtr.Zero;
            int ret = CeProcessConfig(configXml, 1, out pOut);
            if (pOut != IntPtr.Zero) {
                result = Marshal.PtrToStringUni(pOut);
                CeRapiFreeBuffer(pOut);
            }
            return ret;
        }

        #endregion

        #region P/Invoke declarations and constants
        private const uint WAIT_OBJECT_0 = 0x00000000;
        private const uint WAIT_ABANDONED = 0x00000080;
        private const uint WAIT_FAILED = 0xffffffff;
        private const int FILE_SHARE_READ = 0x00000001;
        private const short CREATE_NEW = 1;
        private const short CREATE_ALWAYS = 2;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint GENERIC_READ = 0x80000000;
        private const short OPEN_EXISTING = 3;

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAPIINIT
        {
            public int cbSize;
            public IntPtr heRapiInit;
            public int hrRapiInit;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessID;
            public int dwThreadID;

        } // Used
        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CeCreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            int dwShareMode,
            int lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            int hTemplateFile);

        [DllImport("kernel32.dll", EntryPoint = "WaitForSingleObject", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateEvent", SetLastError = true)]
        private static extern IntPtr CreateEvent(IntPtr lpEventAttributes, int bManualReset, int bInitialState, string lpName);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        internal static extern int CloseHandle(IntPtr hObject);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeRapiInitEx([MarshalAs(UnmanagedType.Struct)] ref RAPIINIT pRapiInit);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeRapiInit();

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeRapiGetError();

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeRapiUninit();

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeWriteFile(IntPtr hFile, byte[] lpBuffer, int nNumberOfbytesToWrite, ref int lpNumberOfbytesWritten, int lpOverlapped);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        extern static void CeRapiFreeBuffer(IntPtr pBuffer);

        [DllImport("rapi.dll", EntryPoint = "#25", CharSet = CharSet.Unicode)]
        extern static int CeProcessConfig(string config, int flags, out IntPtr pResult);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeCopyFile(string lpExistingFileName, string lpNewFileName, int bFailIfExists);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeDeleteFile(string lpFileName);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeMoveFile(string lpExistingFileName, string lpNewFileName);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint CeGetFileAttributes(string lpFileName);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeSetFileAttributes(string lpFileName, uint dwFileAttributes);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeRemoveDirectory(string lpPathName);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeCreateDirectory(string lpPathName, uint lpSecurityAttributes);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint CeGetFileSize(IntPtr hFile, ref uint lpFileSizeHigh);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeCloseHandle(IntPtr hObject);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeGetFileTime(IntPtr hFile, ref long lpCreationTime, ref long lpLastAccessTime, ref long lpLastWriteTime);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int CeSetFileTime(IntPtr hFile, ref long lpCreationTime, ref long lpLastAccessTime, ref long lpLastWriteTime);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CeGetLastError();

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CeReadFile(IntPtr hFile, byte[] lpBuffer, int nNumberOfbytesToRead, ref int lpNumberOfbytesRead, int lpOverlapped);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal extern static int CeCreateProcess(string pszImageName, string pszCmdLine, IntPtr psaProcess, IntPtr psaThread, int fInheritHandles, int fdwCreate, IntPtr pvEnvironment, IntPtr pszCurDir, IntPtr psiStartInfo, out PROCESS_INFORMATION pi);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal extern static int CeCreateProcess(string pszImageName, IntPtr pszCmdLine, IntPtr psaProcess, IntPtr psaThread, int fInheritHandles, int fdwCreate, IntPtr pvEnvironment, IntPtr pszCurDir, IntPtr psiStartInfo, out PROCESS_INFORMATION pi);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal extern static int CeRapiInvoke(string pDllPath, string pFunctionName, uint cbInput, IntPtr pInput, out uint pcbOutput, out IntPtr ppOutput, IntPtr ppIRAPIStream, uint dwReserved);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal extern static IntPtr CeFindFirstFile(string lpFileName, byte[] lpFindFileData);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CeFindNextFile(IntPtr hFindFile, byte[] lpFindFileData);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CeFindClose(IntPtr hFindFile);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CeSHCreateShortcut(string pShortcutName, string pTarget);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CeSHGetShortcutTarget(string lpszShortcut, string lpszTarget, int cbMax);

        // unused so far...
        /*
        public const short ERROR_FILE_EXISTS = 80;
        public const short ERROR_INVALID_PARAMETER = 87;
        public const short ERROR_DISK_FULL = 112;
        */
        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CeSetEndOfFile(int hFile);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CeGetSystemInfo(out SYSTEM_INFO pSI);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CeGetStoreInformation(out STORE_INFORMATION lpsi);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CeGetSystemPowerStatusEx(out SYSTEM_POWER_STATUS_EX pStatus, bool fUpdate);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CeGetSpecialFolderPath(int nFolder, uint nBufferLength, StringBuilder lpBuffer);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CeGetVersionEx(out OSVERSIONINFO lpVersionInformation);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern void CeGlobalMemoryStatus(out MEMORYSTATUS msce);

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CeGetDesktopDeviceCaps(int nIndex);
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // GC.SupressFinalize takes this object off the finalization queue
            // and prevent finalization code for this object from executing a second time.
            GC.SuppressFinalize(this);
        }

        #endregion

        private void activesync_Disconnect()
        {
            if (m_devicepresent) {
                CeRapiUninit();
                m_devicepresent = false;
            }

            lock (thisLock) {
                m_connected = false;
            }

            OnRAPIDisconnected();
        }

        private void m_activesync_Active()
        {
            m_devicepresent = true;
        }
    }

    /// <summary>
    /// Exceptions thrown by the OpenNETCF.Communications.RAPI class
    /// </summary>
    public class RAPIException : System.Exception
    {
        private int win32Error;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="Message"></param>
        public RAPIException(string Message)
            : base(Message + " " + GetErrorMessage(Marshal.GetLastWin32Error()))
        {
            this.win32Error = RAPI.CeGetLastError();
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="ex"></param>
        public RAPIException(Exception ex)
            : base(ex.Message)
        {
            this.win32Error = 0;
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public RAPIException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.win32Error = 0;
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="ErrorCode"></param>
        public RAPIException(string Message, int ErrorCode)
            : base(Message + " " + GetErrorMessage(ErrorCode))
        {
            this.win32Error = ErrorCode;
        }

        /// <summary>
        /// Win32 Error value
        /// </summary>
        public int Win32Error
        {
            get
            {
                return win32Error;
            }
        }

        internal static string GetErrorMessage(int ErrNo)
        {
            if (ErrNo == 0) {
                return "";
            }

            IntPtr pBuffer;
            int nLen = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ALLOCATE_BUFFER, 0, ErrNo, 0, out pBuffer, 0, null);
            if (nLen == 0) {
                return string.Format("Error {0} (0x{0:X})", ErrNo);
            }
            string sMsg = Marshal.PtrToStringUni(pBuffer, nLen);
            LocalFree(pBuffer);
            return sMsg;
        }

        private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        private const int FORMAT_MESSAGE_FROM_STRING = 0x00000400;
        private const int FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        private const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF;

        [DllImport("kernel32.dll", SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern int FormatMessage(int dwFlags, int lpSource, int dwMessageId, int dwLanguageId, out IntPtr lpBuffer, int nSize, int[] Arguments);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr LocalFree(IntPtr hMem);

    }

    /// <summary> 
    /// This structure describes a file found by the FindFirstFile or FindNextFile. 
    /// </summary> 
    /// <seealso cref="M:OpenNETCF.Desktop.Communication.RAPI.EnumFiles(System.String@)"/> 
    public class FileInformation //WIN32_FIND_DATA
    {
        private byte[] data = new byte[560];

        /// <summary>
        /// Byte representation of FileInformation
        /// </summary>
        public static implicit operator byte[](FileInformation fi)
        {
            return fi.data;
        }

        /// <summary>
        /// File attributes of the file found.
        /// </summary>
        public int FileAttributes
        {
            get
            {
                return BitConverter.ToInt32(data, 0);
            }
        }

        /// <summary>
        /// UTC time at which the file was created.
        /// </summary>
        public DateTime CreateTime
        {
            get
            {
                long time = BitConverter.ToInt64(data, 4);
                return DateTime.FromFileTime(time);
            }
        }

        /// <summary>
        /// UTC time at which the file was last accessed.
        /// </summary>
        public DateTime LastAccessTime
        {
            get
            {
                long time = BitConverter.ToInt64(data, 12);
                return DateTime.FromFileTime(time);
            }
        }

        /// <summary>
        /// UTC time at which the file was modified.
        /// </summary>
        public DateTime LastWriteTime
        {
            get
            {
                long time = BitConverter.ToInt64(data, 20);
                return DateTime.FromFileTime(time);
            }
        }

        /// <summary>
        /// Size, in bytes, of file
        /// </summary>
        public long FileSize
        {
            get
            {
                return BitConverter.ToInt32(data, 28) + (BitConverter.ToInt32(data, 32) << 32);
            }
        }

        /// <summary>
        /// Full name of the file
        /// </summary>
        public string FileName
        {
            get
            {
                return Encoding.Unicode.GetString(data, 40, 260).TrimEnd('\0');
            }
        }
    }


    /// <summary>
    /// Describes the current status of the Object Store
    /// </summary>
    public struct STORE_INFORMATION
    {
        /// <summary>
        /// Size of the Object Store in Bytes
        /// </summary>
        public int dwStoreSize;
        /// <summary>
        /// Free space in the Object Store in Bytes
        /// </summary>
        public int dwFreeSize;
    }

    /// <summary>
    /// OSVERSIONINFO platform type
    /// </summary>
    public enum PlatformType : int
    {
        /// <summary>
        /// Win32 on Windows CE.
        /// </summary>
        VER_PLATFORM_WIN32_CE = 3
    }

    /// <summary>
    /// Version info for the connected device
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct OSVERSIONINFO
    {
        internal int dwOSVersionInfoSize;
        /// <summary>
        /// Major
        /// </summary>
        public int dwMajorVersion;
        /// <summary>
        /// Minor
        /// </summary>
        public int dwMinorVersion;
        /// <summary>
        /// Build
        /// </summary>
        public int dwBuildNumber;
        /// <summary>
        /// Platform type
        /// </summary>
        public PlatformType dwPlatformId;
        /// <summary>
        /// Provides arbitrary additional information about the operating system
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion;
    }

    /// <summary>
    /// Device Capability COnstants (GetDeviceCapabilities)
    /// </summary>
    public enum DeviceCaps : short
    {
        /// <summary>
        /// Screen width in mm
        /// </summary>
        HorizontalSize = 4,
        /// <summary>
        /// Screen height in mm
        /// </summary>
        VerticalSize = 6,
        /// <summary>
        /// Screen width in pixels
        /// </summary>
        HorizontalResolution = 8,
        /// <summary>
        /// Screen height in raster lines
        /// </summary>
        VerticalResolution = 10,
        /// <summary>
        /// Number of adjacent color bits for each pixel
        /// </summary>
        BitsPerPixel = 12,
    }

    /// <summary>
    /// Memory information for a remote device
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORYSTATUS
    {
        internal uint dwLength;
        /// <summary>
        /// Current memory load (%)
        /// </summary>
        public int dwMemoryLoad;
        /// <summary>
        /// Total physical memory
        /// </summary>
        public int dwTotalPhys;
        /// <summary>
        /// Available Physical Memory
        /// </summary>
        public int dwAvailPhys;
        /// <summary>
        /// Total page files
        /// </summary>
        public int dwTotalPageFile;
        /// <summary>
        /// Available page files
        /// </summary>
        public int dwAvailPageFile;
        /// <summary>
        /// Totla virtual memory
        /// </summary>
        public int dwTotalVirtual;
        /// <summary>
        /// Available virtual memory
        /// </summary>
        public int dwAvailVirtual;
    }
    /// <summary>
    /// Structure for power information of mobile device
    /// </summary>
    public struct SYSTEM_POWER_STATUS_EX
    {
        /// <summary>
        /// AC Power status
        /// </summary>
        public byte ACLineStatus;
        /// <summary>
        /// Battery flag
        /// </summary>
        public byte BatteryFlag;
        /// <summary>
        /// Remaining battery life
        /// </summary>
        public byte BatteryLifePercent;
        internal byte Reserved1;
        /// <summary>
        /// Total battery life
        /// </summary>
        public int BatteryLifeTime;
        /// <summary>
        /// Battery life remaining
        /// </summary>
        public int BatteryFullLifeTime;
        internal byte Reserved2;
        /// <summary>
        /// Backup battery present
        /// </summary>
        public byte BackupBatteryFlag;
        /// <summary>
        /// Life remaining
        /// </summary>
        public byte BackupBatteryLifePercent;
        internal byte Reserved3;
        /// <summary>
        /// Life remaining
        /// </summary>
        public int BackupBatteryLifeTime;
        /// <summary>
        /// Total life when fully charged
        /// </summary>
        public int BackupBatteryFullLifeTime;
    }

    /// <summary>
    /// Processor Architecture values (GetSystemInfo)
    /// </summary>
    public enum ProcessorArchitecture : short
    {
        /// <summary>
        /// Intel
        /// </summary>
        Intel = 0,
        /// <summary>
        /// MIPS
        /// </summary>
        MIPS = 1,
        /// <summary>
        /// Alpha
        /// </summary>
        Alpha = 2,
        /// <summary>
        /// PowerPC
        /// </summary>
        PPC = 3,
        /// <summary>
        /// Hitachi SHx
        /// </summary>
        SHX = 4,
        /// <summary>
        /// ARM
        /// </summary>
        ARM = 5,
        /// <summary>
        /// IA64
        /// </summary>
        IA64 = 6,
        /// <summary>
        /// Alpha 64
        /// </summary>
        Alpha64 = 7,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = -1
    }

    /// <summary>
    /// Processor type values (GetSystemInfo)
    /// </summary>
    public enum ProcessorType : int
    {
        /// <summary>
        /// 386
        /// </summary>
        PROCESSOR_INTEL_386 = 386,
        /// <summary>
        /// 486
        /// </summary>
        PROCESSOR_INTEL_486 = 486,
        /// <summary>
        /// Pentium
        /// </summary>
        PROCESSOR_INTEL_PENTIUM = 586,
        /// <summary>
        /// P2
        /// </summary>
        PROCESSOR_INTEL_PENTIUMII = 686,
        /// <summary>
        /// IA 64
        /// </summary>
        PROCESSOR_INTEL_IA64 = 2200,
        /// <summary>
        /// MIPS 4000 series
        /// </summary>
        PROCESSOR_MIPS_R4000 = 4000,
        /// <summary>
        /// Alpha 21064
        /// </summary>
        PROCESSOR_ALPHA_21064 = 21064,
        /// <summary>
        /// PowerPC 403
        /// </summary>
        PROCESSOR_PPC_403 = 403,
        /// <summary>
        /// PowerPC 601
        /// </summary>
        PROCESSOR_PPC_601 = 601,
        /// <summary>
        /// PowerPC 603
        /// </summary>
        PROCESSOR_PPC_603 = 603,
        /// <summary>
        /// PowerPC 604
        /// </summary>
        PROCESSOR_PPC_604 = 604,
        /// <summary>
        /// PowerPC 620
        /// </summary>
        PROCESSOR_PPC_620 = 620,
        /// <summary>
        /// Hitachi SH3
        /// </summary>
        PROCESSOR_HITACHI_SH3 = 10003,
        /// <summary>
        /// Hitachi SH3E
        /// </summary>
        PROCESSOR_HITACHI_SH3E = 10004,
        /// <summary>
        /// Hitachi SH4
        /// </summary>
        PROCESSOR_HITACHI_SH4 = 10005,
        /// <summary>
        /// Motorola 821
        /// </summary>
        PROCESSOR_MOTOROLA_821 = 821,
        /// <summary>
        /// Hitachi SH3
        /// </summary>
        PROCESSOR_SHx_SH3 = 103,
        /// <summary>
        /// Hitachi SH4
        /// </summary>
        PROCESSOR_SHx_SH4 = 104,
        /// <summary>
        /// Intel StrongARM
        /// </summary>
        PROCESSOR_STRONGARM = 2577,
        /// <summary>
        /// ARM720
        /// </summary>
        PROCESSOR_ARM720 = 1824,
        /// <summary>
        /// ARM820
        /// </summary>
        PROCESSOR_ARM820 = 2080,
        /// <summary>
        /// ARM920
        /// </summary>
        PROCESSOR_ARM920 = 2336,
        /// <summary>
        /// ARM 7
        /// </summary>
        PROCESSOR_ARM_7TDMI = 70001
    }

    /// <summary>
    /// Data structure for GetSystemInfo
    /// </summary>
    public struct SYSTEM_INFO
    {
        /// <summary>
        /// Processor architecture
        /// </summary>
        public ProcessorArchitecture wProcessorArchitecture;
        internal ushort wReserved;
        /// <summary>
        /// Specifies the page size and the granularity of page protection and commitment.
        /// </summary>
        public int dwPageSize;
        /// <summary>
        /// Pointer to the lowest memory address accessible to applications and dynamic-link libraries (DLLs). 
        /// </summary>
        public int lpMinimumApplicationAddress;
        /// <summary>
        /// Pointer to the highest memory address accessible to applications and DLLs.
        /// </summary>
        public int lpMaximumApplicationAddress;
        /// <summary>
        /// Specifies a mask representing the set of processors configured into the system. Bit 0 is processor 0; bit 31 is processor 31. 
        /// </summary>
        public int dwActiveProcessorMask;
        /// <summary>
        /// Specifies the number of processors in the system.
        /// </summary>
        public int dwNumberOfProcessors;
        /// <summary>
        /// Specifies the type of processor in the system.
        /// </summary>
        public ProcessorType dwProcessorType;
        /// <summary>
        /// Specifies the granularity with which virtual memory is allocated.
        /// </summary>
        public int dwAllocationGranularity;
        /// <summary>
        /// Specifies the system’s architecture-dependent processor level.
        /// </summary>
        public short wProcessorLevel;
        /// <summary>
        /// Specifies an architecture-dependent processor revision.
        /// </summary>
        public short wProcessorRevision;
    }

    /// <summary>
    /// Parameter for SHGetSpecialFolder
    /// </summary>
    /// <remarks>Not all platforms support all of these constants.</remarks>
    public enum SystemFolders
    {
        /// <summary>
        /// </summary>
        Personal = 0x05,
        /// <summary>
        /// "\Windows\Program Files"
        /// </summary>
        Programs = 0x02,
        /// <summary>
        /// "\Windows\Favorites"
        /// </summary>
        Favorites = 0x06,
        /// <summary>
        /// "\Windows\StartUp"
        /// </summary>
        StartUp = 0x07,
        /// <summary>
        /// recent files
        /// </summary>
        Recent = 0x08,
        /// <summary>
        /// "\Windows\Desktop"
        /// </summary>
        Desktop = 0x10,
        /// <summary>
        /// "\Windows\Fonts"
        /// </summary>
        Fonts = 0x14,
        /// <summary>
        /// File system directory that serves as a common repository for application-specific data.
        /// </summary>
        AppData = 0x1a
    }
    /// <summary>
    /// Collection class of FileInformation classes
    /// </summary>
    public class FileList : CollectionBase
    {
        internal FileList()
        {
        }

        /// <summary>
        /// Returns the FileInformation at the specified index
        /// </summary>
        public FileInformation this[int index]
        {
            get
            {
                return (FileInformation)List[index];
            }
        }

        internal void Add(FileInformation fi)
        {
            List.Add(fi);
        }
    }
}

