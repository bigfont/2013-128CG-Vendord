using System.Devices.Interop;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Devices
{
	/// <summary>
	/// Exposes static methods for creating, moving, and enumerating through directories and subdirectories on a remote Windows CE device. This class cannot be inherited.
	/// </summary>
	public static class RemoteDirectory
	{
		private const int MAX_PATH = 260;

		/// <summary>
		/// Creates all directories and subdirectories as specified by path.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The directory path to create.</param>
		/// <returns>A <see cref="RemoteDirectoryInfo"/> as specified by <paramref name="path"/>.</returns>
		public static RemoteDirectoryInfo CreateDirectory(RemoteDevice device, string path)
		{
			if (path.Length > (MAX_PATH - 1))
				throw new ArgumentException(string.Format("Directory paths may not exceed {0} characters.", MAX_PATH - 1), "pathName");
			if (0 == device.ISession.CeCreateDirectory(path, IntPtr.Zero))
				device.ThrowRAPIException();
			return new RemoteDirectoryInfo(device, path);
		}

		/// <summary>
		/// Deletes an empty directory from a specified path.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The name of the empty directory to remove. This directory must be writable and empty.</param>
		public static void Delete(RemoteDevice device, string path)
		{
			Delete(device, path, false);
		}

		/// <summary>
		/// Deletes an empty directory from a specified path.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The name of the empty directory to remove. This directory must be writable or empty.</param>
		/// <param name="recursive"><c>true</c> to remove directories, subdirectories, and files in path; otherwise, <c>false</c>.</param>
		public static void Delete(RemoteDevice device, string path, bool recursive)
		{
			if (Exists(device, path))
			{
				if (recursive)
				{
					foreach (var file in GetFiles(device, path))
						RemoteFile.Delete(device, file);
					foreach (var dir in GetDirectories(device, path))
						Delete(device, dir, true);
				}
				if (0 == device.ISession.CeRemoveDirectory(path))
					device.ThrowRAPIException();
			}
		}

		/// <summary>
		/// Determines whether the given path refers to an existing directory on disk.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The path to test.</param>
		/// <returns><c>true</c> if <paramref name="path"/> refers to an existing directory; otherwise, <c>false</c>.</returns>
		public static bool Exists(RemoteDevice device, string path)
		{
			return RemoteFile.Exists(device, path);
		}

		/// <summary>
		/// Gets the creation date and time of a directory.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The path of the directory.</param>
		/// <returns>A <see cref="DateTime"/> structure set to the creation date and time for the specified directory. This value is expressed in local time.</returns>
		public static DateTime GetCreationTime(RemoteDevice device, string path)
		{
			return RemoteFile.GetCreationTime(device, path);
		}

		/// <summary>
		/// Gets the names of subdirectories in the specified directory. 
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The path to search.</param>
		/// <returns>A <see cref="String"/> array containing the names of subdirectories in <paramref name="path"/>.</returns>
		public static string[] GetDirectories(RemoteDevice device, string path)
		{
			return GetDirectories(device, path, "*");
		}

		/// <summary>
		/// Gets an array of directories matching the specified search pattern from the current directory.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The path to search.</param>
		/// <param name="searchPattern">The search string to match against the names of files in path. The parameter cannot end in two periods ("..") or contain two periods ("..") followed by <see cref="Path.DirectorySeparatorChar"/> or <see cref="Path.AltDirectorySeparatorChar"/>, nor can it contain any of the characters in <see cref="Path.InvalidPathChars"/>.</param>
		/// <returns>A <see cref="String"/> array of directories matching the search pattern.</returns>
		public static string[] GetDirectories(RemoteDevice device, string path, string searchPattern)
		{
			return _GetFiles(device, path, searchPattern, 0x4080 /*FAF_NAME|FAF_FOLDERS_ONLY*/, false);
		}

		//public static string GetDirectoryRoot(RemoteDevice device, string path);

		/// <summary>
		/// Returns the names of files in the specified directory.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The directory from which to retrieve the files.</param>
		/// <returns>A <see cref="String"/> array of file names in the specified directory.</returns>
		public static string[] GetFiles(RemoteDevice device, string path)
		{
			return GetFiles(device, path, "*");
		}

		/// <summary>
		/// Returns the names of files in the specified directory that match the specified search pattern.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The directory from which to retrieve the files.</param>
		/// <param name="searchPattern">The search string to match against the names of files in path. The parameter cannot end in two periods ("..") or contain two periods ("..") followed by <see cref="Path.DirectorySeparatorChar"/> or <see cref="Path.AltDirectorySeparatorChar"/>, nor can it contain any of the characters in <see cref="Path.InvalidPathChars"/>.</param>
		/// <returns>A <see cref="String"/> array containing the names of files in the specified directory that match the specified search pattern.</returns>
		public static string[] GetFiles(RemoteDevice device, string path, string searchPattern)
		{
			return _GetFiles(device, path, searchPattern, 0x81 /*FAF_NAME*/, true);
		}

		/// <summary>
		/// Returns the date and time the specified file or directory was last accessed.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The file or directory for which to obtain access date and time information.</param>
		/// <returns>A <see cref="DateTime"/> structure set to the date and time the specified file or directory was last accessed. This value is expressed in local time.</returns>
		public static DateTime GetLastAccessTime(RemoteDevice device, string path)
		{
			return RemoteFile.GetLastAccessTime(device, path);
		}

		/// <summary>
		/// Returns the date and time the specified file or directory was last written to.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">The file or directory for which to obtain access date and time information.</param>
		/// <returns>A <see cref="DateTime"/> structure set to the date and time the specified file or directory was last written to. This value is expressed in local time.</returns>
		public static DateTime GetLastWriteTime(RemoteDevice device, string path)
		{
			return RemoteFile.GetLastWriteTime(device, path);
		}

		/// <summary>
		/// Moves a file or a directory and its contents to a new location.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="sourceDirName">The path of the file or directory to move.</param>
		/// <param name="destDirName">The path to the new location for <paramref name="sourceDirName"/>. If <paramref name="sourceDirName"/> is a file, then <paramref name="destDirName"/> must also be a file name.</param>
		public static void Move(RemoteDevice device, string sourceDirName, string destDirName)
		{
			RemoteFile.Move(device, sourceDirName, destDirName);
		}

		private static string[] _GetFiles(RemoteDevice device, string path, string searchPattern, int flags, bool exclDirs)
		{
			if (exclDirs) flags |= 0x01; // FAF_ATTRIBUTES
			int foundCount = 0;
			IntPtr findDataArray;
			if (0 == device.ISession.CeFindAllFiles(Path.Combine(path, searchPattern), flags, ref foundCount, out findDataArray))
				device.ThrowRAPIException();

			System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>(foundCount);
			try
			{
				CE_FIND_DATA[] fds = new CE_FIND_DATA[foundCount];
				IntPtr current = findDataArray;
				for (int i = 0; i < foundCount; current = (IntPtr)((long)current + Marshal.SizeOf(fds[i++])))
				{
					fds[i] = (CE_FIND_DATA)Marshal.PtrToStructure(current, typeof(CE_FIND_DATA));
					if (exclDirs && (0x10 & fds[i].dwFileAttributes) == 0x10 /*FILE_ATTRIBUTE_DIRECTORY*/)
						continue;
					list.Add(fds[i].Name);
				}
			}
			finally
			{
				device.ISession.CeRapiFreeBuffer(findDataArray);
			}
			return list.ToArray();
		}
	}
}