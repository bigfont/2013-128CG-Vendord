//-----------------------------------------------------------------------------------
// FILENAME: MainForm.cs
//
// Copyright © 2011 Motorola Solutions, Inc. All rights reserved.
//
// DESCRIPTION: The source file for the MainForm.
//
// ----------------------------------------------------------------------------------
//  
//	This sample demonstrates the usage of the EMDK for .NET API Symbol.Barcode 
//   in order to access the functionality of the barcode scanner. Please note the 
//   fact that this sample covers only the most basic operations associated with 
//   the barcode scanner, so illustrates the usage of only a subset of the complete 
//   API available.
//	
// ----------------------------------------------------------------------------------
// 
//-----------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Data;
using Symbol.Exceptions;
using System.IO;
using System.Runtime.InteropServices;
using Symbol.Barcode2;

namespace CS_Barcode2Sample1
{
    /// <summary>
    /// Class MainForm.
    /// </summary>
    public class MainForm : System.Windows.Forms.Form
    {
        // The column headers for the list view.
        private ColumnHeader numberColumn = null;
        private ColumnHeader itemColumn = null;
        private ColumnHeader nameColumn = null;
        private ColumnHeader dataColumn = null;
        private ColumnHeader optionsColumn = null;
        private ColumnHeader optionColumn = null;
        private ColumnHeader code128Column = null;
        private ColumnHeader code39Column = null;
        private ColumnHeader valueColumn = null;

        // The ImageList reference which is used to control the row height of listViewMain.
        //  This is kind of a workaround in the absense of an exposed API to control the 
        //  row height of System.Windows.Forms.ListView.
        private ImageList imageList = null;

        private ListView listViewMain = null;
        private Panel panel;

        private Resources myResources = null;
        private StatusBar statusBar = null;

        private Timer timer = null;
        private int selectedIndex = 0;

        System.EventHandler myActivateHandler = null;

        // The formatting string used with listview items.
        string itemNumberFormat = "";
        // Tracks the current view/page of the listViewMain.
        string currentListView = "";

        // The constants defined for the string table entries.
        const string ROOT = "Main";
        const string EXITAPP = "ExitApp";
        const string BACK = "Back";
        const string SCAN = "Scan";
        const string SCAN_CONTINUOUS = "ScanContinuous";
        const string OPTIONS = "Options";
        const string ABOUT = "About";
        const string AIMTYPE = "AimType";
        const string AIMMODE = "AimMode";
        const string SCANTYPE = "ScanType";
        const string CODE128 = "Code128";
        const string CODE39 = "Code39";
        const string AIMTYPE_TRIGGER = "Trigger";
        const string AIMTYPE_HOLD = "Hold";
        const string AIMTYPE_RELEASE = "Release";
        const string AIMMODE_NONE = "None";
        const string AIMMODE_DOT = "Dot";
        const string AIMMODE_SLAB = "Slab";
        const string AIMMODE_RETICLE = "Reticle";
        const string SCANTYPE_FOREGROUND = "Foreground";
        const string SCANTYPE_BACKGROUND = "Background";
        const string SCANTYPE_MONITOR = "Monitor";
        const string ENABLED = "Enabled";
        const string DISABLED = "Disabled";

        // All the barcode scanner - related operations in this sample would be carried out  
        //  by using this reference of myScanSampleAPI which is an instance of the class 
        //  ScanSampleAPI. Will be initialized later in the code.
        private API myScanSampleAPI = null;
        private Barcode2.OnScanHandler myScanNotifyHandler = null;
        private Barcode2.OnStatusHandler myStatusNotifyHandler = null;

        private EventHandler myFormActivatedEventHandler = null;
        private EventHandler myFormDeactivatedEventHandler = null;

        // The flag to track whether the Barcode object has been initialized or not.
        private bool isBarcodeInitiated = false;

        // The factor(n) which defines the row height of the ListView. 
        //  The default row height would be multiplied by this factor (ROW_HEIGHT_FACTOR = n).
        private const int ROW_HEIGHT_FACTOR = 2; // Currently set to 2. So the row height would be doubled in this sample.
                                                 // The user can set it to any value (i.e. 1,2,3,..., etc.) based on the requirements.

        public MainForm()
        {

            //Save the current cursor.
            Cursor savedCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            myResources = new Resources();
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
			
			if (this.listViewMain.Font.Size != 11)
            {
                this.listViewMain.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular);
            }

            this.itemColumn.Text = Resources.GetString("Item");
            this.valueColumn.Text = Resources.GetString("Value");
            this.timer.Interval = 1000;

            Cursor.Current = savedCursor;
        }
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listViewMain = new System.Windows.Forms.ListView();
            this.imageList = new System.Windows.Forms.ImageList();
            this.numberColumn = new System.Windows.Forms.ColumnHeader();
            this.itemColumn = new System.Windows.Forms.ColumnHeader();
            this.nameColumn = new System.Windows.Forms.ColumnHeader();
            this.dataColumn = new System.Windows.Forms.ColumnHeader();
            this.optionsColumn = new System.Windows.Forms.ColumnHeader();
            this.optionColumn = new System.Windows.Forms.ColumnHeader();
            this.code128Column = new System.Windows.Forms.ColumnHeader();
            this.code39Column = new System.Windows.Forms.ColumnHeader();
            this.valueColumn = new System.Windows.Forms.ColumnHeader();
            this.timer = new System.Windows.Forms.Timer();
            this.statusBar = new System.Windows.Forms.StatusBar();
            this.panel = new System.Windows.Forms.Panel();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewMain
            // 
            this.listViewMain.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listViewMain.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.listViewMain.FullRowSelect = true;
            this.listViewMain.Location = new System.Drawing.Point(0, 0);
            this.listViewMain.Name = "listViewMain";
            this.listViewMain.Size = new System.Drawing.Size(318, 268);
            this.listViewMain.TabIndex = 1;
            this.listViewMain.View = System.Windows.Forms.View.Details;
            this.listViewMain.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ListViewMain_KeyUp);
            this.listViewMain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListViewMain_KeyDown);
            // 
            // imageList
            // 
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            // 
            // numberColumn
            // 
            this.numberColumn.Text = "#";
            // 
            // itemColumn
            // 
            this.itemColumn.Text = "";
            // 
            // nameColumn
            // 
            this.nameColumn.Text = "ColumnHeader";
            // 
            // dataColumn
            // 
            this.dataColumn.Text = "";
            // 
            // optionsColumn
            // 
            this.optionsColumn.Text = "Options";
            // 
            // optionColumn
            // 
            this.optionColumn.Text = "Aim Type";
            // 
            // code128Column
            // 
            this.code128Column.Text = "Code128";
            // 
            // code39Column
            // 
            this.code39Column.Text = "Code39";
            // 
            // valueColumn
            // 
            this.valueColumn.Text = "Value";
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // statusBar
            // 
            this.statusBar.Location = new System.Drawing.Point(0, 268);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(318, 24);
            // 
            // panel
            // 
            this.panel.Controls.Add(this.listViewMain);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(318, 268);
            this.panel.Resize += new System.EventHandler(this.panel_Resize);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(318, 292);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.statusBar);
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "CS_Barcode2Sample1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            MainForm mainForm = new MainForm();
            Application.Run(mainForm);
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            this.myActivateHandler = new System.EventHandler(this.listViewMain_ItemActivate);
            this.listViewMain.ItemActivate += this.myActivateHandler;

            // Add MainMenu if Pocket PC
            if (Symbol.Win32.PlatformType.IndexOf("PocketPC") != -1)
            {
                this.Menu = new MainMenu();
            }

            // Initialize the ScanSampleAPI reference.
            this.myScanSampleAPI = new API();

            this.isBarcodeInitiated = this.myScanSampleAPI.InitBarcode();

            if (!(this.isBarcodeInitiated))// If the Barcode object has not been initialized
            {
                // Display a message & exit the application.
                MessageBox.Show(Resources.GetString("AppExitMsg"));
                Application.Exit();
            }
            else // If the Barcode object has been initialized
            {
                // Clear the statusbar where subsequent status information would be displayed.
                statusBar.Text = "";

                // Attach a scan notification handler.
                this.myScanNotifyHandler = new Barcode2.OnScanHandler(myBarcode2_ScanNotify);
                myScanSampleAPI.AttachScanNotify(myScanNotifyHandler);

                // Attach a status notification handler.
                this.myStatusNotifyHandler = new Barcode2.OnStatusHandler(myBarcode2_StatusNotify);
                myScanSampleAPI.AttachStatusNotify(myStatusNotifyHandler);
            }

            this.loadMainListView();

            setRowHeight(this.listViewMain);
            setGridLines(listViewMain);

            // Ensure that the keyboard focus is set on a control.
            this.listViewMain.Focus();

            myFormActivatedEventHandler = new EventHandler(MainForm_Activated);
            myFormDeactivatedEventHandler = new EventHandler(MainForm_Deactivate);
            this.Activated += myFormActivatedEventHandler;
            this.Deactivate += myFormDeactivatedEventHandler;
        }

        /// <summary>
        /// Add an item to listViewMain.
        /// </summary>
        /// <param name="number">The number to display in Number column.</param>
        /// <param name="itemName">The string to display in Item column.</param>
        private void addListViewItem(int number, string itemName)
        {
            string[] item;
            item = new string[] { number.ToString(itemNumberFormat), Resources.GetString(itemName), itemName };
            ListViewItem li = new ListViewItem(item);
            listViewMain.Items.Add(li);
        }

        /// <summary>
        /// Add an item to listViewMain.
        /// </summary>
        /// <param name="number">The number to display in Number column.</param>
        /// <param name="itemName">The string to display in Item column.</param>
        /// <param name="itemValue">The string to display in Value column.</param>
        private void addListViewItem(int number, string itemName, string itemValue)
        {
            string[] item;
            item = new string[] { number.ToString(itemNumberFormat), Resources.GetString(itemName), itemValue, itemName };
            ListViewItem li = new ListViewItem(item);
            listViewMain.Items.Add(li);
        }

        /// <summary>
        /// Append a data item to listViewMain. This is used in the page for the scanned data.
        /// Used in the scan mode.
        /// </summary>
        /// <param name="itemName">The string to display in data column.</param>
        private void appendListViewDataItem(string name, string value)
        {
            int number = listViewMain.Items.Count;
            string[] item;
            item = new string[] { number.ToString(itemNumberFormat), Resources.GetString(name), value, "" };
            ListViewItem li = new ListViewItem(item);
            listViewMain.Items.Add(li);
            li.Focused = true;
            listViewMain.EnsureVisible(number);

        }

        /// <summary>
        /// Append a data item to listViewMain. This is used in the page for the scanned data.
        /// Used in the scan continuous mode.
        /// </summary>
        /// <param name="itemName">The string to display in data column.</param>
        private void appendListViewContinuousDataItem(string data)
        {
            int number = listViewMain.Items.Count;
            string[] item;
            item = new string[] { number.ToString(itemNumberFormat), data, "" };
            ListViewItem li = new ListViewItem(item);
            listViewMain.Items.Add(li);
            li.Focused = true;
            listViewMain.EnsureVisible(number);
        }

        #region Main listView

        /// <summary>
        /// Add items to the start page of the Form; main list view.
        /// </summary>
        private void loadMainListViewItems()
        {
            itemNumberFormat = "";

            int i = 0;

            // Add items.
            addListViewItem(i++, EXITAPP);
            addListViewItem(i++, SCAN);
            addListViewItem(i++, SCAN_CONTINUOUS);
            addListViewItem(i++, OPTIONS);
            addListViewItem(i++, ABOUT);

        }

        /// <summary>
        /// Load the the start page of the Form; main list view.
        /// </summary>
        private void loadMainListView()
        {
            currentListView = ROOT;

            //Add column headers
            this.listViewMain.Columns.Add(this.numberColumn);
            this.listViewMain.Columns.Add(this.itemColumn);
            this.listViewMain.Columns.Add(this.nameColumn);
            this.loadMainListViewItems();

            setListViewColumnWidth();
            
        }

        /// <summary>
        /// Unload the main list view.
        /// </summary>
        private void unloadMainListView()
        {
            //Remove all items.
            this.listViewMain.Clear();
           
            //Remove column headers.
            this.listViewMain.Columns.Remove(this.numberColumn);
            this.listViewMain.Columns.Remove(this.itemColumn);
            this.listViewMain.Columns.Remove(this.nameColumn);

        }

        /// <summary>
        /// Add items to the page for the scanned data. 
        /// </summary>
        private void loadScanDataListViewItems()
        {
            itemNumberFormat = "0";

            int i = 0;
            addListViewItem(i++, BACK, "");
            addListViewItem(i++, "Data", "");
            addListViewItem(i++, "Type", "");
            addListViewItem(i++, "Source", "");
            addListViewItem(i++, "Time", "");
            addListViewItem(i++, "Length", "");
        }

        /// <summary>
        /// Load the page for the scanned data.
        /// </summary>
        private void loadScanDataListView()
        {
            currentListView = SCAN;

            //Add column headers
            this.listViewMain.Columns.Add(this.numberColumn);
            this.listViewMain.Columns.Add(this.dataColumn);
            this.listViewMain.Columns.Add(this.valueColumn);
            this.listViewMain.Columns.Add(this.nameColumn);
            this.loadScanDataListViewItems();

            //If the Barcode2 object has been initialized
            if (isBarcodeInitiated)
            {
                // Start a read operation & attach a handler.
                myScanSampleAPI.StartScan(false);
            }

            setListViewColumnWidth();
        }

        /// <summary>
        /// Unload the page for the scanned data.
        /// </summary>
        private void unloadScanDataListView()
        {
            // Stop the read operation & detach the handler.
            myScanSampleAPI.StopScan();

            //Remove all items
            this.listViewMain.Clear();
            //Remove column headers
            this.listViewMain.Columns.Remove(this.numberColumn);
            this.listViewMain.Columns.Remove(this.dataColumn);
            this.listViewMain.Columns.Remove(this.valueColumn);
            this.listViewMain.Columns.Remove(this.nameColumn);

            statusBar.Text = "";

        }

        /// <summary>
        /// Add items to the page for the scanned data. Used for the scan continuous mode.
        /// </summary>
        private void loadScanContinuousDataListViewItems()
        {
            itemNumberFormat = "00";

            int i = 0;
            addListViewItem(i++, BACK);

        }

        /// <summary>
        /// Load the page for the scanned data. Used for the scan continuous mode.
        /// </summary>
        private void loadScanContinuousDataListView()
        {
            currentListView = SCAN_CONTINUOUS;

            this.dataColumn.Text = "Data"; 

            //Add column headers
            this.listViewMain.Columns.Add(this.numberColumn);
            this.listViewMain.Columns.Add(this.dataColumn);
            this.listViewMain.Columns.Add(this.nameColumn);
           
            this.loadScanContinuousDataListViewItems();

            //If the Barcode2 object has been initialized
            if (isBarcodeInitiated)
            {
                // Start a read operation & attach a handler.
                myScanSampleAPI.StartScan(true);
            }

            setListViewColumnWidth();
        }

        /// <summary>
        /// Unload the page for the scanned data. Used for the scan continuous mode.
        /// </summary>

        private void unloadScanContinuousDataListView()
        {
            // Stop the read operation & detach the handler.
            myScanSampleAPI.StopScan();

            //Remove all items
            this.listViewMain.Clear();
            //Remove column headers
            this.listViewMain.Columns.Remove(this.numberColumn);
            this.listViewMain.Columns.Remove(this.dataColumn);
            this.listViewMain.Columns.Remove(this.nameColumn);

            this.dataColumn.Text = "";
            statusBar.Text = "";
 
        }

        /// <summary>
        /// Add items to the page for the options (the reader paramters, the scan parameters & the decoders).
        /// </summary>
        private void loadOptionsListViewItems()
        {
            itemNumberFormat = "";

            string aimType = "Unknown";
            string aimMode = "Unknown";
            string scanType = "Unknown";

            switch (myScanSampleAPI.Barcode2.Config.Reader.ReaderType)
            {
                case READER_TYPES.READER_TYPE_IMAGER:
                    aimType = myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.ImagerSpecific.AimType.ToString();
                    aimMode = myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.ImagerSpecific.AimMode.ToString();
                    break;
                case READER_TYPES.READER_TYPE_LASER:
                    aimType = myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.LaserSpecific.AimType.ToString();
                    aimMode = myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.LaserSpecific.AimMode.ToString();
                    break;
            }

            scanType = myScanSampleAPI.Barcode2.Config.Scanner.ScanType.ToString();

            int i = 0;
            addListViewItem(i++, BACK, "");
            addListViewItem(i++, AIMTYPE, aimType.Substring(9)); // Substring by drpping AIM_TYPE_.
            addListViewItem(i++, AIMMODE, aimMode.Substring(9)); // Substring by drpping AIM_MODE_.
            addListViewItem(i++, SCANTYPE, scanType);

            addListViewItem(i++, CODE128, (myScanSampleAPI.Barcode2.Config.Decoders.CODE128.Enabled== true) ? ENABLED : DISABLED );
            addListViewItem(i++, CODE39, (myScanSampleAPI.Barcode2.Config.Decoders.CODE39.Enabled == true) ? ENABLED : DISABLED);
        }

        /// <summary>
        /// Load the page for the options (the reader paramters, the scan parameters & the decoders).
        /// </summary>
        private void loadOptionsListView()
        {
            currentListView = OPTIONS;

            //Add column headers
            this.listViewMain.Columns.Add(this.numberColumn);
            this.listViewMain.Columns.Add(this.optionsColumn);
            this.listViewMain.Columns.Add(this.valueColumn);
            this.listViewMain.Columns.Add(this.nameColumn);
            this.loadOptionsListViewItems();

            setListViewColumnWidth();
        }
        /// <summary>
        /// Unload the page for the options (the reader paramters, the scan parameters & the decoders).
        /// </summary>
        private void unloadOptionsListView()
        {
            //Remove all items
            this.listViewMain.Clear();
            //Remove column headers
            this.listViewMain.Columns.Remove(this.numberColumn);
            this.listViewMain.Columns.Remove(this.optionsColumn);
            this.listViewMain.Columns.Remove(this.valueColumn);
            this.listViewMain.Columns.Remove(this.nameColumn);

        }

        /// <summary>
        /// Add items to the page for the option (reader parameter) Aim Type.
        /// </summary>
        private void loadOptionAimTypeListViewItems()
        {
            itemNumberFormat = "";

            int i = 0;

            addListViewItem(i++, BACK);
            addListViewItem(i++, AIMTYPE_TRIGGER);
            addListViewItem(i++, AIMTYPE_HOLD);
            addListViewItem(i++, AIMTYPE_RELEASE);

        }

        /// <summary>
        /// Load the page for the option (reader parameter) Aim Type.
        /// </summary>
        private void loadOptionAimTypeListView()
        {
            currentListView = AIMTYPE;

            //Add column headers
            this.listViewMain.Columns.Add(this.numberColumn);
            this.listViewMain.Columns.Add(this.optionColumn);
            this.listViewMain.Columns.Add(this.nameColumn);
            this.loadOptionAimTypeListViewItems();

            setListViewColumnWidth();
        }
        /// <summary>
        /// Unload the page for the option (reader parameter) Aim Type.
        /// </summary>
        private void unloadOptionAimTypeListView()
        {
            //Remove all items
            this.listViewMain.Clear();
            //Remove column headers
            this.listViewMain.Columns.Remove(this.numberColumn);
            this.listViewMain.Columns.Remove(this.optionColumn);
            this.listViewMain.Columns.Remove(this.nameColumn);

        }

        /// <summary>
        /// Add items to the page for the option (reader parameter) Aim Mode.
        /// </summary>
        private void loadOptionAimModeListViewItems()
        {
            itemNumberFormat = "";

            int i = 0;

            addListViewItem(i++, BACK);
            addListViewItem(i++, AIMMODE_NONE);
            addListViewItem(i++, AIMMODE_DOT);
            addListViewItem(i++, AIMMODE_SLAB);
            addListViewItem(i++, AIMMODE_RETICLE);

        }

        /// <summary>
        /// Load the page for the option (reader parameter) Aim Mode.
        /// </summary>
        private void loadOptionAimModeListView()
        {
            currentListView = AIMMODE;

            //Add column headers
            this.listViewMain.Columns.Add(this.numberColumn);
            this.listViewMain.Columns.Add(this.optionColumn);
            this.listViewMain.Columns.Add(this.nameColumn);
            this.loadOptionAimModeListViewItems();

            setListViewColumnWidth();
        }
        /// <summary>
        /// Unload the page for the option (reader parameter) Aim Mode.
        /// </summary>
        private void unloadOptionAimModeListView()
        {
            //Remove all items
            this.listViewMain.Clear();
            //Remove column headers
            this.listViewMain.Columns.Remove(this.numberColumn);
            this.listViewMain.Columns.Remove(this.optionColumn);
            this.listViewMain.Columns.Remove(this.nameColumn);

        }

        /// <summary>
        /// Add items to the page for the option (scan parameter) ScanType.
        /// </summary>
        private void loadOptionScanTypeListViewItems()
        {
            itemNumberFormat = "";

            int i = 0;

            addListViewItem(i++, BACK);
            addListViewItem(i++, SCANTYPE_FOREGROUND);
            addListViewItem(i++, SCANTYPE_BACKGROUND);
            addListViewItem(i++, SCANTYPE_MONITOR);

        }

        /// <summary>
        /// Load the page for the option (scan parameter) ScanType.
        /// </summary>
        private void loadOptionScanTypeListView()
        {
            currentListView = SCANTYPE;

            //Add column headers
            this.listViewMain.Columns.Add(this.numberColumn);
            this.listViewMain.Columns.Add(this.optionColumn);
            this.listViewMain.Columns.Add(this.nameColumn);
            this.loadOptionScanTypeListViewItems();

            setListViewColumnWidth();
        }
        /// <summary>
        /// Unload the page for the option (scan parameter) ScanType.
        /// </summary>
        private void unloadOptionScanTypeListView()
        {
            //Remove all items
            this.listViewMain.Clear();
            //Remove column headers
            this.listViewMain.Columns.Remove(this.numberColumn);
            this.listViewMain.Columns.Remove(this.optionColumn);
            this.listViewMain.Columns.Remove(this.nameColumn);

        }

        /// <summary>
        /// Add items to the page for the option (decoder) Code128.
        /// </summary>
        private void loadOptionCode128ListViewItems()
        {
            currentListView = CODE128;

            //Add column headers
            this.listViewMain.Columns.Add(this.numberColumn);
            this.listViewMain.Columns.Add(this.code128Column);
            this.listViewMain.Columns.Add(this.nameColumn);

            setListViewColumnWidth();

            itemNumberFormat = "";

            int i = 0;

            addListViewItem(i++, BACK);
            addListViewItem(i++, ENABLED);
            addListViewItem(i++, DISABLED);
        }

        /// <summary>
        /// Unload the page for the option (decoder) Code128.
        /// </summary>
        private void unloadOptionCode128ListView()
        {
            //Remove all items
            this.listViewMain.Clear();
            //Remove column headers
            this.listViewMain.Columns.Remove(this.numberColumn);
            this.listViewMain.Columns.Remove(this.code128Column);
            this.listViewMain.Columns.Remove(this.nameColumn);

        }


        /// <summary>
        /// Add items to the page for the option (decoder) Code39.
        /// </summary>
        private void loadOptionCode39ListViewItems()
        {
            currentListView = CODE39;

            //Add column headers
            this.listViewMain.Columns.Add(this.numberColumn);
            this.listViewMain.Columns.Add(this.code39Column);
            this.listViewMain.Columns.Add(this.nameColumn);

            setListViewColumnWidth();

            itemNumberFormat = "";

            int i = 0;

            addListViewItem(i++, BACK);
            addListViewItem(i++, ENABLED);
            addListViewItem(i++, DISABLED);
        }

        /// <summary>
        /// Unload the page for the option (decoder) Code39.
        /// </summary>
        private void unloadOptionCode39ListView()
        {
            //Remove all items
            this.listViewMain.Clear();
            //Remove column headers
            this.listViewMain.Columns.Remove(this.numberColumn);
            this.listViewMain.Columns.Remove(this.code39Column);
            this.listViewMain.Columns.Remove(this.nameColumn);

        }

        #endregion


        /// <summary>
        /// This deligate is called when a listview item is activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewMain_ItemActivate(object sender, System.EventArgs e)
        {
            Cursor savedCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                int nameColumn = locateNameColumn(currentListView);

                // Compare the 3rd element in the selected row in the listwiew.
                // This name column has been made invisible, but it's used to track the currently active/highlighted row.
                string s = listViewMain.Items[listViewMain.SelectedIndices[0]].SubItems[nameColumn].Text;
                switch (listViewMain.Items[listViewMain.SelectedIndices[0]].SubItems[nameColumn].Text)
                {
                    case EXITAPP:

                        this.Close();
                        break;

                    case SCAN:

                        unloadMainListView();
                        loadScanDataListView();
                        break;

                    case SCAN_CONTINUOUS:

                        unloadMainListView();
                        loadScanContinuousDataListView();
                        break;

                    case OPTIONS:

                        unloadMainListView();
                        loadOptionsListView();
                        break;

                    case AIMTYPE:

                        unloadOptionsListView();
                        loadOptionAimTypeListView();
                        break;

                    case AIMTYPE_TRIGGER:

                        switch (myScanSampleAPI.Barcode2.Config.Reader.ReaderType)
                        {
                            case READER_TYPES.READER_TYPE_IMAGER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.ImagerSpecific.AimType = AIM_TYPE.AIM_TYPE_TRIGGER;
                                break;
                            case READER_TYPES.READER_TYPE_LASER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.LaserSpecific.AimType = AIM_TYPE.AIM_TYPE_TRIGGER;
                                break;
                        }

                        myScanSampleAPI.Barcode2.Config.Reader.Set();

                        reloadLastListView();

                        break;

                    case AIMTYPE_HOLD:
                        
                        switch (myScanSampleAPI.Barcode2.Config.Reader.ReaderType)
                        {
                            case READER_TYPES.READER_TYPE_IMAGER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.ImagerSpecific.AimType = AIM_TYPE.AIM_TYPE_TIMED_HOLD;
                                break;
                            case READER_TYPES.READER_TYPE_LASER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.LaserSpecific.AimType = AIM_TYPE.AIM_TYPE_TIMED_HOLD;
                                break;
                        }

                        myScanSampleAPI.Barcode2.Config.Reader.Set();

                        reloadLastListView();

                        break;

                    case AIMTYPE_RELEASE:
                        
                        switch (myScanSampleAPI.Barcode2.Config.Reader.ReaderType)
                        {
                            case READER_TYPES.READER_TYPE_IMAGER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.ImagerSpecific.AimType = AIM_TYPE.AIM_TYPE_TIMED_RELEASE;
                                break;
                            case READER_TYPES.READER_TYPE_LASER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.LaserSpecific.AimType = AIM_TYPE.AIM_TYPE_TIMED_RELEASE;
                                break;
                        }

                        myScanSampleAPI.Barcode2.Config.Reader.Set();

                        reloadLastListView();

                        break;

                    case AIMMODE:

                        unloadOptionsListView();
                        loadOptionAimModeListView();
                        break;

                    case AIMMODE_NONE:

                        switch (myScanSampleAPI.Barcode2.Config.Reader.ReaderType)
                        {
                            case READER_TYPES.READER_TYPE_IMAGER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.ImagerSpecific.AimMode = AIM_MODE.AIM_MODE_NONE;
                                break;
                            case READER_TYPES.READER_TYPE_LASER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.LaserSpecific.AimMode = AIM_MODE.AIM_MODE_NONE;
                                break;
                        }

                        myScanSampleAPI.Barcode2.Config.Reader.Set();

                        reloadLastListView();

                        break;

                    case AIMMODE_DOT:

                        switch (myScanSampleAPI.Barcode2.Config.Reader.ReaderType)
                        {
                            case READER_TYPES.READER_TYPE_IMAGER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.ImagerSpecific.AimMode = AIM_MODE.AIM_MODE_DOT;
                                break;
                            case READER_TYPES.READER_TYPE_LASER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.LaserSpecific.AimMode = AIM_MODE.AIM_MODE_DOT;
                                break;
                        }

                        myScanSampleAPI.Barcode2.Config.Reader.Set();

                        reloadLastListView();

                        break;

                    case AIMMODE_SLAB:

                        switch (myScanSampleAPI.Barcode2.Config.Reader.ReaderType)
                        {
                            case READER_TYPES.READER_TYPE_IMAGER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.ImagerSpecific.AimMode = AIM_MODE.AIM_MODE_SLAB;
                                break;
                            case READER_TYPES.READER_TYPE_LASER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.LaserSpecific.AimMode = AIM_MODE.AIM_MODE_SLAB;
                                break;
                        }

                        myScanSampleAPI.Barcode2.Config.Reader.Set();

                        reloadLastListView();

                        break;

                    case AIMMODE_RETICLE:

                        switch (myScanSampleAPI.Barcode2.Config.Reader.ReaderType)
                        {
                            case READER_TYPES.READER_TYPE_IMAGER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.ImagerSpecific.AimMode = AIM_MODE.AIM_MODE_RETICLE;
                                break;
                            case READER_TYPES.READER_TYPE_LASER:
                                myScanSampleAPI.Barcode2.Config.Reader.ReaderSpecific.LaserSpecific.AimMode = AIM_MODE.AIM_MODE_RETICLE;
                                break;
                        }

                        myScanSampleAPI.Barcode2.Config.Reader.Set();

                        reloadLastListView();

                        break;

                    case SCANTYPE:

                        unloadOptionsListView();
                        loadOptionScanTypeListView();
                        break;

                    case SCANTYPE_FOREGROUND:

                        myScanSampleAPI.Barcode2.Config.Scanner.ScanType = SCANTYPES.Foreground;
                        myScanSampleAPI.Barcode2.Config.Scanner.Set();

                        reloadLastListView();

                        break;

                    case SCANTYPE_BACKGROUND:

                        myScanSampleAPI.Barcode2.Config.Scanner.ScanType = SCANTYPES.Background;
                        myScanSampleAPI.Barcode2.Config.Scanner.Set();

                        reloadLastListView();

                        break;

                    case SCANTYPE_MONITOR:

                        myScanSampleAPI.Barcode2.Config.Scanner.ScanType = SCANTYPES.Monitor;
                        myScanSampleAPI.Barcode2.Config.Scanner.Set();

                        reloadLastListView();

                        break;

                    case CODE128:

                        unloadOptionsListView();
                        loadOptionCode128ListViewItems();
                        break;

                    case ENABLED:

                        if (currentListView == CODE128)
                        {
                            myScanSampleAPI.Barcode2.Config.Decoders.CODE128.Enabled = true;
                            myScanSampleAPI.Barcode2.Config.Decoders.Set();
                        }
                        else
                        {
                            myScanSampleAPI.Barcode2.Config.Decoders.CODE39.Enabled = true;
                            myScanSampleAPI.Barcode2.Config.Decoders.Set();
                        }

                        reloadLastListView();
                        break;

                    case DISABLED:

                        if (currentListView == CODE128)
                        {
                            myScanSampleAPI.Barcode2.Config.Decoders.CODE128.Enabled = false;
                            myScanSampleAPI.Barcode2.Config.Decoders.Set();
                        }
                        else
                        {
                            myScanSampleAPI.Barcode2.Config.Decoders.CODE39.Enabled = false;
                            myScanSampleAPI.Barcode2.Config.Decoders.Set();
                        }

                        reloadLastListView();
                        break;

                    case CODE39:

                        unloadOptionsListView();
                        loadOptionCode39ListViewItems();
                        break;

                    case ABOUT:

                        AboutForm myAbout = new AboutForm();
                        Cursor.Current = savedCursor;
                        myAbout.ShowDialog();
                        Cursor.Current = Cursors.WaitCursor;
                        // Ensure that the keyboard focus is set on a control.
                        this.listViewMain.Focus();
                        break;

                    case BACK:
                        reloadLastListView();
                        break;

                    default:
                        break;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                System.Windows.Forms.MessageBox.Show(Resources.GetString("ItemNotSelected"), Resources.GetString("CS_Barcode2Sample1"));
                this.listViewMain.Focus();
            }
            Cursor.Current = savedCursor;
        }

        private int locateNameColumn(string currentListView)
        {
            switch (currentListView)
            {
                case ROOT:
                    return 2;
                case SCAN:
                    return 3;
                case SCAN_CONTINUOUS:
                    return 2;
                case OPTIONS:
                    return 3;
                case AIMTYPE:
                case AIMMODE:
                case SCANTYPE:
                case CODE128:
                case CODE39:
                    return 2;
                default:
                    return -1; // Error ... Shouldn't be reached.
            }
        }

        /// <summary>
        /// Reloads the previous listView.
        /// </summary>
        private void reloadLastListView()
        {
            switch (currentListView)
            {
                case SCAN:
                    unloadScanDataListView();
                    break;
                case SCAN_CONTINUOUS:
                    unloadScanContinuousDataListView();
                    break;
                case OPTIONS:
                    unloadOptionsListView();
                    break;
                case AIMTYPE:
                    unloadOptionAimTypeListView();
                    break;
                case AIMMODE:
                    unloadOptionAimModeListView();
                    break;
                case SCANTYPE:
                    unloadOptionScanTypeListView();
                    break;
                case CODE128:
                    unloadOptionCode128ListView();
                    break;
                case CODE39:
                    unloadOptionCode39ListView();
                    break;
                default:
                    return;
            }

            // Only in the case of current page being one for any option
            if (currentListView == AIMTYPE || currentListView == AIMMODE || currentListView == SCANTYPE || currentListView == CODE128 || currentListView == CODE39)
            {
                // Load the page for the options.
                loadOptionsListView(); 
            }
            else // In all other cases
            {
                // Load the main page.
                loadMainListView();
            }
        }

        /// <summary>
        /// The handler for the KeyUp event.
        /// </summary>
        private void ListViewMain_KeyUp(object sender, KeyEventArgs e)
        {
            this.listViewMain.Focus();
        }

        /// <summary>
        /// The handler for the KeyDown event.
        /// Handle keyboard navigation of the listview.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewMain_KeyDown(object sender, KeyEventArgs e)
        {
            //process softkeys first of all
            switch (e.KeyCode)
            {
                case Keys.Right:
                    this.Close();
                    return;
            }

            //Process rest of the keys
            char c = System.Convert.ToChar(e.KeyValue);
            int tmpIndex;

            if ((c >= '0') && (c <= '9'))
            {
                //A number is pressed
                if (timer.Enabled)
                {//This is the second number being pressed as a pair
                    //stop the timer after the second digit
                    timer.Enabled = false;
                    tmpIndex = selectedIndex * 10 + (int)(c - '0');
                    if (listViewMain.Items.Count > tmpIndex)
                    {
                        selectedIndex = tmpIndex;
                        gotoListItem();
                    }
                    else
                    {
                        selectedIndex = 0;
                    }
                }
                else
                {//The first number being pressed
                    tmpIndex = (int)(c - '0');
                    if (listViewMain.Items.Count > tmpIndex)
                    {
                        selectedIndex = tmpIndex;

                        if (listViewMain.Items[0].SubItems[0].Text.Length < 2)
                        {
                            //list view is one digit so process here
                            gotoListItem();
                            //reset selected index for the next cycle
                            selectedIndex = 0;

                        }
                        else
                        {
                            //list view has more than 10 items so wait for the next cycle or tick
                            //start timer
                            timer.Enabled = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stop the timer and reset index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            //stop timer after one tic
            timer.Enabled = false;
            //reset selected index for the next cycle
            selectedIndex = 0;
        }

        /// <summary>
        /// Go to the selected item and expand it if possible
        /// </summary>
        private void gotoListItem()
        {
            if (selectedIndex <= listViewMain.Items.Count)
            {
                //remove all previous selections
                for (int i = 0; i < listViewMain.Items.Count; i++)
                {
                    listViewMain.Items[i].Selected = false;
                }

                //select the desired item
                listViewMain.Items[selectedIndex].Selected = true;
                listViewMain.Invoke(this.myActivateHandler);
            }
        }

        /// <summary>
        /// The handler called when resizing MainForm. The UI is re-calculated
        /// and adjusted based on the dimentions of the screen.
        /// </summary>
        private void MainForm_Resize(object sender, System.EventArgs e)
        {
            // If it is CE
            if (Symbol.Win32.PlatformType.IndexOf("PocketPC", 0) <= 0)
            {
                this.Width = (Screen.PrimaryScreen.WorkingArea.Width > 320 ? 320 : Screen.PrimaryScreen.WorkingArea.Width);
                this.Height = (Screen.PrimaryScreen.WorkingArea.Height > 320 ? 320 : Screen.PrimaryScreen.WorkingArea.Height);
            }

        }

        /// <summary>
        /// Adjust the listViewMain dimensions, mainly the column widths.
        /// </summary>
        private void setListViewColumnWidth()
        {
            listViewMain.Width = this.Width;
            listViewMain.Height = this.Height;

            switch (currentListView)
            {
                case ROOT:
                    this.numberColumn.Width = (10 * listViewMain.Width) / 100;
                    this.itemColumn.Width = (90 * listViewMain.Width) / 100;
                    break;
                case SCAN:
                    this.numberColumn.Width = (10 * listViewMain.Width) / 100;
                    this.dataColumn.Width = (30 * listViewMain.Width) / 100;
                    this.valueColumn.Width = (60 * listViewMain.Width) / 100;
                    break;
                case SCAN_CONTINUOUS:
                    this.numberColumn.Width = (10 * listViewMain.Width) / 100;
                    this.dataColumn.Width = (90 * listViewMain.Width) / 100;
                    break;
                case OPTIONS:
                    this.numberColumn.Width = (10 * listViewMain.Width) / 100;
                    this.optionsColumn.Width = (50 * listViewMain.Width) / 100;
                    this.valueColumn.Width = (40 * listViewMain.Width) / 100;
                    break;
                case AIMTYPE:
                case AIMMODE:
                case CODE128:
                case CODE39:
                    this.numberColumn.Width = (10 * listViewMain.Width) / 100;
                    this.optionColumn.Width = (90 * listViewMain.Width) / 100;
                    break;
                default:
                    break;

            }

             this.nameColumn.Width = 0;
          
        }

        private void panel_Resize(object sender, EventArgs e)
        {
            setListViewColumnWidth();
        }

        /// <summary>
        /// Read notification handler.
        /// </summary>
        private void myBarcode2_ScanNotify(ScanDataCollection scanDataCollection)
        {
            // Get ScanData
            ScanData scanData = scanDataCollection.GetFirst;

            switch (scanData.Result)
            {
                case Results.SUCCESS:

                    // Handle the data from this read & submit the next read.

                    if (currentListView == SCAN_CONTINUOUS)
                    {
                        HandleContinuousData(scanData);
                        this.myScanSampleAPI.StartScan(true);
                    }
                    else if (currentListView == SCAN)
                    {
                        HandleData(scanData);
                        this.myScanSampleAPI.StartScan(false);
                    }

                    break;

                case Results.E_SCN_READTIMEOUT:

                    if (currentListView == SCAN_CONTINUOUS)
                        this.myScanSampleAPI.StartScan(true);
                    else
                        if (currentListView == SCAN)
                            this.myScanSampleAPI.StartScan(false);
                    break;

                case Results.CANCELED:

                    break;

                case Results.E_SCN_DEVICEFAILURE:

                    this.myScanSampleAPI.StopScan();

                    if (currentListView == SCAN_CONTINUOUS)
                        this.myScanSampleAPI.StartScan(true);
                    else
                        if (currentListView == SCAN)
                            this.myScanSampleAPI.StartScan(false);
                    break;

                default:

                    string sMsg = "Read Failed\n"
                        + "Result = "
                        + (scanData.Result).ToString();

                    appendListViewContinuousDataItem(sMsg);

                    if (scanData.Result == Results.E_SCN_READINCOMPATIBLE)
                    {
                        // If the failure is E_SCN_READINCOMPATIBLE, exit the application.

                        MessageBox.Show(Resources.GetString("AppExitMsg"), Resources.GetString("Failure"));

                        this.Close();
                        return;
                    }

                    break;
            }
        }

        /// <summary>
        /// Status notification handler.
        /// </summary>
        private void myBarcode2_StatusNotify(StatusData statusData)
        {
            switch (statusData.State)
            {
                case States.IDLE:

                    if (currentListView == SCAN)
                        statusBar.Text = "Press trigger to scan";
                    else if (currentListView == SCAN_CONTINUOUS)
                        statusBar.Text = "Aim at barcode to scan";
                    break;

                case States.READY:
                    if ((currentListView == SCAN) || (currentListView == SCAN_CONTINUOUS))
                        statusBar.Text = "Aim at barcode to scan";
                    break;

                default:
                    statusBar.Text = statusData.Text;
                    break;
            }
        }

        /// <summary>
        /// Handle data from the scan. Used in the scan mode.
        /// </summary>
        private void HandleData(ScanData scanData)
        {
            listViewMain.Items[1].SubItems[2].Text = scanData.Text;
            listViewMain.Items[2].SubItems[2].Text = scanData.Type.ToString();
            listViewMain.Items[3].SubItems[2].Text = scanData.Source;
            listViewMain.Items[4].SubItems[2].Text = scanData.TimeStamp.ToString();
            listViewMain.Items[5].SubItems[2].Text = scanData.Length.ToString();
            
        }

        /// <summary>
        /// Handle data from the scan. Used in the scan continuous mode.
        /// </summary>
        private void HandleContinuousData(ScanData scanData)
        {
            appendListViewContinuousDataItem(scanData.Text);
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            this.listViewMain.Focus();

            if (currentListView == SCAN_CONTINUOUS)
                this.myScanSampleAPI.StartScan(true);
            else if (currentListView == SCAN)
                    this.myScanSampleAPI.StartScan(false);
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            if ((currentListView == SCAN_CONTINUOUS) || (currentListView == SCAN))
            {
                myScanSampleAPI.StopScan();
            }
        }


        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Do not exit the app if not on the main screen. Simply return to the previous screen.
            if (currentListView != "Main")
            {
                e.Cancel = true;

                listViewMain.Focus();
                //ASSERT: Item[0] or the first row is always "Exit"
                listViewMain.Items[0].Selected = true;
                listViewMain.Invoke(this.myActivateHandler); 
            }
            if (isBarcodeInitiated)
            {
                myScanSampleAPI.TermBarcode();

                //Remove ListView event handlers
                this.listViewMain.ItemActivate -= myActivateHandler;

                //Remove Form event handlers
                this.Activated -= myFormActivatedEventHandler;
                this.Deactivate -= myFormDeactivatedEventHandler;
            }
        }

        private const int LVM_GETITEMPOSITION = (0x1010);
        private const int LVM_GETEXTENDEDLISTVIEWSTYLE = 0x1037;
        private const int LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1036;
        private const int LVS_EX_GRIDLINES = 0x1;

        public struct Point
        {
            public int x;
            public int y;

            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [DllImport("coredll.dll")]
        private static extern int SendMessageW(int hWnd, uint wMsg, int wParam, ref Point lParam);

        [DllImport("coredll.dll")]
        private static extern int SendMessageW(int hWnd, int wMsg, int wParam, int lParam);

        public void setRowHeight(System.Windows.Forms.ListView lvw)
        {
            Point positionItem1 = new Point(0, 0);
            Point positionItem2 = new Point(0, 0);

            SendMessageW((int)(lvw.Handle), LVM_GETITEMPOSITION, 0, ref positionItem1);

            SendMessageW((int)(lvw.Handle), LVM_GETITEMPOSITION, 1, ref positionItem2);

            int rowHeight = positionItem2.y - positionItem1.y;

            // Adjust the row height of listViewMain by multiplying the current factor by ROW_HEIGHT_FACTOR.
            //  The usage of this imageList is kind of a workaround in the absense of an exposed API to control the 
            //  row height of System.Windows.Forms.ListView.
            this.imageList.ImageSize = new Size(1, (int)(rowHeight * ROW_HEIGHT_FACTOR));
            lvw.SmallImageList = this.imageList;
        }

        public void setGridLines(System.Windows.Forms.ListView lvw)
        {
            int extendedStyle = SendMessageW((int)(lvw.Handle), LVM_GETEXTENDEDLISTVIEWSTYLE, 0, 0);
            extendedStyle |= LVS_EX_GRIDLINES;
            SendMessageW((int)(lvw.Handle), LVM_SETEXTENDEDLISTVIEWSTYLE, 0, extendedStyle);
        }

    }
}
