using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.Devices
{
	/// <summary>
	/// Provides the base class for both <see cref="RemoteFileInfo"/> and <see cref="RemoteDirectoryInfo"/> objects.
	/// </summary>
	public abstract class RemoteFileSystemInfo
	{
		internal RemoteDevice.DeviceFile.FileTimes ftimes;

		/// <summary>Represents the fully qualified path of the directory or file.</summary>
		protected string FullPath;
		/// <summary>The device on which to perform all file and directory operations.</summary>
		protected RemoteDevice Device;

		/// <summary>
		/// Gets or sets the <see cref="FileAttributes"/> of the current <see cref="RemoteFileSystemInfo"/>.
		/// </summary>
		/// <value><see cref="FileAttributes"/> of the current <see cref="RemoteFileSystemInfo"/>.</value>
		public FileAttributes Attributes
		{
			get { return RemoteFile.GetAttribtues(Device, FullPath); }
			set { RemoteFile.SetAttributes(Device, FullPath, value); }
		}

		/// <summary>
		/// Gets or sets the creation date and time of the current <see cref="RemoteFileSystemInfo"/> object.
		/// </summary>
		/// <value>The creation date and time of the current <see cref="RemoteFileSystemInfo"/> object.</value>
		public DateTime CreationTime
		{
			get { if (ftimes == null) Refresh(); return ftimes.CreationTime; }
		}

		/// <summary>
		/// Gets a value indicating whether the file or directory exists. 
		/// </summary>
		/// <value><c>true</c> if the file or directory exists; otherwise, <c>false</c>.</value>
		public bool Exists
		{
			get { return RemoteFile.Exists(Device, FullPath); }
		}

		/// <summary>
		/// Gets the string representing the extension part of the file.
		/// </summary>
		/// <value>A string containing file extension.</value>
		public string Extension
		{
			get { return Path.GetExtension(FullPath); }
		}

		/// <summary>
		/// Gets the full path of the directory or file.
		/// </summary>
		/// <value>A string containing the full path of the directory or file.</value>
		public string FullName
		{
			get { return FullPath; }
		}

		/// <summary>
		/// Gets or sets the date and time the current file or directory was accessed.
		/// </summary>
		/// <value>The date and time the current file or directory was accessed.</value>
		public DateTime LastAccessTime
		{
			get { if (ftimes == null) Refresh(); return ftimes.LastAccessTime; }
		}

		/// <summary>
		/// Gets or sets the date and time the current file or directory was written to.
		/// </summary>
		/// <value>The date and time the current file or directory was written to.</value>
		public DateTime LastWriteTime
		{
			get { if (ftimes == null) Refresh(); return ftimes.LastWriteTime; }
		}

		/// <summary>
		/// For files, gets the name of the file. For directories, gets the name of the last directory in the hierarchy if a hierarchy exists. Otherwise, the <c>Name</c> property gets the name of the directory.
		/// </summary>
		/// <value>A string that is the name of the parent directory, the name of the last directory in the hierarchy, or the name of a file, including the file name extension.</value>
		public abstract string Name { get; }

		/// <summary>
		/// Deletes a file or directory.
		/// </summary>
		public abstract void Delete();

		/// <summary>
		/// Refreshes the state of the object.
		/// </summary>
		public void Refresh()
		{
			using (RemoteDevice.DeviceFile f = new RemoteDevice.DeviceFile(Device.ISession, FullPath))
				ftimes = f.GetFileTimes();
		}

		/// <summary>
		/// Sets the file times.
		/// </summary>
		/// <param name="creationTime">The creation time.</param>
		/// <param name="lastAccessTime">The last access time.</param>
		/// <param name="lastWriteTime">The last write time.</param>
		internal void SetFileTimes(DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
		{
			using (RemoteDevice.DeviceFile f = new RemoteDevice.DeviceFile(Device.ISession, FullPath))
				f.SetFileTimes(creationTime, lastAccessTime, lastWriteTime);
		}
	}
}