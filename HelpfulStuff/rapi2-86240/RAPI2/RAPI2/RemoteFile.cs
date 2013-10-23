using System.IO;
using System.Text;

namespace System.Devices
{
	/// <summary>
	/// Provides static methods for the creation, copying, deletion, moving, and opening of files, and aids in the creation of <see cref="RemoteFileStream"/> objects. 
	/// </summary>
	public static class RemoteFile
	{
		private const uint CREATE_NEW = 1;
		private const uint CREATE_ALWAYS = 2;
		private const uint OPEN_EXISTING = 3;
		private const int ERROR_NO_MORE_FILES = 18;
		private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
		private const int FILE_SHARE_READ = 0x00000001;
		private const uint GENERIC_WRITE = 0x40000000;
		private const uint GENERIC_READ = 0x80000000;
		private const int FileBufferSize = 0x1000;  // 4k transfer buffer

		/// <summary>
		/// Copies an existing file to a new file. Overwriting a file of the same name is prohibited.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="sourceFileName">The file to copy.</param>
		/// <param name="destFileName">The name of the destination file. This cannot be a directory or an existing file.</param>
		public static void Copy(RemoteDevice device, string sourceFileName, string destFileName)
		{
			Copy(device, sourceFileName, destFileName, false);
		}

		/// <summary>
		/// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="sourceFileName">The file to copy.</param>
		/// <param name="destFileName">The name of the destination file. This cannot be a directory.</param>
		/// <param name="overwrite"><c>true</c> if the destination file can be overwritten; otherwise <c>false</c>.</param>
		public static void Copy(RemoteDevice device, string sourceFileName, string destFileName, bool overwrite)
		{
			if (0 == device.ISession.CeCopyFile(sourceFileName, destFileName, overwrite ? 0 : 1))
				device.ThrowRAPIException();
		}

		/// <summary>
		/// Copies a file from the remote device to the local system.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="deviceFileName">The name of the remote file to copy.</param>
		/// <param name="desktopFileName">The name of the local destination file.</param>
		/// <param name="overwrite">true if the destination file can be overwritten; otherwise, false.</param>
		public static void CopyFileFromDevice(RemoteDevice device, string deviceFileName, string desktopFileName, bool overwrite)
		{
			using (RemoteDevice.DeviceFile remoteFile = new RemoteDevice.DeviceFile(device.ISession, deviceFileName, GENERIC_READ, FILE_SHARE_READ, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL))
			{
				// create the local file
				FileStream localFile = new FileStream(desktopFileName, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);

				try
				{
					// read data from remote file into buffer
					byte[] buffer = new byte[FileBufferSize];
					int bytesread = 0;
					while ((bytesread = remoteFile.Read(buffer, 0, FileBufferSize)) > 0)
					{
						// write it into local file
						localFile.Write(buffer, 0, bytesread);
					}
				}
				finally
				{
					// close the local file
					localFile.Flush();
					localFile.Close();
				}
			}
		}

		/// <summary>
		/// Copies a file from the local system to a remote device.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="desktopFileName">The name of the local file to copy.</param>
		/// <param name="deviceFileName">The name of the remote destination file.</param>
		/// <param name="overwrite"><see langword="true"/> if the destination file can be overwritten; otherwise, <see langword="false"/>.</param>
		public static void CopyFileToDevice(RemoteDevice device, string desktopFileName, string deviceFileName, bool overwrite)
		{
			CopyFileToDevice(device, desktopFileName, deviceFileName, overwrite, null);
		}

		/// <summary>
		/// Copies a file from the local system to a remote device.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="desktopFileName">The name of the local file to copy.</param>
		/// <param name="deviceFileName">The name of the remote destination file.</param>
		/// <param name="overwrite"><see langword="true"/> if the destination file can be overwritten; otherwise, <see langword="false"/>.</param>
		/// <param name="percentProgressCallback">Optional. A callback delegate to receive progress updates. This value can be <see langword="null"/>.</param>
		public static void CopyFileToDevice(RemoteDevice device, string desktopFileName, string deviceFileName, bool overwrite, Action<float> percentProgressCallback)
		{
			using (RemoteDevice.DeviceFile remoteFile = new RemoteDevice.DeviceFile(device.ISession, deviceFileName, GENERIC_WRITE, FILE_SHARE_READ, overwrite ? CREATE_ALWAYS : CREATE_NEW, FILE_ATTRIBUTE_NORMAL))
			{
				using (FileStream localFile = new FileStream(desktopFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					float totalSize = localFile.Length;
					long filepos = 0;
					byte[] buffer = new byte[FileBufferSize];
					int bytesread = localFile.Read(buffer, 0, buffer.Length);
					while (bytesread > 0)
					{
						// move remote file pointer # of bytes read
						filepos += bytesread;

						// write our buffer to the remote file
						remoteFile.Write(buffer, 0, bytesread);

						// notify of progress, if elected
						if (percentProgressCallback != null)
						{
							// use BeginInvoke as a "fire and forget" notification, to help prevent deadlocks
							percentProgressCallback.BeginInvoke(filepos / totalSize, null, null);
						}

						try
						{
							// refill the local buffer
							bytesread = localFile.Read(buffer, 0, buffer.Length);
						}
						catch (Exception)
						{
							bytesread = 0;
						}
					}
					remoteFile.SetEndOfFile();
				}

				remoteFile.SetFileTimes(File.GetCreationTime(desktopFileName), File.GetLastAccessTime(desktopFileName),
					File.GetLastWriteTime(desktopFileName));
			}
		}

		/// <summary>
		/// Creates or overwrites a file in the specified path.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The path and name of the file to create.</param>
		/// <returns>A <see cref="RemoteFileStream"/> that provides read/write access to the file specified in <paramref name="path"/>.</returns>
		public static RemoteFileStream Create(RemoteDevice device, string path)
		{
			return new RemoteFileStream(device, path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
		}

		/// <summary>
		/// Creates or overwrites a file in the specified path.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The path and name of the file to create.</param>
		/// <param name="bufferSize">The number of bytes buffered for reads and writes to the file.</param>
		/// <returns>A <see cref="RemoteFileStream"/> with the specified buffer size that provides read/write access to the file specified in <paramref name="path"/>.</returns>
		public static RemoteFileStream Create(RemoteDevice device, string path, int bufferSize)
		{
			return new RemoteFileStream(device, path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read) { BufferSize = bufferSize };
		}

		//public static StreamWriter CreateText(RemoteDevice device, string path);

		/// <summary>
		/// Deletes the specified file. An exception is not thrown if the specified file does not exist. 
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The name of the file to be deleted.</param>
		public static void Delete(RemoteDevice device, string path)
		{
			if (0 == device.ISession.CeDeleteFile(path))
				device.ThrowRAPIException();
		}

		/// <summary>
		/// Determines whether the specified file exists.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The file to check.</param>
		/// <returns><c>true</c> if the caller has the required permissions and <paramref name="path"/> contains the name of an existing file; otherwise, <c>false</c>. This method also returns <c>false</c> if <paramref name="path"/> is <c>null</c>, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns <c>false</c> regardless of the existence of <paramref name="path"/>.</returns>
		public static bool Exists(RemoteDevice device, string path)
		{
			return (uint)device.ISession.CeGetFileAttributes(path) != 0xFFFFFFFF;
		}

		/// <summary>
		/// Gets the <see cref="FileAttributes"/> of the file on the path.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The path to the file.</param>
		/// <returns>The <see cref="FileAttributes"/> of the file on the path.</returns>
		public static FileAttributes GetAttribtues(RemoteDevice device, string path)
		{
			uint ret = device.ISession.CeGetFileAttributes(path);
			if (ret == 0xFFFFFFFF)
				device.ThrowRAPIException();
			return (System.IO.FileAttributes)ret;
		}

		/// <summary>
		/// Gets the creation date and time of the specified file or directory.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The file or directory for which to obtain creation date and time information.</param>
		/// <returns>A <see cref="DateTime"/> structure set to the creation date and time for the specified file or directory. This value is expressed in local time.</returns>
		public static DateTime GetCreationTime(RemoteDevice device, string path)
		{
			using (RemoteDevice.DeviceFile f = new RemoteDevice.DeviceFile(device.ISession, path))
				return f.GetFileTimes().CreationTime;
		}

		/// <summary>
		/// Gets the date and time the specified file or directory was last accessed.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The file or directory for which to obtain access date and time information.</param>
		/// <returns>A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in local time.</returns>
		public static DateTime GetLastAccessTime(RemoteDevice device, string path)
		{
			using (RemoteDevice.DeviceFile f = new RemoteDevice.DeviceFile(device.ISession, path))
				return f.GetFileTimes().LastAccessTime;
		}

		/// <summary>
		/// Gets the date and time the specified file or directory was last written to.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The file or directory for which to obtain access date and time information.</param>
		/// <returns>A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last written to. This value is expressed in local time.</returns>
		public static DateTime GetLastWriteTime(RemoteDevice device, string path)
		{
			using (RemoteDevice.DeviceFile f = new RemoteDevice.DeviceFile(device.ISession, path))
				return f.GetFileTimes().LastWriteTime;
		}

		/// <summary>
		/// Moves a specified file to a new location, providing the option to specify a new file name.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="sourceFileName">The name of the file to move.</param>
		/// <param name="destFileName">The new path for the file.</param>
		public static void Move(RemoteDevice device, string sourceFileName, string destFileName)
		{
			if (0 == device.ISession.CeMoveFile(sourceFileName, destFileName))
				device.ThrowRAPIException();
		}

		/// <summary>
		/// Opens a <see cref="RemoteFileStream"/> on the specified path with read/write access.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The file to open.</param>
		/// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
		/// <returns>A <see cref="RemoteFileStream"/> opened in the specified mode and path, with read/write access and not shared.</returns>
		public static RemoteFileStream Open(RemoteDevice device, string path, FileMode mode)
		{
			return new RemoteFileStream(device, path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
		}

		/// <summary>
		/// Opens a <see cref="RemoteFileStream"/> on the specified path, with the specified mode and access.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The file to open.</param>
		/// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
		/// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
		/// <returns>An unshared <see cref="RemoteFileStream"/> that provides access to the specified file, with the specified mode and access.</returns>
		public static RemoteFileStream Open(RemoteDevice device, string path, FileMode mode, FileAccess access)
		{
			return new RemoteFileStream(device, path, mode, access, FileShare.None);
		}

		/// <summary>
		/// Opens a <see cref="RemoteFileStream"/> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The file to open.</param>
		/// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
		/// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
		/// <param name="share">A <see cref="FileShare"/> value specifying the type of access other threads have to the file.</param>
		/// <returns>A <see cref="RemoteFileStream"/> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</returns>
		public static RemoteFileStream Open(RemoteDevice device, string path, FileMode mode, FileAccess access, FileShare share)
		{
			return new RemoteFileStream(device, path, mode, access, share);
		}

		/// <summary>
		/// Opens an existing file for reading.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The file to be opened for reading.</param>
		/// <returns>A read-only <see cref="RemoteFileStream"/> on the specified path.</returns>
		public static RemoteFileStream OpenRead(RemoteDevice device, string path)
		{
			return new RemoteFileStream(device, path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
		}

		//public static StreamReader OpenText(RemoteDevice device, string path);

		/// <summary>
		/// Opens an existing file for writing.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The file to be opened for writing.</param>
		/// <returns>An unshared <see cref="RemoteFileStream"/> object on the specified path with <see cref="FileAccess">Write</see> access.</returns>
		public static RemoteFileStream OpenWrite(RemoteDevice device, string path)
		{
			return new RemoteFileStream(device, path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}

		/// <summary>
		/// Reads the contents of a file on a remote device into a array of bytes.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">A file to open for reading.</param>
		/// <returns>
		/// A byte array containing the contents of the file.
		/// </returns>
		public static byte[] ReadAllBytes(RemoteDevice device, string path)
		{
			byte[] ret = new byte[0];
			using (RemoteDevice.DeviceFile remoteFile = new RemoteDevice.DeviceFile(device.ISession, path, GENERIC_READ, FILE_SHARE_READ, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL))
			{
				using (MemoryStream mem = new MemoryStream(10240))
				{
					// read data from remote file into buffer
					byte[] buffer = new byte[FileBufferSize];
					int bytesread = 0;
					while ((bytesread = remoteFile.Read(buffer, 0, FileBufferSize)) > 0)
					{
						// write it into local file
						mem.Write(buffer, 0, bytesread);
					}
					ret = mem.ToArray();
				}
			}
			return ret;
		}

		/// <summary>
		/// Opens a text file on a remote device, reads all lines of the file into a string, and then closes the file.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">A file to open for reading.</param>
		/// <param name="encoding">The encoding applied to the contents of the file.</param>
		/// <returns>
		/// A string containing all lines of the file.
		/// </returns>
		public static string ReadAllText(RemoteDevice device, string path, Encoding encoding)
		{
			return encoding.GetString(ReadAllBytes(device, path));
		}

		/// <summary>
		/// Sets the specified <see cref="FileAttributes"/> of a file or directory on a remote device.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="fileName">The path to the file.</param>
		/// <param name="attr">The desired <see cref="FileAttributes"/>, such as <c>Hidden</c>, <c>ReadOnly</c>, <c>Normal</c>, and <c>Archive</c>.</param>
		public static void SetAttributes(RemoteDevice device, string fileName, System.IO.FileAttributes attr)
		{
			if (0 == device.ISession.CeSetFileAttributes(fileName, (uint)attr))
				device.ThrowRAPIException();
		}

		/// <summary>
		/// Sets the date and time that a file was created, last accessed, or last modified for files on a remote device.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="fileName">String that specifies the name of a file.</param>
		/// <param name="creationTime">The date and time the file was created. This parameter can be NULL if the application does not need to set this information.</param>
		/// <param name="lastAccessTime">The date and time the file was last accessed. This parameter can be NULL if the application does not need to set this information.</param>
		/// <param name="lastWriteTime">The date and time the file was last modified. This parameter can be NULL if the application does not need to set this information.</param>
		internal static void SetFileTimes(RemoteDevice device, string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
		{
			using (RemoteDevice.DeviceFile f = new RemoteDevice.DeviceFile(device.ISession, fileName))
				f.SetFileTimes(creationTime, lastAccessTime, lastWriteTime);
		}
	}
}