using System.IO;

namespace System.Devices
{
	/// <summary>
	/// Exposes instance methods for creating, moving, and enumerating through directories and subdirectories. This class cannot be inherited.
	/// </summary>
	public sealed class RemoteDirectoryInfo : RemoteFileSystemInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RemoteDirectoryInfo"/> class on the specified path.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">A string specifying the path on which to create the <see cref="RemoteDirectoryInfo"/>.</param>
		public RemoteDirectoryInfo(RemoteDevice device, string path)
		{
			base.Device = device;
			if (path == null)
				throw new ArgumentNullException();
			base.FullPath = Path.GetFullPath(path);
		}

		/// <summary>
		/// Gets the name of this <see cref="RemoteDirectoryInfo"/> instance.
		/// </summary>
		/// <value>The directory name.</value>
		public override string Name
		{
			get
			{
				string fullPath = base.FullPath;
				if (fullPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					fullPath = base.FullPath.Substring(0, base.FullPath.Length - 1);
				}
				return Path.GetFileName(fullPath);
			}
		}

		/// <summary>
		/// Gets the parent directory of a specified subdirectory.
		/// </summary>
		/// <value>The parent directory, or <c>null</c> if the path is null or if the file path denotes a root (such as "\").</value>
		public RemoteDirectoryInfo Parent
		{
			get
			{
				string fullPath = base.FullPath;
				if (fullPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					fullPath = base.FullPath.Substring(0, base.FullPath.Length - 1);
				}
				string directoryName = Path.GetDirectoryName(fullPath);
				if (directoryName == null)
				{
					return null;
				}
				return new RemoteDirectoryInfo(base.Device, directoryName);
			}
		}

		/// <summary>
		/// Gets the root portion of a path.
		/// </summary>
		/// <value>A <see cref="RemoteDirectoryInfo"/> object representing the root of a path.</value>
		public RemoteDirectoryInfo Root
		{
			get
			{
				return new RemoteDirectoryInfo(base.Device, @"\");
			}
		}

		/// <summary>
		/// Creates a directory.
		/// </summary>
		public void Create()
		{
			RemoteDirectory.CreateDirectory(base.Device, base.FullPath);
		}

		/// <summary>
		/// Creates a subdirectory or subdirectories on the specified path. The specified path can be relative to this instance of the <see cref="RemoteDirectoryInfo"/> class. 
		/// </summary>
		/// <param name="path">The specified path.</param>
		/// <returns>The last directory specified in path.</returns>
		public RemoteDirectoryInfo CreateSubdirectory(string path)
		{
			return RemoteDirectory.CreateDirectory(base.Device, Path.Combine(base.FullPath, path));
		}

		/// <summary>
		/// Deletes the <see cref="RemoteDirectoryInfo"/> if it is empty.
		/// </summary>
		public override void Delete()
		{
			RemoteDirectory.Delete(base.Device, base.FullPath, false);
		}

		/// <summary>
		/// Deletes this instance of a <see cref="RemoteDirectoryInfo"/>, specifying whether to delete subdirectories and files.
		/// </summary>
		/// <param name="recursive"><c>true</c> to delete this directory, its subdirectories, and all files; otherwise <c>false</c>.</param>
		public void Delete(bool recursive)
		{
			RemoteDirectory.Delete(base.Device, base.FullPath, recursive);
		}

		/// <summary>
		/// Returns the subdirectories of the current directory.
		/// </summary>
		/// <returns>An array of <see cref="RemoteDirectoryInfo"/> objects.</returns>
		public RemoteDirectoryInfo[] GetDirectories()
		{
			return GetDirectories("*");
		}

		/// <summary>
		/// Returns an array of directories in the current <see cref="RemoteDirectoryInfo"/> matching the given search criteria. 
		/// </summary>
		/// <param name="searchPattern">The search string, such as "System*", used to search for all directories beginning with the word "System".</param>
		/// <returns>An array of type <see cref="RemoteDirectoryInfo"/> matching the <paramref name="searchPattern"/>.</returns>
		public RemoteDirectoryInfo[] GetDirectories(string searchPattern)
		{
			string[] ret = RemoteDirectory.GetDirectories(base.Device, base.FullPath, searchPattern);
			return Array.ConvertAll<string, RemoteDirectoryInfo>(ret, delegate(string s) { return new RemoteDirectoryInfo(base.Device, s); });
		}

		/// <summary>
		/// Returns a file list from the current directory.
		/// </summary>
		/// <returns>An array of type <see cref="RemoteFileInfo"/>.</returns>
		public RemoteFileInfo[] GetFiles()
		{
			return GetFiles("*");
		}

		/// <summary>
		/// Returns a file list from the current directory matching the given <paramref name="searchPattern"/>.
		/// </summary>
		/// <param name="searchPattern">The search string, such as "*.txt".</param>
		/// <returns>An array of type <see cref="RemoteFileInfo"/>.</returns>
		public RemoteFileInfo[] GetFiles(string searchPattern)
		{
			string[] ret = RemoteDirectory.GetFiles(base.Device, base.FullPath, searchPattern);
			return Array.ConvertAll<string, RemoteFileInfo>(ret, delegate(string s) { return new RemoteFileInfo(base.Device, s); });
		}

		/// <summary>
		/// Moves a <see cref="RemoteDirectoryInfo"/> instance and its contents to a new path.
		/// </summary>
		/// <param name="destDirName">The name and path to which to move this directory. The destination cannot be another disk volume or a directory with the identical name. It can be an existing directory to which you want to add this directory as a subdirectory.</param>
		public void MoveTo(string destDirName)
		{
			RemoteDirectory.Move(base.Device, base.FullPath, destDirName);
		}

		/// <summary>
		/// Returns the path that was passed by the user.
		/// </summary>
		/// <returns>
		/// Returns the path that was passed by the user.
		/// </returns>
		public override string ToString()
		{
			return base.FullPath;
		}
	}
}