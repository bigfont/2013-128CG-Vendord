using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using Symbol;

namespace CS_Barcode2Sample1
{
	/// <summary>
	/// The SelectDevice class provides a dialog for displaying and selecting a
	/// list of available Symbol.Barcode2.Device objects.
	/// </summary>
	/// <remarks>
    /// A SelectDevice dialog is displayed with the list of device choices 
    /// and the user selects one of them to be accessed.
	/// </remarks>
	public class SelectDevice : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button CancelButton;
		private System.Windows.Forms.ListBox AvailableDevicesListBox;
		private System.Windows.Forms.Label AvailableDevicesLabel;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.Button OKButton;
		private int MySelection = -1;

		static private Symbol.Barcode2.Device[] OurAvailableDevices = null;
		static private string OurTitle;
		static private int DefaultIndex;

		private static bool bPortrait = true;   // The default dispaly orientation 
		// has been set to Portrait.

		private bool bSkipMaxLen = false;    // The restriction on the maximum 
		// physical length is considered by default.

		private bool bInitialScale = true;   // The flag to track whether the 
		// scaling logic is applied for
		// the first time (from scatch) or not.
		// Based on that, the (outer) width/height values
		// of the form will be set or not.
		// Initially set to true.

		private int resWidthReference = 248;   // The (cached) width of the form. 
		// INITIALLY HAS TO BE SET TO THE WIDTH OF THE FORM AT DESIGN TIME (IN PIXELS).
		// This setting is also obtained from the platform only on
		// Windows CE devices before running the application on the device, as a verification.
		// For PocketPC (& Windows Mobile) devices, the failure to set this properly may result in the distortion of GUI/viewability.

		private int resHeightReference = 289;  // The (cached) height of the form.
		// INITIALLY HAS TO BE SET TO THE HEIGHT OF THE FORM AT DESIGN TIME (IN PIXELS).
		// This setting is also obtained from the platform only on
		// Windows CE devices before running the application on the device, as a verification.
		// For PocketPC (& Windows Mobile) devices, the failure to set this properly may result in the distortion of GUI/viewability.

		private const double maxLength = 4;  // The maximum physical width/height of the sample (in inches).
		// The actual value on the device may slightly deviate from this
		// since the calculations based on the (received) DPI & resolution values 
		// would provide only an approximation, so not 100% accurate.


		/// <summary>
		/// This function does the (initial) scaling of the form
		/// by re-setting the related parameters (if required) and
		/// then calling the Scale(...) internally. 
		/// </summary>
		/// 
		public void DoScale()
		{
			if (Screen.PrimaryScreen.Bounds.Width > Screen.PrimaryScreen.Bounds.Height)
			{
				bPortrait = false; // If the display orientation is not portrait (so it's landscape), set the flag to false.
			}

			if (this.WindowState == FormWindowState.Maximized)    // If the form is maximized by default.
			{
				this.bSkipMaxLen = true; // we need to skip the max. length restriction
			}

			if ((Symbol.Win32.PlatformType.IndexOf("WinCE") != -1) || (Symbol.Win32.PlatformType.IndexOf("WindowsCE") != -1) || (Symbol.Win32.PlatformType.IndexOf("Windows CE") != -1)) // Only on Windows CE devices
			{
				this.resWidthReference = this.Width;   // The width of the form at design time (in pixels) is obtained from the platorm.
				this.resHeightReference = this.Height; // The height of the form at design time (in pixels) is obtained from the platform.
			}

			Scale(this); // Initial scaling of the GUI
		}

		/// <summary>
		/// This function scales the given Form and its child controls in order to
		/// make them completely viewable, based on the screen width and height.
		/// </summary>
		private static void Scale(SelectDevice frm)
		{
			int PSWAW = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;    // The width of the working area (in pixels).
			int PSWAH = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;   // The height of the working area (in pixels).

			// The entire screen has been taken in to account below 
			// in order to decide the half (S)VGA settings etc.
			if (!((Screen.PrimaryScreen.Bounds.Width <= (1.5) * (Screen.PrimaryScreen.Bounds.Height))
				&& (Screen.PrimaryScreen.Bounds.Height <= (1.5) * (Screen.PrimaryScreen.Bounds.Width))))
			{
				if ((Screen.PrimaryScreen.Bounds.Width) > (Screen.PrimaryScreen.Bounds.Height))
				{
					PSWAW = (int)((1.33) * PSWAH);  // If the width/height ratio goes beyond 1.5,
					// the (longer) effective width is made shorter.
				}

			}

			float dpiX = GetDPIX(); // Get the horizontal DPI value.

			if (frm.bInitialScale == true) // If an initial scale (from scratch)
			{
				if (Symbol.Win32.PlatformType.IndexOf("PocketPC") != -1) // If the platform is either Pocket PC or Windows Mobile
				{
					frm.Width = PSWAW;  // Set the form width. However this setting
					// would be the ultimate one for Pocket PC (& Windows Mobile)devices.
					// Just for the sake of consistency, it's explicitely specified here.
				}
				else
				{
					frm.Width = (int)((frm.Width) * (PSWAW)) / (frm.resWidthReference); // Set the form width for others (Windows CE devices).

				}
			}
			if ((frm.Width <= maxLength * dpiX) || frm.bSkipMaxLen == true) // The calculation of the width & left values for each control
				// without taking the maximum length restriction into consideration.
			{
				foreach (System.Windows.Forms.Control cntrl in frm.Controls)
				{
					cntrl.Width = ((cntrl.Width) * (frm.Width)) / (frm.resWidthReference);
					cntrl.Left = ((cntrl.Left) * (frm.Width)) / (frm.resWidthReference);

					if (cntrl is System.Windows.Forms.TabControl)
					{
						foreach (System.Windows.Forms.TabPage tabPg in cntrl.Controls)
						{
							foreach (System.Windows.Forms.Control cntrl2 in tabPg.Controls)
							{
								cntrl2.Width = (((cntrl2.Width) * (frm.Width)) / (frm.resWidthReference));
								cntrl2.Left = (((cntrl2.Left) * (frm.Width)) / (frm.resWidthReference));
							}
						}
					}
				}

			}
			else
			{   // The calculation of the width & left values for each control
				// with the maximum length restriction taken into consideration.
				foreach (System.Windows.Forms.Control cntrl in frm.Controls)
				{
					cntrl.Width = (int)(((cntrl.Width) * (PSWAW) * (maxLength * dpiX)) / (frm.resWidthReference * (frm.Width)));
					cntrl.Left = (int)(((cntrl.Left) * (PSWAW) * (maxLength * dpiX)) / (frm.resWidthReference * (frm.Width)));

					if (cntrl is System.Windows.Forms.TabControl)
					{
						foreach (System.Windows.Forms.TabPage tabPg in cntrl.Controls)
						{
							foreach (System.Windows.Forms.Control cntrl2 in tabPg.Controls)
							{
								cntrl2.Width = (int)(((cntrl2.Width) * (PSWAW) * (maxLength * dpiX)) / (frm.resWidthReference * (frm.Width)));
								cntrl2.Left = (int)(((cntrl2.Left) * (PSWAW) * (maxLength * dpiX)) / (frm.resWidthReference * (frm.Width)));
							}
						}
					}
				}

				frm.Width = (int)((frm.Width) * (maxLength * dpiX)) / (frm.Width);

			}

			frm.resWidthReference = frm.Width; // Set the reference width to the new value.


			// A similar calculation is performed below for the height & top values for each control ...

			if (!((Screen.PrimaryScreen.Bounds.Width <= (1.5) * (Screen.PrimaryScreen.Bounds.Height))
				&& (Screen.PrimaryScreen.Bounds.Height <= (1.5) * (Screen.PrimaryScreen.Bounds.Width))))
			{
				if ((Screen.PrimaryScreen.Bounds.Height) > (Screen.PrimaryScreen.Bounds.Width))
				{
					PSWAH = (int)((1.33) * PSWAW);
				}

			}

			float dpiY = GetDPIY();

			if (frm.bInitialScale == true)
			{
				if (Symbol.Win32.PlatformType.IndexOf("PocketPC") != -1)
				{
					frm.Height = PSWAH;
				}
				else
				{
					frm.Height = (int)((frm.Height) * (PSWAH)) / (frm.resHeightReference);

				}
			}

			if ((frm.Height <= maxLength * dpiY) || frm.bSkipMaxLen == true)
			{
				foreach (System.Windows.Forms.Control cntrl in frm.Controls)
				{

					cntrl.Height = ((cntrl.Height) * (frm.Height)) / (frm.resHeightReference);
					cntrl.Top = ((cntrl.Top) * (frm.Height)) / (frm.resHeightReference);


					if (cntrl is System.Windows.Forms.TabControl)
					{
						foreach (System.Windows.Forms.TabPage tabPg in cntrl.Controls)
						{
							foreach (System.Windows.Forms.Control cntrl2 in tabPg.Controls)
							{
								cntrl2.Height = ((cntrl2.Height) * (frm.Height)) / (frm.resHeightReference);
								cntrl2.Top = ((cntrl2.Top) * (frm.Height)) / (frm.resHeightReference);
							}
						}
					}

				}

			}
			else
			{
				foreach (System.Windows.Forms.Control cntrl in frm.Controls)
				{

					cntrl.Height = (int)(((cntrl.Height) * (PSWAH) * (maxLength * dpiY)) / (frm.resHeightReference * (frm.Height)));
					cntrl.Top = (int)(((cntrl.Top) * (PSWAH) * (maxLength * dpiY)) / (frm.resHeightReference * (frm.Height)));


					if (cntrl is System.Windows.Forms.TabControl)
					{
						foreach (System.Windows.Forms.TabPage tabPg in cntrl.Controls)
						{
							foreach (System.Windows.Forms.Control cntrl2 in tabPg.Controls)
							{
								cntrl2.Height = (int)(((cntrl2.Height) * (PSWAH) * (maxLength * dpiY)) / (frm.resHeightReference * (frm.Height)));
								cntrl2.Top = (int)(((cntrl2.Top) * (PSWAH) * (maxLength * dpiY)) / (frm.resHeightReference * (frm.Height)));
							}
						}
					}

				}

				frm.Height = (int)((frm.Height) * (maxLength * dpiY)) / (frm.Height);

			}

			frm.resHeightReference = frm.Height;

			if (frm.bInitialScale == true)
			{
				frm.bInitialScale = false; // If this was the initial scaling (from scratch), it's now complete.
			}
			if (frm.bSkipMaxLen == true)
			{
				frm.bSkipMaxLen = false; // No need to consider the maximum length restriction now.
			}


		}

		
		// DPI values have to be taken via PInvokes on VS2003/CF1.0. The 2 proerties, System.Drawing.Graphics.DpiX
		// & System.Drawing.Graphics.DpiY, are not supported by VS2003/CF1.0.

		// For DpiX
		private static float GetDPIX()
		{
			IntPtr hwnd = (IntPtr)(GetCapture()); 
 
			float dpiX = GetDeviceCaps(hwnd, 88); // 88 for DPIX value

			return dpiX;
            
		}

		// For DpiY
		private static float GetDPIY()
		{
			IntPtr hwnd = (IntPtr)(GetCapture()); 
 
			float dpiY = GetDeviceCaps(hwnd, 90); // 90 for DPIY value

			return dpiY;

		}


		/// <summary>
		/// The static Select method is the recommended way to create the SelectDevice
		/// dialog.
		/// </summary>
		/// <remarks>
		/// This method will display the SelectDevice dialog and block until a 
		/// selection has been made.
		/// </remarks>
		/// <param name="Title">A string that will be displayed as the title to the
		/// SelectDevice dialog.</param>
		/// <param name="AvailableDevices">An array of available Symbol.Device objects.
		/// </param>
		/// <returns>The selected device object.</returns>
		public static Symbol.Barcode2.Device Select(
            string Title, Symbol.Barcode2.Device[] AvailableDevices)
		{
			OurAvailableDevices = AvailableDevices;

			if ( OurAvailableDevices.Length == 0 )
			{
				return null;
			}

			if ( OurAvailableDevices.Length == 1 )
			{
				return OurAvailableDevices[0];
			}

			OurTitle = Title;
			DefaultIndex = 0;

			int nSelection = new SelectDevice().Selection;

			if ( nSelection < 0 )
			{
				return null;
			}

			return OurAvailableDevices[nSelection];
		}

		/// <summary>
		/// The static Select method is the recommended way to create the SelectDevice
		/// dialog.
		/// </summary>
		/// <remarks>
		/// This method will display the SelectDevice dialog and block until a 
		/// selection has been made.
		/// </remarks>
		/// <param name="Title">A string that will be displayed as the title to the
		/// SelectDevice dialog.</param>
		/// <param name="AvailableDevices">An array of available Symbol.Device objects.
		/// </param>
		/// <param name="SelectIndex">The index of the initially selected device
		/// object.</param>
		/// <returns>The selected device object.</returns>
        public static Symbol.Barcode2.Device Select(string Title,
            Symbol.Barcode2.Device[] AvailableDevices, int SelectIndex)
		{
			OurAvailableDevices = AvailableDevices;

			if ( OurAvailableDevices.Length == 0 )
			{
				return null;
			}

			if ( OurAvailableDevices.Length == 1 )
			{
				return OurAvailableDevices[0];
			}

			OurTitle = Title;
			DefaultIndex = SelectIndex;

			int nSelection = new SelectDevice().Selection;

			if ( nSelection < 0 )
			{
				return null;
			}

			return OurAvailableDevices[nSelection];
		}

		/// <summary>
		/// Default SelectDevice constructor.
		/// </summary>
		internal SelectDevice()
		{
			InitializeComponent();
			this.DoScale();
			this.OKButton.Focus();
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CancelButton = new System.Windows.Forms.Button();
			this.OKButton = new System.Windows.Forms.Button();
			this.AvailableDevicesListBox = new System.Windows.Forms.ListBox();
			this.AvailableDevicesLabel = new System.Windows.Forms.Label();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			// 
			// CancelButton
			// 
			this.CancelButton.Location = new System.Drawing.Point(8, 232);
			this.CancelButton.Size = new System.Drawing.Size(64, 25);
			this.CancelButton.Text = "Cancel";
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			this.CancelButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CancelButton_KeyDown);
			// 
			// OKButton
			// 
			this.OKButton.Location = new System.Drawing.Point(168, 232);
			this.OKButton.Size = new System.Drawing.Size(64, 25);
			this.OKButton.Text = "OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			this.OKButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OKButton_KeyDown);
			// 
			// AvailableDevicesListBox
			// 
			this.AvailableDevicesListBox.Location = new System.Drawing.Point(8, 40);
			this.AvailableDevicesListBox.Size = new System.Drawing.Size(224, 170);
			// 
			// AvailableDevicesLabel
			// 
			this.AvailableDevicesLabel.Location = new System.Drawing.Point(8, 8);
			this.AvailableDevicesLabel.Size = new System.Drawing.Size(224, 24);
			this.AvailableDevicesLabel.Text = "Available Devices:";
			// 
			// SelectDevice
			// 
			this.ClientSize = new System.Drawing.Size(242, 264);
			this.Controls.Add(this.AvailableDevicesLabel);
			this.Controls.Add(this.AvailableDevicesListBox);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OKButton);
			this.Menu = this.mainMenu1;
			this.Text = "Select";
			this.Resize += new System.EventHandler(this.SelectDevice_Resize);
			this.Load += new System.EventHandler(this.SelectDevice_Load);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SelectDevice_KeyUp);

		}
		#endregion


		// Private: SelectDevice_Load
		private void SelectDevice_Load(object sender, System.EventArgs e)
		{
            //// Set wait cursor.
            //bool SaveWaitCursor = Symbol.Win32.WaitCursor;
            //Symbol.Win32.WaitCursor = true;

			try
			{

				this.AvailableDevicesListBox.Hide();

                foreach (Symbol.Barcode2.Device d in OurAvailableDevices)
                {

                    if (d.DeviceType != Symbol.Barcode2.DEVICETYPES.UNKNOWN)
                    {
                        this.AvailableDevicesListBox.Items.Add(d.DeviceType + " - " + d.FriendlyName);
                    }
                    else
                    {
                        this.AvailableDevicesListBox.Items.Add(d.FriendlyName);
                    }
                }

				this.AvailableDevicesListBox.SelectedIndex = 0;
				this.AvailableDevicesListBox.Show();
				this.KeyDown += new KeyEventHandler(SelectDevice_KeyDown);

				// Add MainMenu if Pocket PC
				if (Symbol.Win32.PlatformType.IndexOf("PocketPC") != -1)
				{
					this.Menu = new MainMenu();
				}

			}
			finally
			{
				// restore wait cursor
				//Symbol.Win32.WaitCursor = SaveWaitCursor;
			}
		}

		// Private: Selection
		private int Selection
		{
			get
			{
				this.AvailableDevicesLabel.Text = "Select " + OurTitle + ":";
				this.ShowDialog();
				return MySelection;
			}
		}

		// Private: OKButton_Click
		private void OKButton_Click(object sender, System.EventArgs e)
		{
			MySelection = AvailableDevicesListBox.SelectedIndex;
			
			this.Close();
		}

		// Private: CancelButton_Click
		private void CancelButton_Click(object sender, System.EventArgs e)
		{
			MySelection = -1;
			this.Close();
		}

		private void OKButton_KeyDown(object sender, KeyEventArgs e)
		{
			// Checks if the key pressed was an enter button (character code 13)
			if (e.KeyValue == (char)13)
				OKButton_Click(this, e);
		}

		private void CancelButton_KeyDown(object sender, KeyEventArgs e)
		{
			// Checks if the key pressed was an enter button (character code 13)
			if (e.KeyValue == (char)13)
				CancelButton_Click(this, e);
		}

		private void SelectDevice_KeyUp(object sender, KeyEventArgs e)
		{
			this.OKButton.Focus();
		}

		// Private: SelectDevice_KeyDown
		private void SelectDevice_KeyDown(object sender,KeyEventArgs e)
		{
			switch(e.KeyCode)
			{
				case Keys.Enter:
					this.OKButton_Click(this,EventArgs.Empty);
					return;
				case Keys.Up:
					if ( this.AvailableDevicesListBox.SelectedIndex > 0 )
					{
						this.AvailableDevicesListBox.SelectedIndex--;
					}
					return;
				case Keys.Down:
					if ( this.AvailableDevicesListBox.SelectedIndex < this.AvailableDevicesListBox.Items.Count-1 )
					{
						this.AvailableDevicesListBox.SelectedIndex++;
					}
					return;
			}
		}

		private void SelectDevice_Resize(object sender, System.EventArgs e)
		{
			if (bInitialScale == true)
			{
				return; // Return if the initial scaling (from scratch)is not complete.
			}

			if (Screen.PrimaryScreen.Bounds.Width > Screen.PrimaryScreen.Bounds.Height) // If landscape orientation
			{
				if (bPortrait != false) // If an orientation change has occured to landscape
				{
					bPortrait = false; // Set the orientation flag accordingly.
					bInitialScale = true; // An initial scaling is required due to orientation change.
					Scale(this); // Scale the GUI.
				}
				else
				{   // No orientation change has occured
					bSkipMaxLen = true; // Initial scaling is now complete, so skipping the max. length restriction is now possible.
					Scale(this); // Scale the GUI.
				}
			}
			else
			{
				// Similarly for the portrait orientation...
				if (bPortrait != true)
				{
					bPortrait = true;
					bInitialScale = true;
					Scale(this);
				}
				else
				{
					bSkipMaxLen = true;
					Scale(this);
				}
			}
		}
					
		[DllImport("coredll.dll")]  
		internal static extern Int32 GetDeviceCaps(IntPtr hdc, Int32 index);
	
		[DllImport("coredll.dll")]
		internal static extern Int32 GetCapture();
	}
}
