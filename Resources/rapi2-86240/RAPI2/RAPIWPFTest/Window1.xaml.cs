using System;
using System.Devices;
using System.Windows;

namespace RAPIWPFTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		RemoteDeviceManager _mgr;
		delegate void BoolMethod(bool value);

		public MainWindow()
		{
			InitializeComponent();
			_mgr = new RemoteDeviceManager();
			_mgr.DeviceConnected += mgr_DeviceConnected;
			_mgr.DeviceDisconnected += mgr_DeviceDisconnected;
			using (RemoteDevice dev = _mgr.Devices.FirstConnectedDevice)
				WriteStatus(label1, dev, dev != null);
		}

		private void mgr_DeviceConnected(object sender, RemoteDeviceConnectEventArgs e)
		{
			WriteStatus(label1, e.Device, true);
			Dispatcher.Invoke(new BoolMethod(this.OnConnection), true);
		}

		private void mgr_DeviceDisconnected(object sender, RemoteDeviceConnectEventArgs e)
		{
			WriteStatus(label1, e.Device, false);
			Dispatcher.Invoke(new BoolMethod(this.OnConnection), false);
		}

		private void WriteStatus(System.Windows.Controls.Label lbl, RemoteDevice dev, bool connected)
		{
			if (connected)
				lbl.Content = string.Format("{0} Connected. OS: {1}", dev.Name, dev.OSVersion);
			else
				lbl.Content = string.Format("{0} Disconnected.", dev == null ? "All Devices" : dev.Name);
		}

		private void OnConnection(bool connected)
		{
			using (RemoteDevice dev = _mgr.Devices.FirstConnectedDevice)
				WriteStatus(label2, dev, dev != null);
		}
	}
}
