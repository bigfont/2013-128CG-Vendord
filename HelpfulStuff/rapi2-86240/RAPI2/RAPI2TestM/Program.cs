using System;
using System.Devices;
using System.Threading;

namespace RAPI2TestM
{
	class Program
	{
		static System.Threading.AutoResetEvent s = new AutoResetEvent(true);

		static void Main(string[] args)
		{
			RemoteDeviceManager r = new RemoteDeviceManager();
			r.UnsafeThreadDeviceConnectedNotice += r_DeviceConnected;
			r.UnsafeThreadDeviceDisconnectedNotice += r_DeviceDisconnected;
			while (!Console.KeyAvailable)
			{
				Console.Write('.');
				Thread.Sleep(500);
			}
			r.UnsafeThreadDeviceConnectedNotice -= r_DeviceConnected;
			r.UnsafeThreadDeviceDisconnectedNotice -= r_DeviceDisconnected;
		}

		static void r_DeviceConnected(object sender, EventArgs e)
		{
			Console.WriteLine("Device has been connected.");

			RemoteDeviceManager r = new RemoteDeviceManager();
			RemoteDevice dev = r.Devices.FirstConnectedDevice;
			if (dev == null)
				return;

			// Device information
			Console.WriteLine(dev.Name + ":" + dev.Platform);
			Console.WriteLine(dev.Status);
			Console.WriteLine(dev.ConnectionType);
			Console.WriteLine(dev.SystemInformation.ProcessorArchitecture);
			Console.WriteLine(dev.OSVersion);
			Console.WriteLine("Screen height is {0}px", dev.GetDeviceCaps(DeviceCapsItem.VERTRES));
			Console.WriteLine("Screen width is {0}px", dev.GetSystemMetrics(SystemMetricsItem.SM_CXSCREEN));
			RemoteDevice.DriveInfo drv = dev.GetDriveInfo(@"\My Documents");
			Console.WriteLine("Disk free is {0}K, {1}K, {2}K", drv.AvailableFreeSpace >> 10, drv.TotalSize >> 10, drv.TotalFreeSpace >> 10);
			string myDocs = dev.GetFolderPath(SpecialFolder.MyDocuments);
			Console.WriteLine("Documents folder: {0}", myDocs);
			Console.WriteLine("Temp folder: {0}", dev.GetTempPath());
			Console.WriteLine("Remaining power: {0}%", dev.PowerStatus.BatteryLifePercent);
			Console.WriteLine("Avail mem: {0}", dev.MemoryStatus.AvailPhysical);
			dev.StartSync();
			System.Threading.Thread.Sleep(2000);
			dev.StopSync();
			Console.WriteLine("Store Info: {1}/{0}", dev.StoreInfo.StoreSize, dev.StoreInfo.FreeSize);

			// Files & Directories
			string deviceFile = myDocs + @"\Test.txt";

			string localFile = System.IO.Path.GetTempFileName();
			System.Text.StringBuilder sb = new System.Text.StringBuilder(1000);
			for (char i = '0'; i <= '9'; i++)
				sb.Append(i, 100);
			System.IO.File.WriteAllText(localFile, sb.ToString());

			RemoteFile.CopyFileToDevice(dev, localFile, deviceFile, true);
			RemoteFile.CopyFileFromDevice(dev, myDocs + @"\Test.txt", localFile, true);
			RemoteDirectory.CreateDirectory(dev, myDocs + @"\TestDir");
			RemoteFile.Copy(dev, myDocs + @"\Test.txt", myDocs + @"\TestDir\test2.txt", true);
			RemoteFile.Move(dev, myDocs + @"\TestDir\test2.txt", myDocs + @"\test2.txt");
			Console.WriteLine(@"{0}: Size={1}, FT={2:u},{3:u},{4:u}, Attr={5}", deviceFile,
				"?",
				RemoteFile.GetCreationTime(dev, deviceFile),
				RemoteFile.GetLastAccessTime(dev, deviceFile),
				RemoteFile.GetLastWriteTime(dev, deviceFile),
				RemoteFile.GetAttribtues(dev, deviceFile));
			RemoteFile.Delete(dev, myDocs + @"\test2.txt");
			RemoteDirectory.Delete(dev, myDocs + @"\TestDir");

			Console.WriteLine("Directory listing:");
			foreach (string fn in RemoteDirectory.GetFiles(dev, myDocs, "*"))
				Console.WriteLine("   " + fn);

			dev.CreateShortcut("Test", @"\Temp\Test.txt");
			Console.WriteLine("Test shortcut = " + dev.GetShortcutTarget("Test"));
			RemoteFile.Delete(dev, "Test");

			// RemoteFileInfo
			RemoteFileInfo fi = new RemoteFileInfo(dev, deviceFile);
			if (fi.Exists)
			{
				Console.WriteLine(@"{0}\{6}: Size={1}, FT={2:u},{3:u},{4:u}, Attr={5}", fi.DirectoryName,
					fi.Length, fi.CreationTime, fi.LastAccessTime, fi.LastWriteTime, fi.Attributes,
					fi.Name, fi.Extension);
				using (RemoteFileStream str = fi.Create())
				{
					byte[] txt = new System.Text.UnicodeEncoding().GetBytes("This is a simple text file.");
					str.Write(txt, 0, txt.Length);
					str.Flush();
				}
				using (RemoteFileStream str = fi.OpenRead())
				{
					byte[] txt = new byte[40];
					str.Seek(12, System.IO.SeekOrigin.Begin);
					int l = str.Read(txt, 0, 40);
					Console.WriteLine("{0} bytes read from {1}: {2}", l, deviceFile, new System.Text.UnicodeEncoding().GetString(txt));
				}
			}

			// Databases
			uint dbid = 0;
			try { dbid = RemoteDatabase.Create(dev, "MyTestDB", 0); }
			catch { }
			try
			{
				using (RemoteDatabase db = new RemoteDatabase(dev, "MyTestDB", true, 0))
				{
					if (dbid == 0) dbid = db.ObjectId;
					uint rec = db.WriteRecordProps(0, new RemoteDatabase.PropertyValue[] {
						new RemoteDatabase.PropertyValue(5),
						new RemoteDatabase.PropertyValue("Sally"),
						new RemoteDatabase.PropertyValue((ushort)3),
						new RemoteDatabase.PropertyValue(DateTime.Now) });
					db.Seek(RemoteDatabase.DatabaseSeekType.Beginning, 0);
					RemoteDatabase.PropertyValue[] vals = db.ReadRecordProps(null);
					Console.WriteLine("Database: Name={0}, Size={1}, Recs={2}, LastWrite={3}", db.Name, db.Size, db.RecordCount, db.LastWriteTime);
					db.DeleteRecord(rec);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Database failure: ", ex.ToString());
			}
			finally { RemoteDatabase.Delete(dev, dbid); }

			foreach (var db in dev.Databases)
			{
				Console.WriteLine("Database: Name={0}, Size={1}, Recs={2}, LastWrite={3}", db.Name, db.Size, db.RecordCount, db.LastWriteTime);
			}

			// Registry
			foreach (string item in dev.DeviceRegistryLocalMachine.GetSubKeyNames())
				Console.WriteLine("HKLM\\" + item);
			using (RemoteDevice.DeviceRegistryKey hKey = dev.DeviceRegistryLocalMachine.OpenSubKey("Ident"))
			{
				Console.WriteLine("Device Name: " + hKey.GetValue("Name", string.Empty).ToString());
				using (RemoteDevice.DeviceRegistryKey hKey2 = hKey.CreateSubKey("Test"))
				{
					hKey2.SetValue("StrVal", "test");
					hKey2.SetValue("DWVal", (int)55);
					hKey2.SetValue("ByVal", new byte[] { 0, 1, 2, 3, 4, 5, 6 });
					hKey2.SetValue("MStrVal", new string[] { "str1", "str2" });
					foreach (string item in hKey2.GetValueNames())
					{
						object o = hKey2.GetValue(item, string.Empty);
						if (o is byte[]) o = string.Join(", ", Array.ConvertAll<byte, string>(o as byte[], delegate(byte b) { return b.ToString(); }));
						if (o is string[]) o = string.Join(", ", o as string[]);
						Console.WriteLine("Test\\{0} = {1}", item, o);
					}
					hKey2.DeleteValue("StrVal");
				}
				hKey.DeleteSubKey("Test");
			}

			dev.CreateProcess(@"\Windows\calendar.exe", null, ProcessCreationFlags.None);
		}

		static void r_DeviceDisconnected(object sender, EventArgs e)
		{
			Console.WriteLine("Device has been disconnected.");
		}
	}
}
