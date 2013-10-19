[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.Desktop.App
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using BrightIdeasSoftware;
    using Vendord.SmartDevice.Shared;

    public class MainForm : Form
    {
        // see also http://msdn.microsoft.com/en-us/library/system.windows.forms.columnheader.width%28v=vs.90%29.aspx
        private const int ColumnHeaderWidthHeadingLength = -2;  // To autosize to the width of the column heading, set the Width property to -2.  
        private const int ColumnHeaderWidthLongestItem = -1; // To adjust the width of the longest item in the column, set the Width property to -1. 
        private const int FormWidthMinimum = 500;
        private const int FormHeightMinimum = 500;
        private const int ColumnHeaderWidthDefault = 200;
        private const int ButtonHeight = 50;
        private const int NumberOfNavigationButtons = 2;
        private const double PrintPreviewZoom = 1f; // this is 100%
        private const string ButtonMessageSeparator = " : ";

        private Panel mainNavigation;
        private Panel mainContent;

        private Button btnBack;

        private Back backDelegate;

        private Save saveDelegate; // TODO Assign to saveDelegate when we have something to save on the desktop

        public MainForm()
        {
            Control[] controls;

            this.Load += new EventHandler(this.MainForm_Load);
            this.Closing += new CancelEventHandler(this.MainForm_Closing);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.MinimumSize = new Size(FormWidthMinimum, FormHeightMinimum);
            this.BackColor = Color.White;

            // create main navigation panel
            this.mainNavigation = new Panel();
            this.mainNavigation.Dock = DockStyle.Top;
            this.mainNavigation.Height = ButtonHeight * NumberOfNavigationButtons;

            // create main content panel
            this.mainContent = new Panel();
            this.mainContent.Dock = DockStyle.Fill;
            this.mainContent.AutoSize = true;
            this.mainContent.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // add to form - this triggers its layout event        
            this.SuspendLayout();
            controls = new Control[] { this.mainContent, this.mainNavigation, };
            foreach (Control c in controls)
            {
                this.Controls.Add(c);
            }

            this.ResumeLayout();

            // Create Buttons
            Button btnClose;

            this.btnBack = new Button() { Text = "Back" };
            this.btnBack.Click += new EventHandler(this.BtnBack_Click);

            btnClose = new Button() { Text = "Save and Close" };
            btnClose.Click += new EventHandler(this.BtnClose_Click);

            // add to panel - this triggers its layout event            
            controls = new Control[] 
            {           
                btnClose,
                this.btnBack                            
            };

            this.mainNavigation.SuspendLayout();

            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Top;
                c.Height = ButtonHeight;
                this.mainNavigation.Controls.Add(c);
            }

            this.mainNavigation.ResumeLayout();
        }

        private delegate void Back();

        private delegate void Save();

        #region Utilities

        private void ButtonMessage_Clear(Button b)
        {
            int i;
            i = b.Text.LastIndexOf(ButtonMessageSeparator);
            if (i >= 0)
            {
                b.Text = b.Text.Remove(i);
            }
        }

        private void ButtonMessage_Generic(Button b, string message)
        {
            this.ButtonMessage_Clear(b);
            b.Text += string.Format("{0} <{1}>", ButtonMessageSeparator, message);
        }

        private void ButtonMessage_Done(Button b, string message)
        {
            this.ButtonMessage_Clear(b);
            b.Text += string.Format("{0} <{1}>", ButtonMessageSeparator, message);
            b.BackColor = Color.LightGreen;
        }

        private void ButtonMessage_Problem(Button b, string message)
        {
            this.ButtonMessage_Clear(b);
            b.Text += string.Format("{0} <{1}>", ButtonMessageSeparator, message);
            b.BackColor = Color.Yellow;
        }        

        private void PrintSelectedOrder()
        {
            ListViewPrinter listViewPrinter;
            ListView listViewDetails;
            ListView listViewMaster;

            listViewMaster = FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvMaster, true)[0];
            listViewDetails = FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvDetails, true)[0];
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
                printPreview.PrintPreviewControl.Zoom = PrintPreviewZoom;

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
            if (listViewItem != null && listViewItem.Text.Equals("Empty"))
            {
                listViewItem = null;
            }

            // good to go
            return listViewItem;
        }

        private void UpdateListViewDetail()
        {
            ListView listViewMaster;
            ListView listViewMediator;
            ListView listViewDetails;
            ListViewItem selectedListViewItem;
            Guid orderID;
            string vendorName;

            // defaults
            orderID = Guid.Empty;
            vendorName = null;

            // retrieve the selected orderID
            listViewMaster = FormHelper.GetControlsByName<ListView>(this, UserInputs.LvMaster, true).First<ListView>();
            selectedListViewItem = this.GetSelectedListViewItem(listViewMaster);
            if (selectedListViewItem != null)
            {
                orderID = new Guid(selectedListViewItem.Tag.ToString());

                // retrieve the selected vendorName
                listViewMediator = FormHelper.GetControlsByName<ListView>(this, UserInputs.LvMediator, true).First<ListView>();
                selectedListViewItem = this.GetSelectedListViewItem(listViewMediator);
                if (selectedListViewItem != null)
                {
                    vendorName = selectedListViewItem.Text;
                }

                // update listViewDetails
                listViewDetails = FormHelper.GetControlsByName<ListView>(this, UserInputs.LvDetails, true).First<ListView>();
                listViewDetails.Items.Clear();
                this.AddDataToListViewDetails(listViewDetails, orderID, vendorName);
            }

            // back
            this.EnableBackButton(this.LoadOrdersView);
        }

        private void AddDataToListViewDetails(ListView listViewDetails, Guid orderID, string vendorName)
        {
            ListViewItem listViewItem;
            VendordDatabase db;
            VendordDatabase.Product product;

            if (orderID != Guid.Empty && vendorName != null && vendorName.Length > 0)
            {
                db = new VendordDatabase();
                foreach (VendordDatabase.OrderProduct orderProduct in db.OrderProducts.Where(i => i.OrderID == orderID))
                {
                    product = db.Products.FirstOrDefault(p => p.ID == orderProduct.ProductID && p.VendorName.Equals(vendorName));

                    if (product != null)
                    {
                        listViewItem = new ListViewItem(product.Name);
                        listViewItem.SubItems.Add(orderProduct.CasesToOrder.ToString());
                        listViewDetails.Items.Add(listViewItem);
                    }
                }
            }
            else
            {
                this.AddEmptyListViewItem(listViewDetails);
            }
        }

        private void AddEmptyListViewItem(ListView listView)
        {
            listView.Items.Add("Empty");
        }

        private ListView CreateOrderDetailsListView()
        {
            ListView listViewDetails;

            listViewDetails = new ListView()
            {
                Name = UserInputs.LvDetails,
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

        private void UpdateListViewMediator()
        {
            ListView listViewMaster;
            ListView listViewMediator;
            ListViewItem selectedListViewItem;

            Guid orderID;
            orderID = Guid.Empty;

            listViewMaster = FormHelper.GetControlsByName<ListView>(this, UserInputs.LvMaster, true).First<ListView>();
            selectedListViewItem = this.GetSelectedListViewItem(listViewMaster);
            if (selectedListViewItem != null)
            {
                orderID = new Guid(selectedListViewItem.Tag.ToString());
                listViewMediator = FormHelper.GetControlsByName<ListView>(this, UserInputs.LvMediator, true).First<ListView>();
                listViewMediator.Items.Clear();
                this.AddDataToListViewMediator(listViewMediator, orderID);
            }

            // back
            this.EnableBackButton(this.LoadOrdersView);
        }

        private void AddDataToListViewMediator(ListView listViewMediator, Guid orderID)
        {
            ListViewItem listViewItem;
            VendordDatabase db;

            db = new VendordDatabase();

            var vendorNames =
                from p in db.Products
                join op in db.OrderProducts on p.ID equals op.ProductID
                where op.OrderID == orderID
                group p by p.VendorName into g
                select g.Key;

            if (vendorNames.Count() > 0)
            {
                foreach (string vendorName in vendorNames)
                {
                    listViewItem = new ListViewItem(vendorName);
                    listViewMediator.Items.Add(listViewItem);
                }
            }
            else
            {
                this.AddEmptyListViewItem(listViewMediator);
            }
        }

        private ListView CreateOrderMediatorListView()
        {
            ListView listViewMediator;

            listViewMediator = new ListView()
            {
                Name = UserInputs.LvMediator,
                View = View.Details,
                HideSelection = false,
                Activation = ItemActivation.OneClick,
                FullRowSelect = true,
            };

            listViewMediator.ItemActivate += new EventHandler(this.ListViewMediator_ItemActivate);

            listViewMediator.Columns.Add("Vendor Name");

            // return
            return listViewMediator;
        }

        private void AddDataToListViewMaster(ListView listViewMaster)
        {
            ListViewItem listViewItem;
            VendordDatabase db;

            // add list view items            
            db = new VendordDatabase();
            foreach (VendordDatabase.Order order in db.Orders)
            {
                listViewItem = new ListViewItem(order.Name);
                listViewItem.Tag = order.ID;
                listViewMaster.Items.Add(listViewItem);
            }
        }

        private ListView CreateOrderMasterListView()
        {
            ListView listViewMaster;

            listViewMaster = new ListView()
            {
                Name = UserInputs.LvMaster,
                View = View.Details,
                Activation = ItemActivation.OneClick,
                HideSelection = false,
                FullRowSelect = true,
            };

            // occurs when an ListViewItem is activated
            listViewMaster.ItemActivate += new EventHandler(this.ListViewMaster_ItemActivate);

            listViewMaster.Columns.Add("Order Name");

            // return
            return listViewMaster;
        }

        private void DisableBackButton()
        {
            this.btnBack.Enabled = false;
            this.backDelegate = null;
        }

        private void EnableBackButton(Back method)
        {
            this.btnBack.Enabled = true;
            this.backDelegate = method;
        }

        #endregion

        #region Views

        private void UnloadCurrentView()
        {
            this.mainContent.Controls.Clear();
        }

        private void LoadHomeView()
        {
            Button btnOrders;

            btnOrders = new Button() { Text = "Orders" };
            btnOrders.Click += new EventHandler(this.BtnOrders_Click);

            btnOrders.Dock = DockStyle.Top;
            btnOrders.Height = ButtonHeight;
            this.mainContent.Controls.Add(btnOrders);

            this.DisableBackButton();
        }

        private void LoadOrdersView()
        {
            Button btnGetProductsFromITRetail;
            Button btnSyncHandheld;
            Button btnViewOrders;
            Button[] buttons;

            btnGetProductsFromITRetail = new Button() { Text = "Get Products from IT Retail" };
            btnGetProductsFromITRetail.Click += new EventHandler(this.BtnGetProductsFromITRetail_Click);

            btnSyncHandheld = new Button() { Text = "Sync Handheld (before and after Scanning)" };
            btnSyncHandheld.Click += new EventHandler(this.BtnSyncHandheld_Click);

            btnViewOrders = new Button() { Text = "View Orders" };
            btnViewOrders.Click += new EventHandler(this.BtnViewOrders_Click);

            // add
            buttons = new Button[] 
            { 
                btnGetProductsFromITRetail,
                btnViewOrders,
                btnSyncHandheld
            };

            foreach (Button b in buttons)
            {
                b.Dock = DockStyle.Top;
                b.Height = ButtonHeight;
                this.mainContent.Controls.Add(b);
            }

            // back
            this.EnableBackButton(this.LoadHomeView);
        }

        private void LoadCompleteOrdersView()
        {
            Button btnPrintOrder;
            Button btnCreateItem;
            Button btnDeleteItem;
            ListView listViewMaster;
            ListView listViewMediator;
            ListView listViewDetails;
            ListView[] listViews;
            Guid orderID;
            string vendorName;

            // create button(s)
            btnPrintOrder = new Button() { Text = "Print Current Order" };
            btnPrintOrder.Click += new EventHandler(this.BtnPrintOrder_Click);
            btnCreateItem = new Button() { Text = "Create", Name = UserInputs.BtnCreate };
            btnCreateItem.Click += new EventHandler(this.BtnCreateItem_Click);
            btnDeleteItem = new Button() { Text = "Delete", Name = UserInputs.BtnDelete };
            btnDeleteItem.Click += new EventHandler(this.BtnDeleteItem_Click);

            // create listviews
            listViewMaster = this.CreateOrderMasterListView();
            listViewMediator = this.CreateOrderMediatorListView();
            listViewDetails = this.CreateOrderDetailsListView();

            // add data to the master list view
            this.AddDataToListViewMaster(listViewMaster);
            if (listViewMaster.Items.Count > 0)
            {
                // add data to the mediator list view
                orderID = new Guid(listViewMaster.Items[0].Tag.ToString());
                listViewMediator.Items.Clear();
                this.AddDataToListViewMediator(listViewMediator, orderID);
                if (listViewMediator.Items.Count > 0)
                {
                    // add data to the details list view
                    vendorName = listViewMediator.Items[0].Text;
                    listViewDetails.Items.Clear();
                    this.AddDataToListViewDetails(listViewDetails, orderID, vendorName);
                }
            }
            else
            {
                this.AddEmptyListViewItem(listViewMaster);
                this.AddEmptyListViewItem(listViewMediator);
                this.AddEmptyListViewItem(listViewDetails);
            }

            // add all controls to the form
            this.mainContent.SuspendLayout();

            // start with list views
            listViews = new ListView[] 
            { 
                listViewDetails,
                listViewMediator,
                listViewMaster
            };

            // add list views
            foreach (ListView lv in listViews)
            {
                lv.Dock = DockStyle.Left;
                lv.Width = lv.Columns.Count * ColumnHeaderWidthDefault;

                lv.BorderStyle = BorderStyle.FixedSingle;
                lv.GridLines = true;

                foreach (ColumnHeader h in lv.Columns)
                {
                    h.Width = ColumnHeaderWidthDefault;
                }

                // occurs when any click happens in the ListView
                lv.MouseDown += new MouseEventHandler(this.ListView_MouseClick);

                // makes the ListViewItem.BeginEdit() method effective
                lv.LabelEdit = true;

                // handle the completion of editing
                lv.AfterLabelEdit += new LabelEditEventHandler(this.ListView_AfterLabelEdit);

                this.mainContent.Controls.Add(lv);
            }

            // set selected... you have to focus before setting the selected item :-|
            listViewMediator.Focus();
            listViewMediator.Items[0].Selected = true;

            listViewMaster.Focus();
            listViewMaster.Items[0].Selected = true;

            // add button(s)            
            btnPrintOrder.Dock = DockStyle.Top;
            btnPrintOrder.Height = ButtonHeight;
            this.mainContent.Controls.Add(btnPrintOrder);

            btnCreateItem.Dock = DockStyle.Top;
            btnCreateItem.Height = ButtonHeight;
            this.mainContent.Controls.Add(btnCreateItem);

            btnDeleteItem.Dock = DockStyle.Top;
            btnDeleteItem.Height = ButtonHeight;
            this.mainContent.Controls.Add(btnDeleteItem);

            this.mainContent.ResumeLayout();

            // back
            this.EnableBackButton(this.LoadOrdersView);
        }

        #endregion

        #region Events

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.LoadHomeView();
        }

        private void MainForm_Closing(object sender, EventArgs e)
        {
            if (this.saveDelegate != null)
            {
                this.saveDelegate();
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            if (this.saveDelegate != null)
            {
                this.saveDelegate();
            }

            this.Close();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (this.saveDelegate != null)
            {
                this.saveDelegate();
            }

            this.UnloadCurrentView();

            if (this.backDelegate != null)
            {
                this.backDelegate();
            }
        }

        private void BtnOrders_Click(object sender, EventArgs e)
        {
            this.UnloadCurrentView();
            this.LoadOrdersView();
        }

        private void BtnPrintOrder_Click(object sender, EventArgs e)
        {
            this.PrintSelectedOrder();
        }

        private void BtnCreateItem_Click(object sender, EventArgs e)
        {
            List<ListView> listViews;
            ListView targetListView;
            ListViewItem listViewItem;
            
            listViews = FormHelper.GetControlsByType<ListView>(this, true);
            targetListView = listViews.FirstOrDefault<ListView>(lv => lv.Tag != null && lv.Tag is ControlTag && (lv.Tag as ControlTag).IsSelected);

            if (targetListView != null)
            {
                listViewItem = new ListViewItem("Edit Me");                
                targetListView.Items.Add(listViewItem);
                listViewItem.BeginEdit();                
            }
        }

        private void BtnDeleteItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            VendordDatabase.Order order;

            if (e.Label != null && e.Label.Length > 0)
            { 
                // TODO Add validation for label length
                order = new VendordDatabase.Order() 
                { 
                    Name = e.Label 
                };
                order.UpsertIntoDB(new VendordDatabase());
            }
        }

        private void ListView_MouseClick(object sender, MouseEventArgs e)
        {
            ListView listView;
            string listViewItemType;
            ControlTag tag = new ControlTag();

            listView = sender as ListView;
            listView.Focus();
            tag.IsSelected = true;
            listView.Tag = tag;

            listViewItemType = string.Empty;
            switch (listView.Name)
            {
                case UserInputs.LvMaster:
                    listViewItemType = "Order";
                    break;

                case UserInputs.LvMediator:
                    listViewItemType = "Vendor";
                    break;

                case UserInputs.LvDetails:
                    listViewItemType = "Product";
                    break;

                default: break;
            }

            this.ButtonMessage_Generic(FormHelper.GetControlsByName<Button>(this, UserInputs.BtnCreate, true).FirstOrDefault(), listViewItemType);
            this.ButtonMessage_Generic(FormHelper.GetControlsByName<Button>(this, UserInputs.BtnDelete, true).FirstOrDefault(), listViewItemType);
        }

        private void ListViewMaster_ItemActivate(object sender, EventArgs e)
        {
            this.UpdateListViewMediator();
            this.UpdateListViewDetail();
        }

        private void ListViewMediator_ItemActivate(object sender, EventArgs e)
        {
            this.UpdateListViewDetail();
        }

        private void BtnViewOrders_Click(object sender, EventArgs e)
        {
            this.UnloadCurrentView();
            this.LoadCompleteOrdersView();
        }

        private void BtnSyncHandheld_Click(object sender, EventArgs e)
        {
            Sync sync;
            Sync.SyncResult syncResult;
            sync = new Sync();

            Cursor.Current = Cursors.WaitCursor;
            syncResult = sync.MergeDesktopAndDeviceDatabases();
            Cursor.Current = Cursors.Default;

            if (syncResult == Sync.SyncResult.Complete)
            {
                this.ButtonMessage_Done(sender as Button, "Done");
            }
            else if (syncResult == Sync.SyncResult.Disconnected)
            {
                this.ButtonMessage_Problem(sender as Button, "Disconnected");
            }
        }

        private void BtnGetProductsFromITRetail_Click(object sender, EventArgs e)
        {
            Sync sync;
            Sync.SyncResult syncResult;
            sync = new Sync();

            Cursor.Current = Cursors.WaitCursor;
            syncResult = sync.PullProductsFromITRetailDatabase();
            Cursor.Current = Cursors.Default;

            if (syncResult == Sync.SyncResult.Complete)
            {
                this.ButtonMessage_Done(sender as Button, "Done");
            }
            else if (syncResult == Sync.SyncResult.Disconnected)
            {
                this.ButtonMessage_Problem(sender as Button, "Disconnected");
            }
        }

        #endregion

        private class UserInputs
        {
            internal const string LvMaster = "LvMaster";
            internal const string LvMediator = "LvMediator";
            internal const string LvDetails = "LvDetails";
            internal const string BtnCreate = "BtnCreate";
            internal const string BtnDelete = "BtnDelete";
        }

        private class ControlTag
        {
            public bool IsSelected { get; set; }
        }
    }
}
