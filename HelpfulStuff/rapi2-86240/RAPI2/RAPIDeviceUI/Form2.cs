using System;
using System.Devices;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RAPIDeviceUI
{
	public partial class Form2 : Form
	{
		RemoteDeviceManager mgr;
		RemoteDevice dev;

		public Form2()
		{
			InitializeComponent();
			mgr = new RemoteDeviceManager();
			mgr.DeviceConnected += mgr_DeviceConnected;
			mgr.DeviceDisconnected += mgr_DeviceDisconnected;
		}

		private void mgr_DeviceConnected(object sender, RemoteDeviceConnectEventArgs e)
		{
			dev = e.Device;
			OnConnection(true);
		}

		private void mgr_DeviceDisconnected(object sender, RemoteDeviceConnectEventArgs e)
		{
			dev = e.Device;
			OnConnection(false);
		}

		private void OnConnection(bool connected)
		{
			if (connected)
			{
				deviceStatusText.Clear();
				backgroundWorker1.RunWorkerAsync(dev);
			}
			else
				statusLabel.Text = "No devices have been connected.";
			statusLabel.Visible = !connected;
			tabControl1.Visible = connected;
		}

		private string GetDeviceInfo(RemoteDevice dev)
		{
			StringBuilder sb = new StringBuilder();
			try
			{
				sb.AppendFormat("Name:\t{0}\r\n", dev.Name);
				sb.AppendFormat("ID:\t{0}\r\n", dev.DeviceId);
				sb.AppendFormat("Platform:\t{0}\r\n", dev.Platform);
				sb.AppendFormat("Status:\t{0}\r\n", dev.Status);
				if (dev.Status == DeviceStatus.Connected)
				{
					sb.AppendFormat("OS:\t{0}\r\n", dev.OSVersion);
					sb.AppendFormat("Connection:\t{0}\r\n", dev.ConnectionType);
					sb.AppendFormat("IP Address:\t{0}\r\n", dev.IPAddress);
					sb.AppendFormat("Power:\t{0}% ({1})\r\n", dev.PowerStatus.BatteryLifePercent, dev.PowerStatus.ACLineStatus == 1 ? "On AC" : "On battery");
					sb.AppendFormat("Phys Mem:\t{0}K of {1}K\r\n", dev.MemoryStatus.AvailPhysical >> 10, dev.MemoryStatus.TotalPhysical >> 10);
					sb.AppendFormat("Virtual Mem:\t{0}K of {1}K\r\n", dev.MemoryStatus.AvailableVirtual >> 10, dev.MemoryStatus.TotalVirtual >> 10);

					// SystemInformation
					sb.AppendFormat("Processor:\t{0}\r\n", dev.SystemInformation.ProcessorArchitecture);
					sb.AppendFormat("Screen Size:\t{0}x{1} px\r\n", dev.GetSystemMetrics(SystemMetricsItem.SM_CXSCREEN), dev.GetDeviceCaps(DeviceCapsItem.VERTRES));

					// Storage
					RemoteDevice.DriveInfo drv = dev.GetDriveInfo(@"\My Documents");
					sb.AppendFormat("Drive Info:\t{0}K of {1}K\r\n", drv.AvailableFreeSpace >> 10, drv.TotalSize >> 10);
					sb.AppendFormat("Store Info:\t{1}K of {0}K\r\n", dev.StoreInfo.StoreSize >> 10, dev.StoreInfo.FreeSize >> 10);
					sb.AppendFormat("Doc folder:\t{0}\r\n", dev.GetFolderPath(SpecialFolder.MyDocuments));
					sb.AppendFormat("Temp folder:\t{0}\r\n", dev.GetTempPath());
				}
			}
			catch (Exception ex)
			{
				sb.Append(ex.ToString());
			}
			return sb.ToString();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			using (Graphics g = deviceStatusText.CreateGraphics())
				deviceStatusText.SelectionTabs = new int[] { (int)g.MeasureString("Screen Size:  ", deviceStatusText.Font).Width };
			this.Text = this.Text + " - " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
			OnConnection((dev = mgr.Devices.FirstConnectedDevice) != null);
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			mgr.DeviceConnected -= mgr_DeviceConnected;
			mgr.DeviceDisconnected -= mgr_DeviceDisconnected;
		}

		delegate void StrDelegate(string str);

		private void SetStatus(string str) { deviceStatusText.Text = str; }

		private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			RemoteDevice mDev = mgr.Devices.FirstConnectedDevice; //e.Argument as RemoteDevice;
			if (mDev != null)
			{
				string info = GetDeviceInfo(mDev);
				deviceStatusText.Invoke(new StrDelegate(SetStatus), info);
			}
		}
	}
}
