using System.IO;

namespace System.Devices
{
	/// <summary>
	/// Provides instance methods for the creation, copying, deletion, moving, and opening of files, and aids in the creation of <see cref="RemoteFileStream"/> objects. This class cannot be inherited.
	/// </summary>
	public sealed class RemoteFileInfo : RemoteFileSystemInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RemoteFileInfo"/> class, which acts as a wrapper for a file path.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="fileName">The fully qualified name of the new file, or the relative file name.</param>
		public RemoteFileInfo(RemoteDevice device, string fileName)
		{
			CheckValidFileName(fileName);
			Device = device;
			FullPath = fileName;
		}

		/// <summary>
		/// Gets an instance of the parent directory.
		/// </summary>
		/// <value>A <see cref="RemoteDirectoryInfo"/> object representing the parent directory of this file.</value>
		public RemoteDirectoryInfo Directory
		{
			get { return new RemoteDirectoryInfo(base.Device, this.DirectoryName); }
		}

		/// <summary>
		/// Gets a string representing the directory's full path.
		/// </summary>
		/// <value>A string representing the directory's full path.</value>
		public string DirectoryName
		{
			get { return Path.GetDirectoryName(FullPath); }
		}

		/// <summary>
		/// Gets the size, in bytes, of the current file.
		/// </summary>
		/// <value>The size of the current file in bytes.</value>
		public long Length
		{
			get
			{
				using (RemoteDevice.DeviceFile f = new RemoteDevice.DeviceFile(Device.ISession, FullPath))
					return (long)f.Size;
			}
		}

		/// <summary>
		/// Gets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
		public override string Name
		{
			get { return Path.GetFileName(FullPath); }
		}

		//public StreamWriter AppendText();

		/// <summary>
		/// Copies an existing file to a new file, disallowing the overwriting of an existing file.
		/// </summary>
		/// <param name="destFileName">Name of the new file to copy to.</param>
		/// <returns>A new file with a fully qualified path.</returns>
		public RemoteFileInfo CopyTo(string destFileName)
		{
			return CopyTo(destFileName, false);
		}

		/// <summary>
		/// Copies an existing file to a new file, allowing the overwriting of an existing file.
		/// </summary>
		/// <param name="destFileName">The name of the new file to copy to.</param>
		/// <param name="overwrite"><c>true</c> to allow an existing file to be overwritten; otherwise <c>false</c>.</param>
		/// <returns>A new file, or an overwrite of an existing file if overwrite is <c>true</c>. If the file exists and overwrite is <c>false</c>, an exception is thrown.</returns>
		public RemoteFileInfo CopyTo(string destFileName, bool overwrite)
		{
			CheckValidFileName(destFileName);
			RemoteFile.Copy(Device, FullPath, destFileName, overwrite);
			return new RemoteFileInfo(Device, destFileName);
		}

		/// <summary>
		/// Creates a file.
		/// </summary>
		/// <returns>A new file.</returns>
		public RemoteFileStream Create()
		{
			return new RemoteFileStream(Device, FullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
		}

		/// <summary>
		/// Deletes a file.
		/// </summary>
		public override void Delete()
		{
			RemoteFile.Delete(Device, FullPath);
		}

		/// <summary>
		/// Moves a specified file to a new location, providing the option to specify a new file name.
		/// </summary>
		/// <param name="destFileName">The path to move the file to, which can specify a different file name.</param>
		public void MoveTo(string destFileName)
		{
			CheckValidFileName(destFileName);
			RemoteFile.Move(Device, FullPath, destFileName);
		}

		/// <summary>
		/// Opens a file in the specified mode. 
		/// </summary>
		/// <param name="mode">A <see cref="FileMode"/> constant specifying the mode (for example, <c>Open</c> or <c>Append</c>) in which to open the file. </param>
		/// <returns>A <see cref="RemoteFileStream"/> opened with the specified mode, with read/write access and unshared.</returns>
		public RemoteFileStream Open(FileMode mode)
		{
			return new RemoteFileStream(Device, FullPath, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
		}

		/// <summary>
		/// Opens a file in the specified mode with read, write, or read/write access.
		/// </summary>
		/// <param name="mode">A <see cref="FileMode"/> constant specifying the mode (for example, <c>Open</c> or <c>Append</c>) in which to open the file. </param>
		/// <param name="access">A <see cref="FileAccess"/> constant specifying whether to open the file with <c>Read</c>, <c>Write</c>, or <c>ReadWrite</c> file access.</param>
		/// <returns>A <see cref="RemoteFileStream"/> object opened in the specified mode and access, and unshared.</returns>
		public RemoteFileStream Open(FileMode mode, FileAccess access)
		{
			return new RemoteFileStream(Device, FullPath, mode, access, FileShare.None);
		}

		/// <summary>
		/// Opens a file in the specified mode with read, write, or read/write access and the specified sharing option. 
		/// </summary>
		/// <param name="mode">A <see cref="FileMode"/> constant specifying the mode (for example, <c>Open</c> or <c>Append</c>) in which to open the file. </param>
		/// <param name="access">A <see cref="FileAccess"/> constant specifying whether to open the file with <c>Read</c>, <c>Write</c>, or <c>ReadWrite</c> file access.</param>
		/// <param name="share">A <see cref="FileShare"/> constant specifying the type of access other <see cref="RemoteFileStream"/> objects have to this file. </param>
		/// <returns>A <see cref="RemoteFileStream"/> object opened with the specified mode, access, and sharing options.</returns>
		public RemoteFileStream Open(FileMode mode, FileAccess access, FileShare share)
		{
			return new RemoteFileStream(Device, FullPath, mode, access, share);
		}

		/// <summary>
		/// Creates a read-only <see cref="RemoteFileStream"/>.
		/// </summary>
		/// <returns>A new read-only <see cref="RemoteFileStream"/> object.</returns>
		public RemoteFileStream OpenRead()
		{
			return new RemoteFileStream(Device, FullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
		}

		//public StreamReader OpenText();

		/// <summary>
		/// Creates a write-only <see cref="RemoteFileStream"/>.
		/// </summary>
		/// <returns>A new write-only <see cref="RemoteFileStream"/> object.</returns>
		public RemoteFileStream OpenWrite()
		{
			return new RemoteFileStream(Device, FullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}

		/// <summary>
		/// Returns the path as a string.
		/// </summary>
		/// <returns>
		/// A string representing the path.
		/// </returns>
		public override string ToString()
		{
			return FullPath;
		}

		/// <summary>
		/// Validates the supplied file name and throws exceptions if invalid.
		/// </summary>
		/// <param name="fileName">Name of the file to validate.</param>
		private void CheckValidFileName(string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException();
			fileName = fileName.TrimEnd(null);
			if (fileName.Length == 0 || fileName.IndexOfAny(Path.GetInvalidPathChars()) != -1)
				throw new ArgumentException();
		}
	}
}