namespace Vendord.Desktop.App
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Collections.ObjectModel;
    using Vendord.SmartDevice.Shared;

    public class MainForm : Form
    {
        internal Panel mainNavigation;
        internal Panel mainContent;

        // see also http://msdn.microsoft.com/en-us/library/system.windows.forms.columnheader.width%28v=vs.90%29.aspx
        private int COLUMN_HEADER_WIDTH_HEADING_LENGTH = -2;  // To autosize to the width of the column heading, set the Width property to -2.  
        private int COLUMN_HEADER_WIDTH_LONGEST_ITEM = -1; // To adjust the width of the longest item in the column, set the Width property to -1. 
        private int COLUMN_HEADER_WIDTH_DEFAULT = 100;
        private int BUTTON_HEIGHT = 50;
        private int NUMBER_OF_NAV_BUTTONS = 2;

        private Button btnBack;
        private delegate void Back();
        private Back BackDelegate;
        private delegate void Save();
        private Save SaveDelegate;

        private static class UserInputs
        {
            internal const string LV_MASTER = "lvMaster";
            internal const string LV_DETAILS = "lvDetails";
        }

        // database
        private VendordDatabase _db;
        private VendordDatabase db
        {
            get
            {
                if (_db == null)
                {
                    _db = new VendordDatabase();
                }
                return _db;
            }
        }

        public MainForm()
        {
            Control[] controls;

            this.Load += new EventHandler(MainForm_Load);
            this.Closing += new CancelEventHandler(MainForm_Closing);
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;

            //
            // create main navigation panel
            //            
            mainNavigation = new Panel();
            mainNavigation.Dock = DockStyle.Top;
            mainNavigation.Height = BUTTON_HEIGHT * NUMBER_OF_NAV_BUTTONS;

            //
            // create main content panel
            //
            mainContent = new Panel();
            mainContent.Dock = DockStyle.Top;

            //
            // add to form - this triggers its layout event 
            //        
            this.SuspendLayout();
            controls = new Control[] { mainNavigation, mainContent }.Reverse().ToArray();
            foreach (Control c in controls)
            {
                this.Controls.Add(c);
            }
            this.ResumeLayout();

            //
            // Create Buttons
            //
            Button btnClose;

            btnBack = new Button() { Text = "Back" };
            btnBack.Click += new EventHandler(btnBack_Click);

            btnClose = new Button() { Text = "Close" };
            btnClose.Click += new EventHandler(btnClose_Click);

            //
            // add to panel - this triggers its layout event
            //
            this.mainNavigation.SuspendLayout();

            controls = new Control[] { 
            
                btnBack,
                btnClose
            
            }.Reverse().ToArray();

            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Top;
                c.Height = BUTTON_HEIGHT;
                this.mainNavigation.Controls.Add(c);
            }

            this.mainNavigation.ResumeLayout();
        }

        #region Utilities

        private void sync_pullProductsFromITRetailDatabase()
        {
            DatabaseSync sync;
            DatabaseSync.SyncResultMessage syncResult;

            sync = new DatabaseSync();
            syncResult = sync.PullProductsFromITRetailDatabase();            

            MessageBox.Show(syncResult.Caption, syncResult.Message, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void sync_mergeDesktopAndDeviceDatabases()
        {
            DatabaseSync sync;
            DatabaseSync.SyncResultMessage syncResult;

            sync = new DatabaseSync();            
            syncResult = sync.MergeDesktopAndDeviceDatabases();

            MessageBox.Show(syncResult.Caption, syncResult.Message, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void addDataToListViewDetails(ListView listViewDetails, int orderSessionID)
        {
            ListViewItem listViewItem;
            VendordDatabase.Product product;
            listViewDetails.Items.Clear();
            foreach (VendordDatabase.OrderSession_Product order_product in db.OrderSession_Products.Where(i => i.OrderSessionID == orderSessionID))
            {
                product = db.Products.First(p => p.ID == order_product.ProductID);

                listViewItem = new ListViewItem(product.Name);
                listViewItem.SubItems.Add(order_product.CasesToOrder.ToString());
                listViewDetails.Items.Add(listViewItem);
            }
        }

        private ListView createOrderSessionDetailsListView(int orderSessionID)
        {
            ListView listViewDetails;

            listViewDetails = new ListView()
            {

                Name = UserInputs.LV_DETAILS,
                View = View.Details,
                Activation = ItemActivation.OneClick,
                FullRowSelect = true

            };

            // columns are required in View.Details
            listViewDetails.Columns.Add("Product");
            listViewDetails.Columns.Add("Cases to Order");

            // return 
            return listViewDetails;
        }

        private void addDataToListViewMaster(ListView listViewMaster)
        {
            ListViewItem listViewItem;
            // add list view items            
            foreach (VendordDatabase.OrderSession orderSession in db.OrderSessions)
            {
                listViewItem = new ListViewItem(orderSession.Name);
                listViewItem.SubItems.Add(orderSession.ID.ToString());
                listViewMaster.Items.Add(listViewItem);
            }
        }

        private ListView createOrderSessionMasterListView()
        {
            ListView listViewMaster;

            listViewMaster = new ListView()
            {

                Name = UserInputs.LV_MASTER,
                View = View.Details,
                Activation = ItemActivation.OneClick,
                FullRowSelect = true

            };
            listViewMaster.ItemActivate += new EventHandler(listViewMaster_ItemActivate);

            // for prototyping            
            if (listViewMaster.View == View.Details || listViewMaster.View == View.Tile)
            {
                listViewMaster.Columns.Add("Order Name");
            }

            // add data
            addDataToListViewMaster(listViewMaster);

            // return
            return listViewMaster;
        }

        private void disableBackButton()
        {
            btnBack.Enabled = false;
            BackDelegate = null;
        }

        private void enableBackButton(Back method)
        {
            btnBack.Enabled = true;
            BackDelegate = method;
        }

        #endregion

        #region Views

        private void unloadCurrentView()
        {
            this.mainContent.Controls.Clear();
        }

        private void loadHomeView()
        {
            Button btnOrders;

            btnOrders = new Button() { Text = "Orders" };
            btnOrders.Click += new EventHandler(btnOrders_Click);

            btnOrders.Dock = DockStyle.Top;
            btnOrders.Height = BUTTON_HEIGHT;
            this.mainContent.Controls.Add(btnOrders);

            disableBackButton();
        }

        private void loadOrdersView()
        {
            Button btnSyncHandheld;
            Button btnCompleteOrder;
            Control[] controls;

            btnCompleteOrder = new Button() { Text = "Complete Order" };
            btnCompleteOrder.Click += new EventHandler(btnCompleteOrder_Click);

            btnSyncHandheld = new Button() { Text = "Sync with IT Retail" };
            btnSyncHandheld.Click += new EventHandler(btnSyncHandheldWithITRetail_Click);

            // add
            controls = new Control[] { 

                btnCompleteOrder, 
                btnSyncHandheld 

            }.Reverse().ToArray();

            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Top;
                c.Height = BUTTON_HEIGHT;
                this.mainContent.Controls.Add(c);
            }

            //
            // back
            // 
            enableBackButton(loadHomeView);
        }

        private void loadCompleteOrderView()
        {
            ListView listViewMaster;
            ListView listViewDetails;
            Control[] controls;
            int orderSessionID;

            listViewMaster = createOrderSessionMasterListView();
            orderSessionID = Convert.ToInt32(listViewMaster.Items[0].SubItems[1].Text);
            listViewDetails = createOrderSessionDetailsListView(orderSessionID);
            addDataToListViewDetails(listViewDetails, orderSessionID);

            this.mainContent.SuspendLayout();

            controls = new Control[] { 
            
                listViewMaster,
                listViewDetails
            
            }.Reverse().ToArray();

            foreach (ListView lv in controls)
            {
                lv.Dock = DockStyle.Left;
                lv.Width
                    = lv.Columns.Count
                    * COLUMN_HEADER_WIDTH_DEFAULT
                    + SystemInformation.VerticalScrollBarWidth
                    + SystemInformation.BorderSize.Width * 2;

                lv.BorderStyle = BorderStyle.FixedSingle;
                lv.GridLines = true;

                foreach (ColumnHeader h in lv.Columns)
                {
                    h.Width = COLUMN_HEADER_WIDTH_DEFAULT;
                }
                this.mainContent.Controls.Add(lv);
            }

            this.mainContent.ResumeLayout();

            //
            // back
            //
            enableBackButton(loadOrdersView);
        }

        private void updateListViewDetail()
        {
            ListView listViewMaster;
            ListView listViewDetails;
            int orderSessionID;

            listViewMaster = FormHelper.GetControlsByName<ListView>(this, UserInputs.LV_MASTER, true).First<ListView>();
            orderSessionID = Convert.ToInt32(listViewMaster.SelectedItems[0].SubItems[1].Text);

            listViewDetails = FormHelper.GetControlsByName<ListView>(this, UserInputs.LV_DETAILS, true).First<ListView>();
            addDataToListViewDetails(listViewDetails, orderSessionID);

            //
            // back
            //
            enableBackButton(loadOrdersView);
        }

        #endregion

        #region Events

        private void MainForm_Load(object sender, EventArgs e)
        {
            loadHomeView();
        }

        private void MainForm_Closing(object sender, EventArgs e)
        {
            if (SaveDelegate != null)
            {
                SaveDelegate();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (SaveDelegate != null)
            {
                SaveDelegate();
            }

            this.Close();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (SaveDelegate != null)
            {
                SaveDelegate();
            }

            unloadCurrentView();

            if (BackDelegate != null)
            {
                BackDelegate();
            }
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            unloadCurrentView();
            loadOrdersView();
        }

        private void listViewMaster_ItemActivate(object sender, EventArgs e)
        {
            updateListViewDetail();
        }

        private void btnCompleteOrder_Click(object sender, EventArgs e)
        {
            unloadCurrentView();
            sync_mergeDesktopAndDeviceDatabases();
            loadCompleteOrderView();
        }

        private void btnSyncHandheldWithITRetail_Click(object sender, EventArgs e)
        {
            sync_pullProductsFromITRetailDatabase();
        }

        #endregion
    }
}
