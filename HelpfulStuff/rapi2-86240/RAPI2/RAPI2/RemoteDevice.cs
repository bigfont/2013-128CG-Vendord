using System.Devices.Interop;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Devices
{
	#region Structures

	/// <summary>
	/// Contains information about current memory availability for a remote device.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct MemoryStatus
	{
		internal uint dwLength;
		/// <summary>
		/// Specifies a number between zero and 100 that gives a general idea of current memory use, in which zero indicates no memory use and 100 indicates full memory use.
		/// </summary>
		public uint MemoryLoad;
		/// <summary>
		/// Indicates the total number of bytes of physical memory.
		/// </summary>
		public uint TotalPhysical;
		/// <summary>
		/// Indicates the number of bytes of physical memory available.
		/// </summary>
		public uint AvailPhysical;
		/// <summary>
		/// Indicates the total number of bytes that can be stored in the paging file. This number does not represent the physical size of the paging file on disk.
		/// </summary>
		public uint TotalPageFile;
		/// <summary>
		/// Indicates the number of bytes available in the paging file.
		/// </summary>
		public uint AvailablePageFile;
		/// <summary>
		/// Indicates the total number of bytes that can be described in the user mode portion of the virtual address space of the calling process.
		/// </summary>
		public uint TotalVirtual;
		/// <summary>
		/// Indicates the number of bytes of unreserved and uncommitted memory in the user mode portion of the virtual address space of the calling process.
		/// </summary>
		public uint AvailableVirtual;
	}

	/*public struct CeFileInfo
	{
		internal CeFileInfo(byte[] buffer)
		{
			CEFILEINFO fi = (CEFILEINFO)RemoteDevice.MarshalArrayToStruct(buffer, typeof(CEFILEINFO));
			Attributes = (System.IO.FileAttributes)fi.dwAttributes;
			ParentId = (int)fi.oidParent;
			FileName = fi.szFileName;
			LastChangedTime = fi.ftLastChanged.ToDateTime();
			Length = fi.dwLength;
		}

		public System.IO.FileAttributes Attributes;
		public int ParentId;
		public string FileName;
		public DateTime LastChangedTime;
		public uint Length;
	}*/

	/// <summary>
	/// Describes the current status of the Object Store
	/// </summary>
	public struct StoreInfo
	{
		/// <summary>
		/// Size of the Object Store in Bytes
		/// </summary>
		public int StoreSize;
		/// <summary>
		/// Free space in the Object Store in Bytes
		/// </summary>
		public int FreeSize;
	}

	#pragma warning disable 0649

	/// <summary>
	/// Structure for power information of mobile device
	/// </summary>
	public struct PowerStatus
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
	/// Data structure for GetSystemInfo
	/// </summary>
	public struct SystemInformation
	{
		/// <summary>
		/// Processor architecture
		/// </summary>
		public ProcessorArchitecture ProcessorArchitecture;
		internal ushort wReserved;
		/// <summary>
		/// Specifies the page size and the granularity of page protection and commitment.
		/// </summary>
		public uint PageSize;
		/// <summary>
		/// Pointer to the lowest memory address accessible to applications and dynamic-link libraries (DLLs). 
		/// </summary>
		public uint MinimumApplicationAddress;
		/// <summary>
		/// Pointer to the highest memory address accessible to applications and DLLs.
		/// </summary>
		public uint MaximumApplicationAddress;
		/// <summary>
		/// Specifies a mask representing the set of processors configured into the system. Bit 0 is processor 0; bit 31 is processor 31. 
		/// </summary>
		public uint ActiveProcessorMask;
		/// <summary>
		/// Specifies the number of processors in the system.
		/// </summary>
		public uint NumberOfProcessors;
		/// <summary>
		/// Specifies the type of processor in the system.
		/// </summary>
		public ProcessorType dwProcessorType;
		/// <summary>
		/// Specifies the granularity with which virtual memory is allocated.
		/// </summary>
		public uint AllocationGranularity;
		/// <summary>
		/// Specifies the system’s architecture-dependent processor level.
		/// </summary>
		public ushort ProcessorLevel;
		/// <summary>
		/// Specifies an architecture-dependent processor revision.
		/// </summary>
		public ushort ProcessorRevision;
	}

	#pragma warning restore 0649
	#endregion

	#region Enums

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
	/// Connection status for a device.
	/// </summary>
	public enum DeviceStatus
	{
		/// <summary>Not connected.</summary>
		Disconnected = 0,
		/// <summary>Connected.</summary>
		Connected = 1,
	}

	/// <summary>
	/// Mechanism used to connect to device.
	/// </summary>
	public enum ConnectionType
	{
		/// <summary>A USB connection.</summary>
		USB = 0,
		/// <summary>An infrared connection.</summary>
		IR = 1,
		/// <summary>A serial connection.</summary>
		Serial = 2,
		/// <summary>A network connection.</summary>
		Network = 3,
	}

	/// <summary>
	/// Flags that control the priority and the creation of the process. 
	/// </summary>
	[Flags]
	public enum ProcessCreationFlags
	{
		/// <summary>No conditions are set on the created process.</summary>
		None = 0,
		/// <summary>For Windows CE versions 2.0 and later. Calling process is treated as a debugger, and the new process is a process being debugged. Child processes of the new process are also debugged. The system notifies the debugger of all debug events that occur in the process being debugged.</summary>
		DebugProcess = 0x00000001,
		/// <summary>For Windows CE versions 2.0 and later. Calling process is treated as a debugger, and the new process is a process being debugged. No child processes of the new process are debugged. The system notifies the debugger of all debug events that occur in the process being debugged.</summary>
		DebugOnlyThisProcess = 0x00000002,
		/// <summary>The primary thread of the new process is created in a suspended state.</summary>
		CreateSuspended = 0x00000004,
		/// <summary>For Windows CE versions 3.0 and later. The new process has a new console, instead of inheriting the parent's console.</summary>
		CreateNewConsole = 0x00000010
	}

	/// <summary>
	/// Special folder defined on the device.
	/// </summary>
	public enum SpecialFolder
	{
		/// <summary>File system directory that serves as a common repository for application-specific data.</summary>
		ApplicationData = 0x001a,
		//RecylceBinFolder = 0x000a,
		/// <summary>File system directory used to physically store file objects on the desktop.</summary>
		Desktop = 0x0000,
		//DesktopDirectory = 0x0010,
		//MyComputer = 0x0011,
		/// <summary>File system directory that serves as a common repository for the user's favorite items.</summary>
		Favorites = 0x0006,
		/// <summary>Virtual folder containing fonts.</summary>
		Fonts = 0x0014,
		//MyMusic = 0x000d,
		/// <summary>The file system directory that serves as a common repository for image files. </summary>
		MyPictures = 0x0027,
		//MyVideo = 0x000e,
		//NetworkFolder = 0x0012,
		/// <summary>The file system directory used to physically store a user's common repository of documents.</summary>
		MyDocuments = 0x0005,
		/// <summary>Program files folder.</summary>
		ProgramFiles = 0x0026,
		//Programs = 0x0002,
		//Recent = 0x0008,
		/// <summary>File system directory that contains Start menu items.</summary>
		StartMenu = 0x000b,
		/// <summary>File system directory that corrsponds to the user's Startup program group. The system starts these programs when a device is powered on.</summary>
		Startup = 0x0007,
		/// <summary>Windows folder.</summary>
		Windows = 0x0024,
	}

	/// <summary>
	/// Items available in a call to RemoteDevice.GetDeviceCaps
	/// </summary>
	public enum DeviceCapsItem
	{
		/// <summary>Device driver version</summary>
		DRIVERVERSION = 0,
		/// <summary>Device classification</summary>
		TECHNOLOGY = 2,
		/// <summary>Horizontal size in millimeters</summary>
		HORZSIZE = 4,
		/// <summary>Vertical size in millimeters</summary>
		VERTSIZE = 6,
		/// <summary>Horizontal width in pixels</summary>
		HORZRES = 8,
		/// <summary>Vertical height in pixels</summary>
		VERTRES = 10,
		/// <summary>Number of bits per pixel</summary>
		BITSPIXEL = 12,
		/// <summary>Number of planes</summary>
		PLANES = 14,
		/// <summary>Number of brushes the device has</summary>
		NUMBRUSHES = 16,
		/// <summary>Number of pens the device has</summary>
		NUMPENS = 18,
		/// <summary>Number of markers the device has</summary>
		NUMMARKERS = 20,
		/// <summary>Number of fonts the device has</summary>
		NUMFONTS = 22,
		/// <summary>Number of colors the device supports</summary>
		NUMCOLORS = 24,
		/// <summary>Size required for device descriptor</summary>
		PDEVICESIZE = 26,
		/// <summary>Curve capabilities</summary>
		CURVECAPS = 28,
		/// <summary>Line capabilities</summary>
		LINECAPS = 30,
		/// <summary>Polygonal capabilities</summary>
		POLYGONALCAPS = 32,
		/// <summary>Text capabilities</summary>
		TEXTCAPS = 34,
		/// <summary>Clipping capabilities</summary>
		CLIPCAPS = 36,
		/// <summary>Bitblt capabilities</summary>
		RASTERCAPS = 38,
		/// <summary>Length of the X leg</summary>
		ASPECTX = 40,
		/// <summary>Length of the Y leg</summary>
		ASPECTY = 42,
		/// <summary>Length of the hypotenuse</summary>
		ASPECTXY = 44,
		/// <summary>Physical Width in device units</summary>
		PHYSICALWIDTH = 110,
		/// <summary>Physical Height in device units</summary>
		PHYSICALHEIGHT = 111,
		/// <summary>Physical Printable Area x margin</summary>
		PHYSICALOFFSETX = 112,
		/// <summary>Physical Printable Area y margin</summary>
		PHYSICALOFFSETY = 113,
		/// <summary>Shading and blending caps</summary>
		SHADEBLENDCAPS = 120,
	}

	/// <summary>
	/// Items available in a call to RemoteDevice.GetSystemMetrics
	/// </summary>
	public enum SystemMetricsItem
	{
		/// <summary></summary>
		SM_CXSCREEN = 0,
		/// <summary></summary>
		SM_CYSCREEN = 1,
		/// <summary></summary>
		SM_CXVSCROLL = 2,
		/// <summary></summary>
		SM_CYHSCROLL = 3,
		/// <summary></summary>
		SM_CYCAPTION = 4,
		/// <summary></summary>
		SM_CXBORDER = 5,
		/// <summary></summary>
		SM_CYBORDER = 6,
		/// <summary></summary>
		SM_CXDLGFRAME = 7,
		/// <summary></summary>
		SM_CYDLGFRAME = 8,
		/// <summary></summary>
		SM_CXICON = 11,
		/// <summary></summary>
		SM_CYICON = 12,
		/// <summary></summary>
		SM_CYMENU = 15,
		/// <summary></summary>
		SM_CXFULLSCREEN = 16,
		/// <summary></summary>
		SM_CYFULLSCREEN = 17,
		/// <summary></summary>
		SM_MOUSEPRESENT = 19,
		/// <summary></summary>
		SM_CYVSCROLL = 20,
		/// <summary></summary>
		SM_CXHSCROLL = 21,
		/// <summary></summary>
		SM_DEBUG = 22,
		/// <summary></summary>
		SM_CXDOUBLECLK = 36,
		/// <summary></summary>
		SM_CYDOUBLECLK = 37,
		/// <summary></summary>
		SM_CXICONSPACING = 38,
		/// <summary></summary>
		SM_CYICONSPACING = 39,
		/// <summary></summary>
		SM_CXEDGE = 45,
		/// <summary></summary>
		SM_CYEDGE = 46,
		/// <summary></summary>
		SM_CXSMICON = 49,
		/// <summary></summary>
		SM_CYSMICON = 50,
		/// <summary></summary>
		SM_XVIRTUALSCREEN = 76,
		/// <summary></summary>
		SM_YVIRTUALSCREEN = 77,
		/// <summary></summary>
		SM_CXVIRTUALSCREEN = 78,
		/// <summary></summary>
		SM_CYVIRTUALSCREEN = 79,
		/// <summary></summary>
		SM_CMONITORS = 80,
		/// <summary></summary>
		SM_SAMEDISPLAYFORMAT = 81,
	}
	#endregion

	#region Support Classes
	
	/// <summary>
	/// Methods to extend date classes.
	/// </summary>
	public static class ExetensionMethods
	{
		/// <summary>
		/// Converts a FILETIME to a DateTime
		/// </summary>
		/// <param name="ft">FILETIME to convert.</param>
		/// <returns>Equivalent DateTime.</returns>
		public static DateTime ToDateTime(this System.Runtime.InteropServices.ComTypes.FILETIME ft)
		{
			DateTime dt = DateTime.MaxValue;
			long hFT2 = (((long)ft.dwHighDateTime) << 32) + ft.dwLowDateTime;

			try
			{
				dt = DateTime.FromFileTimeUtc(hFT2);
			}
			catch (ArgumentOutOfRangeException)
			{
				dt = DateTime.MaxValue;
			}

			return dt;
		}

		/// <summary>
		/// Converts a DateTime to a FILETIME
		/// </summary>
		/// <param name="dt">DateTime to convert.</param>
		/// <returns>Equivalent FILETIME.</returns>
		public static System.Runtime.InteropServices.ComTypes.FILETIME ToFILETIME(this DateTime dt)
		{
			System.Runtime.InteropServices.ComTypes.FILETIME ft = new System.Runtime.InteropServices.ComTypes.FILETIME();
			long hFT1 = dt.ToFileTimeUtc();
			ft.dwLowDateTime = (int)(hFT1 & 0xFFFFFFFF);
			ft.dwHighDateTime = (int)(hFT1 >> 32);
			return ft;
		}
	}

	/// <summary>
	/// An exception thrown by the RAPI2 set of interfaces.
	/// </summary>
	public class RapiException : System.ComponentModel.Win32Exception
	{
		internal RapiException(int lastError)
			: base(lastError)
		{
		}
	}

	#endregion
	
	/// <summary>
	/// Represents a remote device. This can only be accessed through <see cref="RemoteDeviceManager.Devices"/>.
	/// </summary>
	public class RemoteDevice : IDisposable
	{
		private const int MAX_PATH = 260;

		private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

		private RAPI_DEVICEINFO cache = new RAPI_DEVICEINFO();
		private IRAPIDevice iDevice;
		private IRAPISession iSession;
		private bool sessionFailure = false;

		internal RemoteDevice(IRAPIDevice iDev)
		{
			this.IDevice = iDev;
		}

		internal RemoteDevice(ref RAPI_DEVICEINFO di)
		{
			cache = di;
		}

		/// <summary>
		/// Gets the means by which the device is connected.
		/// </summary>
		public ConnectionType ConnectionType
		{
			get { return ConnInfo.connType; }
		}

		/// <summary>
		/// Gets an enumerated list of all the databases on the device.
		/// </summary>
		/// <value>The databases.</value>
		public RemoteDatabaseList Databases
		{
			get { return new RemoteDatabaseList(this.ISession, 0); }
		}

		/// <summary>
		/// Gets the unique identifier for the device.
		/// </summary>
		public Guid DeviceId
		{
			get { return DevInfo.DeviceId; }
		}

		/// <summary>Reads the remote device's registry base key HKEY_CLASSES_ROOT.</summary>
		public DeviceRegistryKey DeviceRegistryClassesRoot
		{
			get { return new DeviceRegistryKey(ISession, 0x80000000, "HKEY_CLASSES_ROOT"); }
		}

		/// <summary>Reads the remote device's registry base key HKEY_CURRENT_USER.</summary>
		public DeviceRegistryKey DeviceRegistryCurrentUser
		{
			get { return new DeviceRegistryKey(ISession, 0x80000001, "HKEY_CURRENT_USER"); }
		}

		/// <summary>Reads the remote device's registry base key HKEY_LOCAL_MACHINE.</summary>
		public DeviceRegistryKey DeviceRegistryLocalMachine
		{
			get { return new DeviceRegistryKey(ISession, 0x80000002, "HKEY_LOCAL_MACHINE"); }
		}

		/// <summary>Reads the remote device's registry base key HKEY_USERS.</summary>
		public DeviceRegistryKey DeviceRegistryUsers
		{
			get { return new DeviceRegistryKey(ISession, 0x80000003, "HKEY_USERS"); }
		}

		/// <summary>
		/// Gets the host address of the connected desktop.
		/// </summary>
		public System.Net.IPEndPoint HostAddress
		{
			get { return ConnInfo.host; }
		}

		/// <summary>
		/// Gets the assigned address for the device.
		/// </summary>
		public System.Net.IPEndPoint IPAddress
		{
			get { return ConnInfo.addr; }
		}

		/// <summary>
		/// Gets the last error raised by the device.
		/// </summary>
		public int LastError
		{
			get { return ISession.CeGetLastError(); }
		}

		/// <summary>
		/// Gets the <see cref="MemoryStatus"/> for the device.
		/// </summary>
		public MemoryStatus MemoryStatus
		{
			get
			{
				MemoryStatus stat = new MemoryStatus();
				ISession.CeGlobalMemoryStatus(ref stat);
				return stat;
			}
		}

		/// <summary>
		/// Gets the name of the device.
		/// </summary>
		public string Name
		{
			get { return DevInfo.bstrName; }
		}

		/// <summary>
		/// Gets the version of the device OS.
		/// </summary>
		public Version OSVersion
		{
			get
			{
				CEOSVERSIONINFO ver = new CEOSVERSIONINFO();
				ISession.CeGetVersionEx(ref ver);
				return new Version(ver.dwMajorVersion, ver.dwMinorVersion,
					ver.dwBuildNumber, 0);
			}
		}

		/// <summary>
		/// Gets a string representation of the device platform.
		/// </summary>
		public string Platform
		{
			get { return DevInfo.bstrPlatform; }
		}

		/// <summary>
		/// Gets the <see cref="PowerStatus"/> for the device.
		/// </summary>
		public PowerStatus PowerStatus
		{
			get
			{
				PowerStatus stat = new PowerStatus();
				ISession.CeGetSystemPowerStatusEx(ref stat, 1);
				return stat;
			}
		}

		/// <summary>
		/// Gets the connection status of the device.
		/// </summary>
		public DeviceStatus Status
		{
			get { return (IDevice == null) ? DeviceStatus.Disconnected : IDevice.GetConnectStat(); }
		}

		/// <summary>
		/// Gets the <see cref="StoreInfo"/> with information about the object store on the device.
		/// </summary>
		public StoreInfo StoreInfo
		{
			get
			{
				StoreInfo stat = new StoreInfo();
				ISession.CeGetStoreInformation(ref stat);
				return stat;
			}
		}

		/// <summary>
		/// Gets the <see cref="SystemInformation"/> for the device.
		/// </summary>
		public SystemInformation SystemInformation
		{
			get
			{
				SystemInformation si = new SystemInformation();
				ISession.CeGetSystemInfo(ref si);
				return si;
			}
		}

		internal IRAPIDevice IDevice
		{
			get { return iDevice; }
			set
			{
				ResetSession();
				iDevice = value;
			}
		}

		internal IRAPISession ISession
		{
			get
			{
				if (sessionFailure)
					throw new InvalidOperationException(Properties.Resources.ErrorDisconnectedDevice);

				if (iSession == null)
				{
					try { iSession = IDevice.CreateSession(); }
					catch (Exception ex) { System.Diagnostics.Debug.WriteLine("ISession: " + ex.ToString()); }

					if (iSession == null)
					{
						sessionFailure = true;
						throw new InvalidOperationException(Properties.Resources.ErrorDisconnectedDevice);
					}

					iSession.CeRapiInit();
				}

				return iSession;
			}
		}

		private RAPIConnectionInfo ConnInfo
		{
			get
			{
				RAPI_CONNECTIONINFO ci = new RAPI_CONNECTIONINFO();
				IDevice.GetConnectionInfo(ref ci);
				return new RAPIConnectionInfo(ref ci);
			}
		}

		private RAPI_DEVICEINFO DevInfo
		{
			get { return iDevice == null ? cache : GetDeviceInfo(IDevice); }
		}

		/// <summary>
		/// This method compares a specified string to the system password on a remote device.
		/// </summary>
		/// <param name="pwd">Password to compare with the system password.</param>
		/// <returns>true if password matches. Otherwise false.</returns>
		public bool CheckPassword(string pwd)
		{
			return Convert.ToBoolean(ISession.CeCheckPassword(pwd));
		}

		/// <summary>
		/// Runs a program on a remote device. It creates a new process and its primary thread. The new process executes the specified executable file.
		/// </summary>
		/// <param name="applicationName">String that specifies the module to execute. <para>The string can specify the full path and file name of the module to execute or it can specify just the module name. In the case of a partial name, the function uses the current drive and current directory to complete the specification.</para></param>
		/// <param name="commandLine">String that specifies the command line arguments with which the application will be executed. 
		/// <para>The commandLine parameter can be NULL. In that case, the method uses the string pointed to by applicationName as the command line.</para>
		/// <para>If commandLine is non-NULL, applicationName specifies the module to execute, and commandLine specifies the command line arguments.</para></param>
		/// <param name="creationFlags">Optional conditions for creating the process.</param>
		public void CreateProcess(string applicationName, string commandLine, ProcessCreationFlags creationFlags)
		{
			PROCESS_INFORMATION pi;
			if (0 == ISession.CeCreateProcess(applicationName, commandLine, 0, 0, 0, (int)creationFlags, 0, 0, 0, out pi))
				ThrowRAPIException();
		}

		/// <summary>
		/// Creates a shortcut file on the device in the specified location.
		/// </summary>
		/// <param name="shortcutFileName">Name of the shortcut file.</param>
		/// <param name="targetFileName">Name of the target file.</param>
		public void CreateShortcut(string shortcutFileName, string targetFileName)
		{
			if (0 == ISession.CeSHCreateShortcut(shortcutFileName, targetFileName))
				ThrowRAPIException();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.IDevice = null;
			ResetSession();
		}

		/// <summary>
		/// Retrieves device-specific information about a remote device.
		/// </summary>
		/// <param name="index">Item to retrieve information on.</param>
		/// <returns>The return value of the specified item.</returns>
		public int GetDeviceCaps(DeviceCapsItem index)
		{
			return ISession.CeGetDesktopDeviceCaps((int)index);
		}

		/// <summary>
		/// Retrieves the amount of space on a disk volume on a remote device.
		/// </summary>
		/// <param name="drivePath">String that specifies a directory on a disk.</param>
		/// <returns><see cref="DriveInfo"/> structure with information about specified disk.</returns>
		public DriveInfo GetDriveInfo(string drivePath)
		{
			DriveInfo di = new DriveInfo();
			if (0 == ISession.CeGetDiskFreeSpaceEx(drivePath, ref di.AvailableFreeSpace, ref di.TotalSize, ref di.TotalFreeSpace))
				ThrowRAPIException();
			return di;
		}

		/// <summary>
		/// Retrieves the path to a special shell folder on a remote device.
		/// </summary>
		/// <param name="folder">SpecialFolder enumeration.</param>
		/// <returns>Path of special folder on device.</returns>
		public string GetFolderPath(SpecialFolder folder)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(MAX_PATH);
			if (0 == ISession.CeGetSpecialFolderPath((int)folder, MAX_PATH, sb))
				ThrowRAPIException();
			return sb.ToString();
		}

		/// <summary>
		/// Gets the shortcut target.
		/// </summary>
		/// <param name="shortcutFileName">Name of the shortcut file.</param>
		/// <returns>A <see cref="String"/> containing the path of the shortcut target.</returns>
		public string GetShortcutTarget(string shortcutFileName)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(MAX_PATH);
			if (0 == ISession.CeSHGetShortcutTarget(shortcutFileName, sb, MAX_PATH))
				ThrowRAPIException();
			return sb.ToString();
		}

		/// <summary>
		/// This method retrieves the dimensions of display elements and system configuration settings of a remote device. All dimensions are in pixels.
		/// </summary>
		/// <param name="index">Item to retrieve information on.</param>
		/// <returns>The return value of the specified item.</returns>
		public int GetSystemMetrics(SystemMetricsItem index)
		{
			return ISession.CeGetSystemMetrics((int)index);
		}

		/// <summary>
		/// Gets the path to the directory designated for temporary files on a remote device.
		/// </summary>
		/// <returns>Temporary path.</returns>
		public string GetTempPath()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(MAX_PATH);
			if (0 == ISession.CeGetTempPath(MAX_PATH, sb))
				ThrowRAPIException();
			return sb.ToString();
		}

		/// <summary>
		/// Starts the synchronization process with the device.
		/// </summary>
		public void StartSync()
		{
			ISession.CeSyncStart(null);
		}

		/// <summary>
		/// Stops the synchronization process with the device.
		/// </summary>
		public void StopSync()
		{
			ISession.CeSyncStop();
		}

		internal static RAPI_DEVICEINFO GetDeviceInfo(IRAPIDevice iDevice)
		{
			RAPI_DEVICEINFO di = new RAPI_DEVICEINFO();
			if (iDevice != null)
				iDevice.GetDeviceInfo(ref di);
			return di;
		}

		/*private const ushort OBJTYPE_FILE = 1;
		private const ushort OBJTYPE_DIRECTORY = 2;
		private const ushort OBJTYPE_DATABASE = 3;
		private const ushort OBJTYPE_RECORD = 4;

		internal CEFILEINFO GetFileInfo(int oid)
		{
			CEOIDINFO oidi = GetOidInfo(oid);
			if (oidi.wObjType == OBJTYPE_FILE)
				return (CEFILEINFO)MarshalArrayToStruct(GetOidInfo(oid).inf, typeof(CEFILEINFO));
			throw new ArgumentException();
		}

		internal CEDBASEINFO GetDatabaseInfo(int oid)
		{
			CEOIDINFO oidi = GetOidInfo(oid);
			if (oidi.wObjType == OBJTYPE_FILE)
				return (CEDBASEINFO)MarshalArrayToStruct(GetOidInfo(oid).inf, typeof(CEDBASEINFO));
			throw new ArgumentException();
		}*/

		internal static object MarshalArrayToStruct(byte[] bits, Type objType)
		{
			IntPtr ptr = Marshal.AllocHGlobal(bits.Length);
			Marshal.Copy(bits, 0, ptr, bits.Length);
			object ret = Marshal.PtrToStructure(ptr, objType);
			Marshal.FreeHGlobal(ptr);
			return ret;
		}

		internal void Reconfigure(ref RAPI_DEVICEINFO di)
		{
			this.IDevice = null;
			this.cache = di;
		}

		internal void ResetSession()
		{
			if (iSession != null)
				try { iSession.CeRapiUninit(); } catch { }
			iSession = null;
			sessionFailure = false;
		}

		internal void ThrowRAPIException()
		{
			throw new RapiException(ISession.CeGetLastError());
		}

		private CEOIDINFO GetOidInfo(uint oid)
		{
			CEOIDINFO info = new CEOIDINFO();
			int ret = ISession.CeOidGetInfo(oid, ref info);
			if (ret == 0)
				ThrowRAPIException();
			return info;
		}

		/// <summary>Information about a space on a disk.</summary>
		public struct DriveInfo
		{
			/// <summary>The total number of free bytes on a disk that are available to the user.</summary>
			public ulong AvailableFreeSpace;

			/// <summary>The total number of bytes on a disk that are available to the user.</summary>
			public ulong TotalFreeSpace;

			/// <summary>The total number of free bytes on a disk</summary>
			public ulong TotalSize;
		}

		/// <summary>
		/// Represents a key-level node in the remote device's registry. This class is a registry encapsulation.
		/// </summary>
		public sealed class DeviceRegistryKey : IDisposable
		{
			private const int ERROR_FILE_NOT_FOUND = 2;
			private const int ERROR_MORE_DATA = 234;
			private const int ERROR_NO_MORE_ITEMS = 259;
			private const int ERROR_SUCCESS = 0;

			private uint hKey = 0;
			private IRAPISession sess;

			internal DeviceRegistryKey(IRAPISession session)
			{
				sess = session;
			}

			internal DeviceRegistryKey(IRAPISession session, uint handle, string keyName)
			{
				sess = session;
				hKey = handle;
				Name = keyName;
			}

			/// <summary>
			/// Gets the name of the registry key.
			/// </summary>
			/// <value>The name of the registry key.</value>
			public string Name { get; private set; }

			/// <summary>
			/// Closes the key and flushes it to disk if its contents have been modified.
			/// </summary>
			public void Close()
			{
				this.Dispose();
			}

			/// <summary>
			/// Creates a new subkey or opens an existing subkey.
			/// </summary>
			/// <param name="subkey">Name of key to create.</param>
			/// <returns>A <see cref="DeviceRegistryKey"/> object that represents the newly created subkey, or <c>null</c> if the operation failed.</returns>
			public DeviceRegistryKey CreateSubKey(string subkey)
			{
				EnsureNotDisposed();
				uint hNewKey = 0, disp = 0;
				int ret = sess.CeRegCreateKeyEx(hKey, subkey, 0, string.Empty, 0, 0, IntPtr.Zero, ref hNewKey, ref disp);
				if (ret != ERROR_SUCCESS)
					throw new RapiException(ret);
				return new DeviceRegistryKey(sess, hNewKey, this.Name + "\\" + subkey);
			}

			/// <summary>
			/// Deletes the specified subkey.
			/// </summary>
			/// <param name="subkey">The name of the subkey to delete.</param>
			/// <remarks>This method will fail if named subkey has children.</remarks>
			public void DeleteSubKey(string subkey)
			{
				DeleteSubKey(subkey, false);
			}

			/// <summary>
			/// Deletes the specified subkey.
			/// </summary>
			/// <param name="subkey">The name of the subkey to delete.</param>
			/// <param name="recursive">if set to <c>true</c> delete all subkeys under this subkey.</param>
			/// <exception cref="System.Devices.RapiException"></exception>
			public void DeleteSubKey(string subkey, bool recursive)
			{
				EnsureNotDisposed();
				if (recursive)
				{
					using (DeviceRegistryKey subKey = OpenSubKey(subkey))
					{
						foreach (var childKey in subKey.GetSubKeyNames())
							subKey.DeleteSubKey(childKey, true);
					}
				}
				int ret = sess.CeRegDeleteKey(hKey, subkey);
				if (ret != ERROR_SUCCESS && ret != ERROR_FILE_NOT_FOUND)
					throw new RapiException(ret);
			}

			/// <summary>
			/// Deletes the specified value from this key.
			/// </summary>
			/// <param name="name">The name of the value to delete.</param>
			public void DeleteValue(string name)
			{
				EnsureNotDisposed();
				int ret = sess.CeRegDeleteValue(hKey, name);
				if (ret != ERROR_SUCCESS && ret != ERROR_FILE_NOT_FOUND)
					throw new RapiException(ret);
			}

			/// <summary>
			/// Performs a close on the current key.
			/// </summary>
			public void Dispose()
			{
				sess.CeRegCloseKey(hKey);
				hKey = 0;
				GC.SuppressFinalize(this);
			}

			/// <summary>
			/// Retrieves an array of strings that contains all the subkey names.
			/// </summary>
			/// <returns>An array of strings that contains the names of the subkeys for the current key.</returns>
			public string[] GetSubKeyNames()
			{
				uint idx = 0;
				StringBuilder sb = new StringBuilder(MAX_PATH);
				StringBuilder sbNull = new StringBuilder(MAX_PATH);
				uint cbName = MAX_PATH;
				uint cbClass = MAX_PATH;
				int ret = 0;
				System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
				do
				{
					cbName = (uint)sb.Capacity;
					ret = sess.CeRegEnumKeyEx(hKey, idx, sb, ref cbName, 0, sbNull, ref cbClass, IntPtr.Zero);
					if (ret == ERROR_MORE_DATA)
					{
						sb.Capacity = (int)cbName + 1;
						ret = sess.CeRegEnumKeyEx(hKey, idx, sb, ref cbName, 0, sbNull, ref cbClass, IntPtr.Zero);
					}
					if (ret == ERROR_SUCCESS)
						list.Add(sb.ToString());
					if (ret == ERROR_NO_MORE_ITEMS)
						break;
					idx++;
				} while (ret == ERROR_SUCCESS);

				if (ret != ERROR_NO_MORE_ITEMS)
					throw new RapiException(ret);

				return list.ToArray();
			}

			/// <summary>
			/// Retrieves the value associated with the specified name. If the name is not found, returns the default value that you provide.
			/// </summary>
			/// <param name="name">The name of the value to retrieve.</param>
			/// <param name="defaultValue">The value to return if name does not exist.</param>
			/// <returns>The value associated with name, with any embedded environment variables left unexpanded, or defaultValue if name is not found.</returns>
			public object GetValue(string name, object defaultValue)
			{
				EnsureNotDisposed();
				int lpType = 0;
				int cbData = 0;
				int ret = sess.CeRegQueryValueEx(hKey, name, IntPtr.Zero, out lpType, IntPtr.Zero, ref cbData);
				if (ret != ERROR_SUCCESS)
					throw new RapiException(ret);
				HGlobalSafeHandle data = new HGlobalSafeHandle(cbData);
				ret = sess.CeRegQueryValueEx(hKey, name, IntPtr.Zero, out lpType, data, ref cbData);
				if (ret != 0)
					throw new RapiException(ret);
				if (data == IntPtr.Zero)
					return defaultValue;
				byte[] buffer = new byte[cbData];
				Marshal.Copy(data, buffer, 0, cbData);
				switch (lpType)
				{
					case (int)Microsoft.Win32.RegistryValueKind.ExpandString:
					case (int)Microsoft.Win32.RegistryValueKind.String:
						return Encoding.Unicode.GetString(buffer);
					case (int)Microsoft.Win32.RegistryValueKind.DWord:
						return BitConverter.ToInt32(buffer, 0);
					case (int)Microsoft.Win32.RegistryValueKind.QWord:
						return BitConverter.ToInt64(buffer, 0);
					case (int)Microsoft.Win32.RegistryValueKind.Binary:
						return buffer;
					case (int)Microsoft.Win32.RegistryValueKind.MultiString:
						return Encoding.Unicode.GetString(buffer).TrimEnd('\0').Split('\0');
					default:
						return defaultValue;
				}
			}

			/// <summary>
			/// Gets the type of value in a registry value.
			/// </summary>
			/// <param name="name">The name of the value whose registry data type is to be retrieved.</param>
			/// <returns>A RegistryValueKind value representing the registry data type of the value associated with name.</returns>
			public Microsoft.Win32.RegistryValueKind GetValueKind(string name)
			{
				EnsureNotDisposed();
				int lpType = 0;
				int lpcbData = 0;
				int ret = sess.CeRegQueryValueEx(hKey, name, IntPtr.Zero, out lpType, IntPtr.Zero, ref lpcbData);
				if (ret != ERROR_SUCCESS)
					throw new RapiException(ret);
				if (!Enum.IsDefined(typeof(Microsoft.Win32.RegistryValueKind), lpType))
					return Microsoft.Win32.RegistryValueKind.Unknown;
				return (Microsoft.Win32.RegistryValueKind)lpType;
			}

			/// <summary>
			/// Retrieves an array of strings that contains all the value names associated with this key.
			/// </summary>
			/// <returns>An array of strings that contains the value names for the current key.</returns>
			public string[] GetValueNames()
			{
				StringBuilder sb = new StringBuilder(MAX_PATH);
				uint idx = 0, cbName = MAX_PATH, cbData = 0, lpType;
				int ret = 0;
				System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
				do
				{
					cbName = (uint)sb.Capacity;
					ret = sess.CeRegEnumValue(hKey, idx, sb, ref cbName, 0, out lpType, IntPtr.Zero, ref cbData);
					if (ret == ERROR_MORE_DATA)
					{
						sb.Capacity = (int)cbName + 1;
						ret = sess.CeRegEnumValue(hKey, idx, sb, ref cbName, 0, out lpType, IntPtr.Zero, ref cbData);
					}
					if (ret == ERROR_SUCCESS)
						list.Add(sb.ToString());
					if (ret == ERROR_NO_MORE_ITEMS)
						break;
					idx++;
				} while (ret == ERROR_SUCCESS);

				if (ret != ERROR_NO_MORE_ITEMS)
					throw new RapiException(ret);

				return list.ToArray();
			}

			/// <summary>
			/// Retrieves a subkey as read-only.
			/// </summary>
			/// <param name="name">The name or path of the subkey to open read-only.</param>
			/// <returns>The subkey requested, or null if the operation failed.</returns>
			public DeviceRegistryKey OpenSubKey(string name)
			{
				EnsureNotDisposed();
				uint hNewKey = 0;
				int ret = sess.CeRegOpenKeyEx(hKey, name, 0, 0, ref hNewKey);
				if (ret != ERROR_SUCCESS)
					throw new RapiException(ret);
				return new DeviceRegistryKey(sess, hNewKey, this.Name + "\\" + name);
			}

			/// <summary>
			/// Sets the specified name/value pair.
			/// </summary>
			/// <param name="name">The name of the value to store.</param>
			/// <param name="value">The data to be stored.</param>
			public void SetValue(string name, object value)
			{
				Microsoft.Win32.RegistryValueKind valueKind = Microsoft.Win32.RegistryValueKind.Unknown;
				if (value is int)
					valueKind = Microsoft.Win32.RegistryValueKind.DWord;
				else if (value is long)
					valueKind = Microsoft.Win32.RegistryValueKind.QWord;
				else if (value is string)
					valueKind = Microsoft.Win32.RegistryValueKind.String;
				else if (value is byte[])
					valueKind = Microsoft.Win32.RegistryValueKind.Binary;
				else if (value is string[])
					valueKind = Microsoft.Win32.RegistryValueKind.MultiString;
				if (valueKind == Microsoft.Win32.RegistryValueKind.Unknown)
					throw new ArgumentException("value must be of a known type");
				SetValue(name, value, valueKind);
			}

			/// <summary>
			/// Sets the value of a name/value pair in the registry key, using the specified registry data type.
			/// </summary>
			/// <param name="name">The name of the value to store.</param>
			/// <param name="value">The data to be stored.</param>
			/// <param name="valueKind">The registry data type to use when storing the data.</param>
			public void SetValue(string name, object value, Microsoft.Win32.RegistryValueKind valueKind)
			{
				EnsureNotDisposed();
				HGlobalSafeHandle data = null;
				int cbData = 0;
				switch (valueKind)
				{
					case Microsoft.Win32.RegistryValueKind.Binary:
						cbData = ((byte[])value).Length;
						data = new HGlobalSafeHandle((byte[])value);
						break;
					case Microsoft.Win32.RegistryValueKind.DWord:
						cbData = sizeof(int);
						data = new HGlobalSafeHandle(BitConverter.GetBytes(Convert.ToInt32(value)));
						break;
					case Microsoft.Win32.RegistryValueKind.String:
					case Microsoft.Win32.RegistryValueKind.ExpandString:
					case Microsoft.Win32.RegistryValueKind.MultiString:
						string str = (valueKind == Microsoft.Win32.RegistryValueKind.MultiString ? string.Join("\0", (string[])value) : value.ToString()) + '\0';
						byte[] bytes = Encoding.Unicode.GetBytes(str);
						cbData = bytes.Length;
						data = new HGlobalSafeHandle(bytes);
						break;
					case Microsoft.Win32.RegistryValueKind.QWord:
						cbData = sizeof(long);
						data = new HGlobalSafeHandle(BitConverter.GetBytes(Convert.ToInt64(value)));
						break;
					case Microsoft.Win32.RegistryValueKind.Unknown:
					default:
						throw new InvalidOperationException();
				}
				int ret = sess.CeRegSetValueEx(hKey, name, 0, (int)valueKind, data, cbData);
				if (ret != ERROR_SUCCESS)
					throw new RapiException(ret);
			}

			/// <summary>
			/// Retrieves a string representation of this key.
			/// </summary>
			/// <returns>Key name.</returns>
			public override string ToString()
			{
				return this.Name;
			}

			private void EnsureNotDisposed()
			{
				if (this.hKey == 0)
					throw new ObjectDisposedException("DeviceRegistryKey");
			}
		}

		/// <summary>
		/// Enumerates all connected devices. Access through <see cref="RemoteDeviceManager.Devices"/>.
		/// </summary>
		public class RemoteDatabaseList : System.Collections.Generic.IEnumerable<RemoteDatabase>, IDisposable
		{
			private uint dbType;
			private IRAPISession sess;

			internal RemoteDatabaseList(IRAPISession session, uint dbType)
			{
				sess = session;
				this.dbType = dbType;
			}

			/// <summary>
			/// Cleans up all internal references.
			/// </summary>
			public void Dispose()
			{
				sess = null;
				GC.SuppressFinalize(this);
			}

			/// <summary>
			/// Returns the strongly typed enumerator.
			/// </summary>
			/// <returns>Enumerator</returns>
			public System.Collections.Generic.IEnumerator<RemoteDatabase> GetEnumerator()
			{
				return new RemoteDatabaseEnum(sess, dbType);
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			/// <summary>
			/// Internal enumerator for databases on a remote device.
			/// </summary>
			public class RemoteDatabaseEnum : System.Collections.Generic.IEnumerator<RemoteDatabase>, IDisposable
			{
				private RemoteDatabase current = null;
				private uint dbType;
				private RemoteDevice.DeviceHandle handle;
				private IRAPISession sess;

				internal RemoteDatabaseEnum(IRAPISession session, uint dbType)
				{
					sess = session;
					this.dbType = dbType;
					this.Reset();
				}

				/// <summary>
				/// Gets the current item in the enumeration.
				/// </summary>
				public RemoteDatabase Current
				{
					get { if (current == null) throw new InvalidOperationException(); return current; }
				}

				object System.Collections.IEnumerator.Current
				{
					get { return this.Current; }
				}

				/// <summary>
				/// Frees all available resources.
				/// </summary>
				public void Dispose()
				{
					if (handle != null)
					{
						handle.Dispose();
						handle = null;
					}
				}

				/// <summary>
				/// Moves to the next database.
				/// </summary>
				/// <returns>true if a database was found. Otherwise, false.</returns>
				public bool MoveNext()
				{
					try
					{
						uint ret = sess.CeFindNextDatabase(this.handle);
						if (ret == 0)
							throw new RapiException(sess.CeGetLastError());
						current = new RemoteDatabase(sess, ret);
						return true;
					}
					catch (Exception)
					{
						current = null;
					}
					return false;
				}

				/// <summary>
				/// Resets the enumeration.
				/// </summary>
				public void Reset()
				{
					Dispose();
					handle = new DeviceHandle(sess, sess.CeFindFirstDatabase(dbType));
					if (handle.IsInvalid)
						throw new RapiException(sess.CeGetLastError());
					current = null;
				}
			}
		}

		internal class DeviceFile : SafeHandle
		{
			internal string Name;

			private IRAPISession sess;

			internal DeviceFile(IRAPISession sess, string fileName, uint dwDesiredAccess, uint dwShareMode, uint dwCreationDistribution, uint dwFlags)
				: base(InvalidHandleValue, true)
			{
				this.sess = sess;
				this.Name = fileName;
				base.SetHandle(sess.CeCreateFile(fileName, dwDesiredAccess, dwShareMode, IntPtr.Zero, dwCreationDistribution, dwFlags, IntPtr.Zero));
				if (this.IsInvalid)
					throw new RapiException(sess.CeGetLastError());
			}

			internal DeviceFile(IRAPISession sess, string fileName)
				: this(sess, fileName, 0, 1, 3, 0x80)
			{
			}

			public override bool IsInvalid
			{
				get { return base.handle == IntPtr.Zero || base.handle == InvalidHandleValue; }
			}

			public ulong Size
			{
				get
				{
					uint size = 0;
					uint res = sess.CeGetFileSize(base.handle, ref size);
					if (res == (uint)0xFFFFFFFF)
						throw new RapiException(sess.CeRapiGetError());
					return (ulong)res + ((ulong)size << 32);
				}
			}

			/// <summary>
			/// Allows to use DeviceFile as IntPtr
			/// </summary>
			public static implicit operator IntPtr(DeviceFile f)
			{
				return f.DangerousGetHandle();
			}

			public FileTimes GetFileTimes()
			{
				FileTimes ft = new FileTimes();
				if (0 == sess.CeGetFileTime(base.handle, ref ft.cft, ref ft.aft, ref ft.wft))
					throw new RapiException(sess.CeGetLastError());
				return ft;
			}

			public int Read(byte[] array, int offset, int count)
			{
				byte[] buf = new byte[count];
				int read = 0;
				int res = sess.CeReadFile(base.handle, buf, (uint)count, ref read, IntPtr.Zero);
				if (0 == res)
					throw new RapiException(sess.CeGetLastError());
				buf.CopyTo(array, offset);
				return read;
			}

			/*public void WriteStream(System.IO.Stream stream)
			{
				// read 4k of data
				int filepos = 0;
				byte[] buffer = new byte[0x1000]; // 4k transfer buffer
				int bytesread = stream.Read(buffer, filepos, buffer.Length);
				while (bytesread > 0)
				{
					// move remote file pointer # of bytes read
					filepos += bytesread;

					// write our buffer to the remote file
					this.Write(buffer, 0, bytesread);
					try
					{
						// refill the local buffer
						bytesread = stream.Read(buffer, 0, buffer.Length);
					}
					catch (Exception)
					{
						bytesread = 0;
					}
				}
			}*/

			public long Seek(long offset, System.IO.SeekOrigin origin)
			{
				int lowOffset = (int)offset;
				int highOffset = (int)(offset >> 32);
				uint res = sess.CeSetFilePointer(base.handle, lowOffset, ref highOffset, (uint)origin);
				if (0xFFFFFFFF == res)
					throw new RapiException(sess.CeGetLastError());
				return (long)res + ((long)highOffset << 32);
			}

			public void SetEndOfFile()
			{
				if (0 == sess.CeSetEndOfFile(base.handle))
					throw new RapiException(sess.CeGetLastError());
			}

			public void SetFileTimes(DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
			{
				CFILETIME cft = creationTime.HasValue ? new CFILETIME(creationTime.Value) : null;
				CFILETIME aft = lastAccessTime.HasValue ? new CFILETIME(lastAccessTime.Value) : null;
				CFILETIME wft = lastWriteTime.HasValue ? new CFILETIME(lastWriteTime.Value) : null;
				if (0 == sess.CeSetFileTime(base.handle, cft, aft, wft))
					throw new RapiException(sess.CeGetLastError());
			}

			public int Write(byte[] array, int offset, int count)
			{
				int ret = 0;
				byte[] buf;
				if (offset == 0)
					buf = array;
				else
				{
					buf = new byte[count];
					array.CopyTo(buf, offset);
				}
				if (0 == sess.CeWriteFile(base.handle, buf, count, ref ret, IntPtr.Zero))
					throw new RapiException(sess.CeGetLastError());
				return ret;
			}

			protected override bool ReleaseHandle()
			{
				if (!IsInvalid)
				{
					if (0 == sess.CeCloseHandle(base.handle))
						return false;
				}
				return true;
			}

			internal class FileTimes
			{
				public System.Runtime.InteropServices.ComTypes.FILETIME cft, aft, wft;

				public FileTimes()
				{
				}

				public DateTime CreationTime
				{
					get { return cft.ToDateTime(); } set { cft = value.ToFILETIME(); }
				}

				public DateTime LastAccessTime
				{
					get { return aft.ToDateTime(); } set { aft = value.ToFILETIME(); }
				}

				public DateTime LastWriteTime
				{
					get { return wft.ToDateTime(); } set { wft = value.ToFILETIME(); }
				}
			}
		}

		internal class DeviceHandle : SafeHandle
		{
			private IRAPISession sess;

			public DeviceHandle(IRAPISession session, IntPtr handle)
				: base(InvalidHandleValue, true)
			{
				sess = session;
				base.SetHandle(handle);
			}

			public override bool IsInvalid
			{
				get { return base.handle == IntPtr.Zero || base.handle == InvalidHandleValue; }
			}

			/// <summary>
			/// Allows to use DeviceHandle as IntPtr
			/// </summary>
			public static implicit operator IntPtr(DeviceHandle f)
			{
				return f.DangerousGetHandle();
			}

			protected override bool ReleaseHandle()
			{
				if (!IsInvalid)
				{
					if (0 == sess.CeCloseHandle(base.handle))
						return false;
				}
				return true;
			}
		}

		private class RAPIConnectionInfo
		{
			public System.Net.IPEndPoint addr, host;
			public ConnectionType connType;

			public RAPIConnectionInfo(ref RAPI_CONNECTIONINFO ci)
			{
				this.connType = ci.connectionType;
				addr = ci.ipaddr;
				host = ci.hostIpaddr;
			}
		}
	}
}

namespace System.Runtime.CompilerServices
{
	/// <summary>
	/// Attribute allowing extenders to be used with .NET Framework 2.0.
	/// </summary>
	internal sealed class ExtensionAttribute : Attribute
	{
	}
}