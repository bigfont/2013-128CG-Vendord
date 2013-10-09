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
    using BrightIdeasSoftware;

    public class MainForm : Form
    {
        internal Panel mainNavigation;
        internal Panel mainContent;

        // see also http://msdn.microsoft.com/en-us/library/system.windows.forms.columnheader.width%28v=vs.90%29.aspx
        private int COLUMN_HEADER_WIDTH_HEADING_LENGTH = -2;  // To autosize to the width of the column heading, set the Width property to -2.  
        private int COLUMN_HEADER_WIDTH_LONGEST_ITEM = -1; // To adjust the width of the longest item in the column, set the Width property to -1. 
        private int FORM_WIDTH_MINIMUM = 500;
        private int FORM_HEIGHT_MINIMUM = 500;
        private int COLUMN_HEADER_WIDTH_DEFAULT = 200;
        private int BUTTON_HEIGHT = 50;
        private int NUMBER_OF_NAV_BUTTONS = 2;
        private double PRINT_PREVIEW_ZOOM = 1f; // this is 100%
        private string BUTTON_MESSAGE_SEPARATOR = " : ";
        private static class USER_INPUTS
        {
            internal const string LV_MASTER = "LV_MASTER";
            internal const string LV_MEDIATOR = "LV_MEDIATOR";
            internal const string LV_DETAILS = "LV_DETAILS";
        }

        private Button btnBack;
        private delegate void Back();
        private Back BackDelegate;
        private delegate void Save();
        private Save SaveDelegate;

        public MainForm()
        {
            Control[] controls;

            this.Load += new EventHandler(MainForm_Load);
            this.Closing += new CancelEventHandler(MainForm_Closing);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.MinimumSize = new Size(FORM_WIDTH_MINIMUM, FORM_HEIGHT_MINIMUM);
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
            mainContent.Dock = DockStyle.Fill;
            mainContent.AutoSize = true;
            mainContent.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            //
            // add to form - this triggers its layout event 
            //        
            this.SuspendLayout();
            controls = new Control[] { mainContent, mainNavigation, };
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

            btnClose = new Button() { Text = "Save and Close" };
            btnClose.Click += new EventHandler(btnClose_Click);

            //
            // add to panel - this triggers its layout event
            //            

            controls = new Control[] { 
            
                btnClose,
                btnBack                
            
            };

            this.mainNavigation.SuspendLayout();

            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Top;
                c.Height = BUTTON_HEIGHT;
                this.mainNavigation.Controls.Add(c);
            }

            this.mainNavigation.ResumeLayout();
        }

        #region Utilities

        private void ButtonStatus_Clear(Button b)
        {
            int i;
            i = b.Text.LastIndexOf(BUTTON_MESSAGE_SEPARATOR);
            if (i >= 0)
            {
                b.Text = b.Text.Remove(i);
            }
        }

        private void ButtonStatus_Done(Button b, string message)
        {
            ButtonStatus_Clear(b);
            b.Text += string.Format("{0} <{1}>", BUTTON_MESSAGE_SEPARATOR, message);
            b.BackColor = Color.LightGreen;
        }

        private void ButtonStatus_Problem(Button b, string message)
        {
            ButtonStatus_Clear(b);
            b.Text += string.Format("{0} <{1}>", BUTTON_MESSAGE_SEPARATOR, message);
            b.BackColor = Color.Yellow;
        }

        private void printSelectedOrder()
        {
            ListViewPrinter listViewPrinter;
            ListView listViewDetails;
            ListView listViewMaster;

            listViewMaster = FormHelper.GetControlsByName<ListView>(mainContent, USER_INPUTS.LV_MASTER, true)[0];
            listViewDetails = FormHelper.GetControlsByName<ListView>(mainContent, USER_INPUTS.LV_DETAILS, true)[0];
            if (listViewMaster != null && listViewDetails != null)
            {
                // NOTE the listViewPrinter derives the cell width from that of the listView it's printing.

                // create the document
                listViewPrinter = new ListViewPrinter()
                {

                    // set the most important settings
                    ListView = listViewDetails,
                    DocumentName = "DocumentName",
                    Header = "Header",
                    Footer = "Footer",
                    IsShrinkToFit = false

                };

                // preview the document                                
                PrintPreviewDialog printPreview = new PrintPreviewDialog()
                {
                    Document = listViewPrinter,
                    UseAntiAlias = true
                };
                printPreview.PrintPreviewControl.Zoom = PRINT_PREVIEW_ZOOM;

                // maximize if PrintPreviewDialog can act as a Form
                if (printPreview is Form)
                {
                    (printPreview as Form).WindowState = FormWindowState.Maximized;
                }

                printPreview.ShowDialog();
            }
        }

        private ListViewItem GetSelectedListViewItem(ListView listView)
        {
            ListViewItem listViewItem;
            listViewItem = null;

            // get the selected or the first list view item
            if (listView.SelectedItems.Count > 0)
            {
                listViewItem = listView.SelectedItems[0];
            }
            else if (listView.Items.Count > 0)
            {
                listViewItem = listView.Items[0];
            }

            // if that item is the empty template
            if (listViewItem.Text.Equals("Empty"))
            {
                listViewItem = null;
            }

            // good to go
            return listViewItem;
        }

        private void updateListViewDetail()
        {
            ListView listViewMaster;
            ListView listViewMediator;
            ListView listViewDetails;
            ListViewItem selectedListViewItem;
            int orderSessionID;
            string vendorName;

            // defaults
            orderSessionID = -1;
            vendorName = null;

            // retrieve the selected orderID
            listViewMaster = FormHelper.GetControlsByName<ListView>(this, USER_INPUTS.LV_MASTER, true).First<ListView>();
            selectedListViewItem = GetSelectedListViewItem(listViewMaster);
            if (selectedListViewItem != null)
            {
                orderSessionID = Convert.ToInt32(selectedListViewItem.Tag.ToString());

                // retrieve the selected vendorName
                listViewMediator = FormHelper.GetControlsByName<ListView>(this, USER_INPUTS.LV_MEDIATOR, true).First<ListView>();
                selectedListViewItem = GetSelectedListViewItem(listViewMediator);
                if (selectedListViewItem != null)
                {
                    vendorName = selectedListViewItem.Text;
                }

                // update listViewDetails
                if (orderSessionID > 0 && vendorName != null)
                {
                    listViewDetails = FormHelper.GetControlsByName<ListView>(this, USER_INPUTS.LV_DETAILS, true).First<ListView>();
                    addDataToListViewDetails(listViewDetails, orderSessionID, vendorName);
                }
            }

            //
            // back
            //
            enableBackButton(loadOrdersView);
        }

        private void addDataToListViewDetails(ListView listViewDetails, int orderSessionID, string vendorName)
        {
            ListViewItem listViewItem;
            VendordDatabase db;
            VendordDatabase.Product product;
            listViewDetails.Items.Clear();

            if (orderSessionID >= 0 && vendorName != null && vendorName.Length > 0)
            {
                db = new VendordDatabase();
                foreach (VendordDatabase.OrderSession_Product order_product in db.OrderSession_Products.Where(i => i.OrderSessionID == orderSessionID))
                {
                    product = db.Products.FirstOrDefault(p => p.ID == order_product.ProductID && p.VendorName.Equals(vendorName));

                    if (product != null)
                    {
                        listViewItem = new ListViewItem(product.Name);
                        listViewItem.SubItems.Add(order_product.CasesToOrder.ToString());
                        listViewDetails.Items.Add(listViewItem);
                    }
                }
            }
        }

        private ListView createOrderSessionDetailsListView()
        {
            ListView listViewDetails;

            listViewDetails = new ListView()
            {

                Name = USER_INPUTS.LV_DETAILS,
                View = View.Details,
                HideSelection = false,
                Activation = ItemActivation.OneClick,
                FullRowSelect = true

            };

            // columns are required in View.Details
            listViewDetails.Columns.Add("Product");
            listViewDetails.Columns.Add("Cases to Order");

            // return 
            return listViewDetails;
        }

        private void updateListViewMediator()
        {
            ListView listViewMaster;
            ListView listViewMediator;
            ListViewItem selectedListViewItem;

            int orderSessionID;
            orderSessionID = -1;

            listViewMaster = FormHelper.GetControlsByName<ListView>(this, USER_INPUTS.LV_MASTER, true).First<ListView>();
            selectedListViewItem = GetSelectedListViewItem(listViewMaster);
            if (selectedListViewItem != null)
            {
                orderSessionID = Convert.ToInt32(selectedListViewItem.Tag.ToString());
                listViewMediator = FormHelper.GetControlsByName<ListView>(this, USER_INPUTS.LV_MEDIATOR, true).First<ListView>();
                if (orderSessionID > 0)
                {
                    addDataToListViewMediator(listViewMediator, orderSessionID);
                }
            }

            //
            // back
            //
            enableBackButton(loadOrdersView);
        }

        private void addDataToListViewMediator(ListView listViewMediator, int orderSessionID)
        {
            ListViewItem listViewItem;
            VendordDatabase db;

            listViewMediator.Items.Clear();
            db = new VendordDatabase();

            var vendorNames =
                from p in db.Products
                join op in db.OrderSession_Products on p.ID equals op.ProductID
                group p by p.VendorName into g
                select g.Key;

            foreach (string vendorName in vendorNames)
            {
                listViewItem = new ListViewItem(vendorName);
                listViewMediator.Items.Add(listViewItem);
            }
        }

        private ListView createOrderSessionMediatorListView()
        {
            ListView listViewMediator;

            listViewMediator = new ListView()
            {
                Name = USER_INPUTS.LV_MEDIATOR,
                View = View.Details,
                HideSelection = false,
                Activation = ItemActivation.OneClick,
                FullRowSelect = true,
            };
            listViewMediator.ItemActivate += new EventHandler(listViewMediator_ItemActivate);

            listViewMediator.Columns.Add("Vendor Name");

            // return
            return listViewMediator;
        }

        private void addDataToListViewMaster(ListView listViewMaster)
        {
            ListViewItem listViewItem;
            VendordDatabase db;
            // add list view items            
            db = new VendordDatabase();
            foreach (VendordDatabase.OrderSession orderSession in db.OrderSessions)
            {
                listViewItem = new ListViewItem(orderSession.Name);
                listViewItem.Tag = orderSession.ID;
                listViewMaster.Items.Add(listViewItem);
            }
        }

        private ListView createOrderSessionMasterListView()
        {
            ListView listViewMaster;

            listViewMaster = new ListView()
            {
                Name = USER_INPUTS.LV_MASTER,
                View = View.Details,
                Activation = ItemActivation.OneClick,
                HideSelection = false,
                FullRowSelect = true,
            };
            listViewMaster.ItemActivate += new EventHandler(listViewMaster_ItemActivate);

            listViewMaster.Columns.Add("Order Name");

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
            Button btnGetProductsFromITRetail;
            Button btnSyncHandheld;
            Button btnViewOrders;
            Button[] buttons;

            btnGetProductsFromITRetail = new Button() { Text = "Get Products from IT Retail" };
            btnGetProductsFromITRetail.Click += new EventHandler(btnGetProductsFromITRetail_Click);

            btnSyncHandheld = new Button() { Text = "Sync Handheld (before and after Scanning)" };
            btnSyncHandheld.Click += new EventHandler(btnSyncHandheld_Click);

            btnViewOrders = new Button() { Text = "View Orders" };
            btnViewOrders.Click += new EventHandler(btnViewOrders_Click);

            // add
            buttons = new Button[] { 

                btnGetProductsFromITRetail,
                btnViewOrders,
                btnSyncHandheld
                                 
            };

            foreach (Button b in buttons)
            {
                b.Dock = DockStyle.Top;
                b.Height = BUTTON_HEIGHT;
                this.mainContent.Controls.Add(b);
            }

            //
            // back
            // 
            enableBackButton(loadHomeView);
        }

        private void loadCompleteOrdersView()
        {
            Button btnPrintOrder;
            ListView listViewMaster;
            ListView listViewMediator;
            ListView listViewDetails;
            ListView[] listViews;
            int orderSessionID;
            string vendorName;

            // create button(s)
            btnPrintOrder = new Button() { Text = "Print Current Order" };
            btnPrintOrder.Click += new EventHandler(btnPrintOrder_Click);

            // create listviews
            listViewMaster = createOrderSessionMasterListView();
            listViewMediator = createOrderSessionMediatorListView();
            listViewDetails = createOrderSessionDetailsListView();

            // add data to the master list view
            addDataToListViewMaster(listViewMaster);
            if (listViewMaster.Items.Count > 0)
            {
                // add data to the mediator list view
                orderSessionID = Convert.ToInt32(listViewMaster.Items[0].Tag.ToString());
                addDataToListViewMediator(listViewMediator, orderSessionID);
                if (listViewMediator.Items.Count > 0)
                {
                    // add data to the details list view
                    vendorName = listViewMediator.Items[0].Text;
                    addDataToListViewDetails(listViewDetails, orderSessionID, vendorName);
                }
            }
            else
            {
                // add empty values
                listViewMaster.Items.Add("Empty");
                listViewMediator.Items.Add("Empty");
                listViewDetails.Items.Add("Empty");
            }

            // add all controls to the form
            this.mainContent.SuspendLayout();

            // start with list views
            listViews = new ListView[] { 
            
                listViewDetails,
                listViewMediator,
                listViewMaster
            
            };

            // add list views
            foreach (ListView lv in listViews)
            {
                lv.Dock = DockStyle.Left;
                lv.Width = lv.Columns.Count * COLUMN_HEADER_WIDTH_DEFAULT;

                lv.BorderStyle = BorderStyle.FixedSingle;
                lv.GridLines = true;

                foreach (ColumnHeader h in lv.Columns)
                {
                    h.Width = COLUMN_HEADER_WIDTH_DEFAULT;
                }
                this.mainContent.Controls.Add(lv);
            }

            // add button(s)
            btnPrintOrder.Dock = DockStyle.Top;
            btnPrintOrder.Height = BUTTON_HEIGHT;
            this.mainContent.Controls.Add(btnPrintOrder);

            this.mainContent.ResumeLayout();

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

        private void btnPrintOrder_Click(object sender, EventArgs e)
        {
            printSelectedOrder();
        }

        private void listViewMaster_ItemActivate(object sender, EventArgs e)
        {
            updateListViewMediator();
            updateListViewDetail();
        }

        private void listViewMediator_ItemActivate(object sender, EventArgs e)
        {
            updateListViewDetail();
        }

        private void btnViewOrders_Click(object sender, EventArgs e)
        {
            unloadCurrentView();
            loadCompleteOrdersView();
        }

        private void btnSyncHandheld_Click(object sender, EventArgs e)
        {
            Sync sync;
            Sync.SyncResult syncResult;
            sync = new Sync();

            Cursor.Current = Cursors.WaitCursor;
            syncResult = sync.MergeDesktopAndDeviceDatabases();
            Cursor.Current = Cursors.Default;

            if (syncResult == Sync.SyncResult.Complete)
            {
                ButtonStatus_Done(sender as Button, "Done");
            }
            else if (syncResult == Sync.SyncResult.Disconnected)
            {
                ButtonStatus_Problem(sender as Button, "Disconnected");
            }
        }

        private void btnGetProductsFromITRetail_Click(object sender, EventArgs e)
        {
            Sync sync;
            Sync.SyncResult syncResult;
            sync = new Sync();

            Cursor.Current = Cursors.WaitCursor;
            syncResult = sync.PullProductsFromITRetailDatabase();
            Cursor.Current = Cursors.Default;

            if (syncResult == Sync.SyncResult.Complete)
            {
                ButtonStatus_Done(sender as Button, "Done");
            }
            else if (syncResult == Sync.SyncResult.Disconnected)
            {
                ButtonStatus_Problem(sender as Button, "Disconnected");
            }
        }

        #endregion
    }
}
