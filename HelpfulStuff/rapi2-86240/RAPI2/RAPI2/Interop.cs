using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace System.Devices.Interop
{
	#region Structs

	[StructLayout(LayoutKind.Sequential)]
	internal class CFILETIME
	{
		public CFILETIME(DateTime dt)
		{
			long hFT1 = dt.ToFileTimeUtc();
			this.dwLowDateTime = (int)(hFT1 & 0xFFFFFFFF);
			this.dwHighDateTime = (int)(hFT1 >> 32);
		}
		public int dwLowDateTime;
		public int dwHighDateTime;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;
		public IntPtr hThread;
		public int dwProcessID;
		public int dwThreadID;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct CEBLOB
	{
		public CEBLOB(byte[] bytes)
		{
			size = bytes.Length;
			ptr = Marshal.AllocHGlobal(size);
			Marshal.Copy(bytes, 0, ptr, size);
		}
		int size;
		IntPtr ptr;
		public byte[] Data
		{
			get
			{
				byte[] bytes = new byte[size];
				Marshal.Copy(ptr, bytes, 0, size);
				return bytes;
			}
		}
	}

	[StructLayout(LayoutKind.Explicit, Size = 8)]
	internal struct CEVALUNION
	{
		[FieldOffset(0)]
		public short iVal;     //@field CEVT_I2
		[FieldOffset(0)]
		public ushort uiVal;    //@field CEVT_UI2
		[FieldOffset(2)]
		private short pad1;
		[FieldOffset(0)]
		public int lVal;     //@field CEVT_I4
		[FieldOffset(0)]
		public uint ulVal;    //@field CEVT_UI4
		[FieldOffset(4)]
		private short pad2;
		//@field CEVT_AUTO_I4_
		[FieldOffset(0)]
		public long filetime; //@field CEVT_FILETIME 
		[FieldOffset(0)]
		public IntPtr lpwstr;   //@field CEVT_LPWSTR - Ptr to null terminated string
		[FieldOffset(0)]
		public IntPtr blob;     //@field CEVT_BLOB - DWORD count, and Ptr to bytes
		//@field CEVT_AUTO_I8
		//@field CEVT_RECID
		//@field CEVT_STREAM
		[FieldOffset(0)]
		public int boolVal;  //@field CEVT_BOOL
		[FieldOffset(0)]
		public double dblVal;   //@field CEVT_R8
	}

	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
	internal struct CE_FIND_DATA 
	{
		[FieldOffset( 0)] public uint      dwFileAttributes;
		[FieldOffset( 4)] public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
		[FieldOffset(12)] public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
		[FieldOffset(20)] public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
		[FieldOffset(28)] public uint      nFileSizeHigh;
		[FieldOffset(32)] public uint      nFileSizeLow;
		[FieldOffset(36)] public uint      dwOID;
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=260), FieldOffset(40)] public string Name;
	}

	[StructLayout(LayoutKind.Explicit, Size = 544, Pack = 2)]
	internal struct CEOIDINFO
	{
		[FieldOffset(0)]
		public ushort wObjType;
		[FieldOffset(4), MarshalAs(UnmanagedType.ByValArray, SizeConst = 540)]
		public byte[] inf;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 2)]
	internal struct CEFILEINFO
	{
		public uint dwAttributes;
		public uint oidParent;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szFileName;
		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastChanged;
		public uint dwLength;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 2)]
	internal struct CEDIRINFO
	{
		public uint dwAttributes;
		public uint oidParent;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szDirName;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct CERECORDINFO
	{
		public uint oidParent;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 2)]
	internal struct CEDBASEINFO 
	{
		public uint dwFlags;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string szDbaseName;
		public uint dwDbaseType;
		public ushort wNumRecords;
		public ushort wNumSortOrder;
		public uint dwSize;
		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastModified;
		//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeConst = 4)]
		//public SortOrderDescriptor[] rgSortSpecs;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct CEDB_FIND_DATA
	{
		public uint OidDb;
		public CEDBASEINFO DbInfo;
	}

	/// <summary>
	/// Version info for the connected device
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CEOSVERSIONINFO
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
		public int dwPlatformId;
		/// <summary>
		/// Null-terminated string that provides arbitrary additional information about the operating system.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string szCSDVersion;
	}

	internal enum RAPI_GETDEVICEOPCODE
	{
		RAPI_GETDEVICE_NONBLOCKING,
		RAPI_GETDEVICE_BLOCKING
	}

	[StructLayout(LayoutKind.Explicit, Size = 128, CharSet = CharSet.Ansi)]
	internal struct SOCKADDR_STORAGE
	{
		[FieldOffset(0)]
		short ss_family;
		[FieldOffset(2)]
		ushort port;
		[FieldOffset(4)]
		uint addr;

		public System.Net.Sockets.AddressFamily AddressFamily { get { return (Net.Sockets.AddressFamily)ss_family; } }
		public System.Net.IPEndPoint IPAddress
		{
			get
			{
				try { return new Net.IPEndPoint(addr, port); }
				catch (Exception ex) { System.Diagnostics.Debug.WriteLine(string.Format("Invalid IP Address from {1:x}:{2}\n{0}", ex, addr, port)); }
				return new Net.IPEndPoint(Net.IPAddress.Any, 0);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Size=264)]
	internal struct RAPI_CONNECTIONINFO
	{
		private SOCKADDR_STORAGE _ipaddr;
		private SOCKADDR_STORAGE _hostIpaddr;
		public ConnectionType connectionType;

		public System.Net.IPEndPoint ipaddr { get { return _ipaddr.IPAddress; } }
		public System.Net.IPEndPoint hostIpaddr { get { return _hostIpaddr.IPAddress; } }
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct RAPI_DEVICEINFO
	{
		public Guid DeviceId;
		public int dwOsVersionMajor;
		public int dwOsVersionMinor;
		[MarshalAs(UnmanagedType.BStr)]
		public string bstrName;
		[MarshalAs(UnmanagedType.BStr)]
		public string bstrPlatform;

		public bool IsEmpty { get { return DeviceId == Guid.Empty; } }
	}

	internal static class HKEY
	{
		public static readonly uint HKEY_CLASSES_ROOT = 0x80000000;
		public static readonly uint HKEY_CURRENT_USER = 0x80000001;
		public static readonly uint HKEY_LOCAL_MACHINE = 0x80000002;
		public static readonly uint HKEY_USERS = 0x80000003;
	}

	#endregion

	[ComImport, Guid("35440327-1517-4B72-865E-3FFE8E97002F")]
	internal class RAPI2
	{
	}

	[Guid("76a78b7d-8e54-4c06-ac38-459e6a1ab5e3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IRAPISession
	{
		void CeRapiInit();

		void CeRapiUninit();

		[PreserveSig]
		int CeGetLastError();

		[PreserveSig]
		int CeRapiGetError();

		void CeRapiFreeBuffer(
			[In] IntPtr Buffer);

		[PreserveSig]
		IntPtr CeFindFirstFile(
			[In, MarshalAs(UnmanagedType.LPWStr)] string FileName,
			[In, Out, MarshalAs(UnmanagedType.Struct)] ref CE_FIND_DATA FindData);

		[PreserveSig]
		int CeFindNextFile(
			[In] IntPtr FoundFile,
			[In, Out, MarshalAs(UnmanagedType.Struct)] ref CE_FIND_DATA FindData);

		[PreserveSig]
		int CeFindClose(
			[In] IntPtr FoundFile);

		[PreserveSig]
		uint CeGetFileAttributes(
			[In, MarshalAs(UnmanagedType.LPWStr)] string FileName);

		[PreserveSig]
		int CeSetFileAttributes(
			[In, MarshalAs(UnmanagedType.LPWStr)] string FileName,
			[In] uint FileAttrib);
		
		[PreserveSig]
		IntPtr CeCreateFile( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
			[In] uint dwDesiredAccess,
			[In] uint dwShareMode,
			[In] IntPtr lpSecurityAttributes,
			[In] uint dwCreationDistribution,
			[In] uint dwFlagsAndAttributes,
			[In] IntPtr hTemplateFile);

		[PreserveSig]
		int CeReadFile( 
			[In] IntPtr hFile,
			[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] lpBuffer,
			[In] uint nNumberOfBytesToRead,
			[In, Out] ref int lpNumberOfBytesRead,
			[In] IntPtr lpOverlapped);

		[PreserveSig]
		int CeWriteFile( 
			[In] IntPtr hFile,
			[In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] lpBuffer,
			[In] int nNumberOfBytesToWrite,
			[In, Out] ref int lpNumberOfBytesWritten,
			[In] IntPtr lpOverlapped);

		[PreserveSig]
		int CeCloseHandle(
			[In] IntPtr hObject);
		
		[PreserveSig]
		int CeFindAllFiles( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string Path,
			[In] int Flags,
			[In, Out] ref int pFoundCount,
			[Out] out IntPtr ppFindDataArray);
		
		[PreserveSig]
		IntPtr CeFindFirstDatabase( 
			[In] uint dwDbaseType);

		[PreserveSig]
		uint CeFindNextDatabase( 
			[In] IntPtr hEnum);

		[PreserveSig]
		int CeOidGetInfo([In] uint oid, [In, Out, MarshalAs(UnmanagedType.Struct)] ref CEOIDINFO poidInfo);

		[PreserveSig]
		uint CeCreateDatabase( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszName,
			[In] uint dwDbaseType,
			[In] ushort cNumSortOrder,
			[In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] SortOrderDescriptor[] rgSortSpecs);

		[PreserveSig]
		IntPtr CeOpenDatabase( 
			[In, Out] ref uint poid,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszName,
			[In] uint propid,
			[In] int dwFlags,
			[In] IntPtr hwndNotify);

		[PreserveSig]
		int CeDeleteDatabase( 
			[In] uint oidDbase);

		[PreserveSig]
		uint CeReadRecordProps( 
			[In] IntPtr hDbase,
			[In] uint dwFlags,
			[In, Out] ref ushort lpcPropID,
			//[In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rgPropID,
			[In] uint[] rgPropID,
			[In, Out] ref IntPtr lplpBuffer,
			[In, Out] ref int lpcbBuffer);

		[PreserveSig]
		uint CeWriteRecordProps( 
			[In] IntPtr hDbase,
			[In] uint oidRecord,
			[In] ushort cPropID,
			[In, MarshalAs(UnmanagedType.LPArray)] RemoteDatabase.PropertyValue[] rgPropVal);

		[PreserveSig]
		int CeDeleteRecord( 
			[In] IntPtr hDatabase,
			[In] uint oidRecord);

		[PreserveSig]
		uint CeSeekDatabase( 
			[In] IntPtr hDatabase,
			[In] uint dwSeekType,
			[In] uint dwValue,
			[In, Out] ref int lpdwIndex);

		[PreserveSig]
		int CeSetDatabaseInfo( 
			[In] uint oidDbase,
			ref IntPtr /*CEDBASEINFO*/ pNewInfo);

		[PreserveSig]
		uint CeSetFilePointer( 
			[In] IntPtr hFile,
			[In] int lDistanceToMove,
			[In, Out] ref int lpDistanceToMoveHigh,
			[In] uint dwMoveMethod);

		[PreserveSig]
		int CeSetEndOfFile( 
			[In] IntPtr hFile);

		[PreserveSig]
		int CeCreateDirectory( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpPathName,
			[In] IntPtr lpSecurityAttributes);

		[PreserveSig]
		int CeRemoveDirectory( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpPathName);

		[PreserveSig]
		int CeCreateProcess( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszImageName,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszCmdLine,
			[In] int lpsaProcess,
			[In] int lpsaThread,
			[In] int fInheritHandles,
			[In] int fdwCreate,
			[In] int lpvEnvironment,
			[In] int lpszCurDir,
			[In] int lpsiStartInfo,
			[Out] out PROCESS_INFORMATION lppiProcInfo);

		[PreserveSig]
		int CeMoveFile( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpExistingFileName,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpNewFileName);

		[PreserveSig]
		int CeCopyFile( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpExistingFileName,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpNewFileName,
			[In] int bFailIfExists);

		[PreserveSig]
		int CeDeleteFile( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string FileName);

		[PreserveSig]
		uint CeGetFileSize( 
			[In] IntPtr hFile,
			[In, Out] ref uint lpFileSizeHigh);

		[PreserveSig]
		int CeRegOpenKeyEx( 
			[In] uint hKey,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszSubKey,
			[In] uint dwReserved,
			[In] uint samDesired,
			[In, Out] ref uint phkResult);

		[PreserveSig]
		int CeRegEnumKeyEx(
			[In] uint hKey,
			[In] uint dwIndex,
			[In, Out] System.Text.StringBuilder lpName,
			[In, Out] ref uint lpcbName,
			[In] int lpReserved,
			[In, Out] System.Text.StringBuilder lpClass,
			[In, Out] ref uint lpcbClass,
			[In] IntPtr lpftLastWriteTime);

		[PreserveSig]
		int CeRegCreateKeyEx(
			[In] uint hKey,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszSubKey,
			[In] int dwReserved,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszClass,
			[In] uint fdwOptions,
			[In] uint samDesired,
			[In] IntPtr lpSecurityAttributes,
			[In, Out] ref uint phkResult,
			[In, Out] ref uint lpdwDisposition);

		[PreserveSig]
		int CeRegCloseKey(
			[In] uint hKey);

		[PreserveSig]
		int CeRegDeleteKey(
			[In] uint hKey,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszSubKey);

		[PreserveSig]
		int CeRegEnumValue(
			[In] uint hKey,
			[In] uint dwIndex,
			[In, Out] System.Text.StringBuilder lpszValueName,
			[In, Out] ref uint lpcbValueName,
			[In, Out] int lpReserved,
			[Out] out uint lpType,
			[In, Out] IntPtr lpData,
			[In, Out] ref uint lpcbData);

		[PreserveSig]
		int CeRegDeleteValue(
			[In] uint hKey,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszValueName);

		[PreserveSig]
		int CeRegQueryInfoKey(
			[In] uint hKey,
			[Out, MarshalAs(UnmanagedType.LPWStr)] string lpClass,
			[In, Out] ref int lpcbClass,
			[In] IntPtr lpReserved,
			[Out] out int lpcSubKeys,
			[Out] out int lpcbMaxSubKeyLen,
			[Out] out int lpcbMaxClassLen,
			[Out] out int lpcValues,
			[Out] out int lpcbMaxValueNameLen,
			[Out] out int lpcbMaxValueLen,
			[In] IntPtr lpcbSecurityDescriptor,
			[In] IntPtr lpftLastWriteTime);

		[PreserveSig]
		int CeRegQueryValueEx(
			[In] uint hKey,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpValueName,
			[In] IntPtr lpReserved,
			[Out] out int lpType,
			[In, Out] IntPtr lpData,
			[In, Out] ref int lpcbData);

		[PreserveSig]
		int CeRegSetValueEx(
			[In] uint hKey,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpValueName,
			[In] int Reserved,
			[In] int dwType,
			[In] IntPtr lpData,
			[In] int cbData);

		[PreserveSig]
		int CeGetStoreInformation( 
			ref StoreInfo lpsi);

		[PreserveSig]
		int CeGetSystemMetrics( 
			[In] int nIndex);
		
		[PreserveSig]
		int CeGetDesktopDeviceCaps( 
			[In] int nIndex);

		[PreserveSig]
		int CeFindAllDatabases( 
			[In] int DbaseType,
			[In] ushort Flags,
			[In, Out] ref ushort cFindData,
			[Out] out IntPtr ppFindData);

		void CeGetSystemInfo(ref SystemInformation sysInfo);

		[PreserveSig]
		int CeSHCreateShortcut( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszShortcut,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszTarget);

		[PreserveSig]
		int CeSHGetShortcutTarget( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszShortcut,
			[In, Out] System.Text.StringBuilder lpszTarget,
			[In] int cbMax);
		
		[PreserveSig]
		int CeCheckPassword( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszPassword);

		[PreserveSig]
		int CeGetFileTime( 
			[In] IntPtr hFile,
			[In, Out] ref System.Runtime.InteropServices.ComTypes.FILETIME lpCreationTime,
			[In, Out] ref System.Runtime.InteropServices.ComTypes.FILETIME lpLastAccessTime,
			[In, Out] ref System.Runtime.InteropServices.ComTypes.FILETIME lpLastWriteTime);

		[PreserveSig]
		int CeSetFileTime( 
			[In] IntPtr hFile,
			[In] CFILETIME lpCreationTime,
			[In] CFILETIME lpLastAccessTime,
			[In] CFILETIME lpLastWriteTime);

		[PreserveSig]
		int CeGetVersionEx( 
			ref CEOSVERSIONINFO lpVersionInformation);

		[PreserveSig]
		IntPtr CeGetWindow( 
			[In] IntPtr hWnd,
			[In] uint uCmd);

		[PreserveSig]
		int CeGetWindowLong( 
			[In] IntPtr hWnd,
			[In] int nIndex);

		[PreserveSig]
		int CeGetWindowText( 
			[In] IntPtr hWnd,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpString,
			[In] int nMaxCount);

		[PreserveSig]
		int CeGetClassName( 
			[In] IntPtr hWnd,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
			[In] int nMaxCount);
		
		void CeGlobalMemoryStatus( 
			ref MemoryStatus lpmst);
		
		[PreserveSig]
		int CeGetSystemPowerStatusEx( 
			ref PowerStatus pstatus,
			[In] int fUpdate);
		
		[PreserveSig]
		uint CeGetTempPath( 
			[In] int nBufferLength,
			[In, Out] System.Text.StringBuilder lpBuffer);
		
		[PreserveSig]
		uint CeGetSpecialFolderPath( 
			[In] int nFolder,
			[In] int nBufferLength,
			[In, Out] System.Text.StringBuilder lpBuffer);
		
		void CeRapiInvoke( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string pDllPath,
			[In, MarshalAs(UnmanagedType.LPWStr)] string pFunctionName,
			[In] int cbInput,
			[In] IntPtr pInput,
			[Out] out int pcbOutput,
			[Out] out IntPtr ppOutput,
			[In, Out, MarshalAs(UnmanagedType.IUnknown)] ref IntPtr ppIRAPIStream,
			[In] int dwReserved);
		
		[PreserveSig]
		IntPtr CeFindFirstDatabaseEx( 
			ref Guid pguid,
			[In] int dwDbaseType);

		[PreserveSig]
		uint CeFindNextDatabaseEx( 
			[In] IntPtr hEnum,
			ref Guid pguid);

		[PreserveSig]
		uint CeCreateDatabaseEx(
			ref Guid pceguid,
			ref IntPtr /*CEDBASEINFO*/ lpCEDBInfo);

		[PreserveSig]
		int CeSetDatabaseInfoEx(
			ref Guid pceguid,
			[In] uint oidDbase,
			ref IntPtr /*CEDBASEINFO*/ pNewInfo);

		[PreserveSig]
		IntPtr CeOpenDatabaseEx(
			[In] ref Guid pceguid,
			ref uint poid,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszName,
			int propid,
			[In] int dwFlags,
			[In] IntPtr pReq);

		[PreserveSig]
		int CeDeleteDatabaseEx(
			[In] ref Guid pceguid,
			[In] uint oidDbase);

		[PreserveSig]
		uint CeReadRecordPropsEx( 
			[In] IntPtr hDbase,
			[In] uint dwFlags,
			[In, Out] ref ushort lpcPropID,
			[In] ref uint[] rgPropID,
			[In, Out] ref IntPtr lplpBuffer,
			[In, Out] ref int lpcbBuffer,
			[In] IntPtr hHeap);

		[PreserveSig]
		int CeMountDBVol(
			ref Guid pceguid,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszDBVol,
			[In] int dwFlags);

		[PreserveSig]
		int CeUnmountDBVol(
			ref Guid pceguid);

		[PreserveSig]
		int CeFlushDBVol(
			ref Guid pceguid);

		[PreserveSig]
		int CeEnumDBVolumes(
			ref Guid pceguid,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpBuf,
			[In] int dwNumChars);

		[PreserveSig]
		int CeOidGetInfoEx(
			ref Guid pceguid,
			[In] uint oid,
			ref IntPtr /*CEOIDINFO*/ oidInfo);
		
		void CeSyncStart( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string szCommand);
		
		void CeSyncStop();

		[PreserveSig]
		int CeQueryInstructionSet( 
			[In] int dwInstructionSet,
			[In, Out] ref int lpdwCurrentInstructionSet);
		
		[PreserveSig]
		int CeGetDiskFreeSpaceEx( 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpDirectoryName,
			ref ulong lpFreeBytesAvailableToCaller,
			ref ulong lpTotalNumberOfBytes,
			ref ulong lpTotalNumberOfFreeBytes);
		
	};

	[Guid("8a0f1632-3905-4ca4-aea4-7e094ecbb9a7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IRAPIDevice
	{
		DeviceStatus GetConnectStat();
		
		void GetDeviceInfo( 
			ref RAPI_DEVICEINFO pDevInfo);
		
		void GetConnectionInfo( 
			ref RAPI_CONNECTIONINFO pConnInfo);
		
		IRAPISession CreateSession();
	};

	[Guid("357a557c-b03f-4240-90d8-c6c71c659bf1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IRAPIEnumDevices
	{
		IRAPIDevice Next();
		
		void Reset();
		
		void Skip(uint cElt);
		
		IRAPIEnumDevices Clone();
		
		int GetCount();
	}

	[Guid("b4fd053e-4810-46db-889b-20e638e334f0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IRAPISink
	{
		[PreserveSig]
		void OnDeviceConnected( 
			[In, MarshalAs(UnmanagedType.Interface)] IRAPIDevice pIDevice);

		[PreserveSig]
		void OnDeviceDisconnected(
			[In, MarshalAs(UnmanagedType.Interface)] IRAPIDevice pIDevice);
	};

	/// <summary>
	/// Provides arguments for a device connect/disconnect event.
	/// </summary>
	[Serializable]
	internal class DeviceConnectEventArgs : EventArgs
	{
		/// <summary>
		/// Device involved in the connect or disconnect event.
		/// </summary>
		public IRAPIDevice Device { get; private set; }

		/// <summary>
		/// Constructs a new instance of the <see cref="DeviceConnectEventArgs" /> class.
		/// </summary>
		public DeviceConnectEventArgs(IRAPIDevice device)
		{
			this.Device = device;
		}
	}

	internal class RAPISink : IRAPISink
	{
		/// <summary>Occurs when device connected.</summary>
		public event EventHandler<DeviceConnectEventArgs> DeviceConnected;

		/// <summary>Occurs when device disconnected.</summary>
		public event EventHandler<DeviceConnectEventArgs> DeviceDisconnected;

		/// <summary>Raises the <see cref="E:RAPISink.DeviceConnected"/> event.</summary>
		public void OnDeviceConnected(IRAPIDevice pIDevice)
		{
			EventHandler<DeviceConnectEventArgs> temp = DeviceConnected;
			if (temp != null)
				temp(this, new DeviceConnectEventArgs(pIDevice));
		}

		/// <summary>Raises the <see cref="E:RAPISink.DeviceDisconnected"/> event.</summary>
		public void OnDeviceDisconnected(IRAPIDevice pIDevice)
		{
			EventHandler<DeviceConnectEventArgs> temp = DeviceDisconnected;
			if (temp != null)
				temp(this, new DeviceConnectEventArgs(pIDevice));
		}
	}

	[Guid("dcbeb807-14d0-4cbd-926c-b991f4fd1b91"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IRAPIDesktop
	{
		void FindDevice( 
			ref Guid pDeviceID,
			RAPI_GETDEVICEOPCODE opFlags,
			[Out, MarshalAs(UnmanagedType.Interface)] out IRAPIDevice ppIDevice);

		IRAPIEnumDevices EnumDevices();
		
		void Advise( 
			[In, MarshalAs(UnmanagedType.Interface)] IRAPISink pISink,
			out int pdwContext);
		
		void UnAdvise( 
			int dwContext);
	};
}