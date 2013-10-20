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
        private const char ButtonMessageStartChar = '<';
        private const char ButtonMessageEndChar = '>';

        private Panel mainNavigation;
        private Panel mainContent;
        private ListView selectedListView;

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
            i = b.Text.IndexOf(ButtonMessageStartChar);
            if (i >= 0)
            {
                b.Text = b.Text.Remove(i);
                b.Text = b.Text.Trim();
            }
        }

        private string CreateButtonMessage(string message)
        {
            return string.Format(" {0}{2}{1} ", ButtonMessageStartChar, ButtonMessageEndChar, message);
        }

        private void ButtonMessage_Generic(Button b, string message)
        {
            this.ButtonMessage_Clear(b);
            b.Text += CreateButtonMessage(message);
        }

        private void ButtonMessage_Done(Button b, string message)
        {
            this.ButtonMessage_Clear(b);
            b.Text += CreateButtonMessage(message);
            b.BackColor = Color.LightGreen;
        }

        private void ButtonMessage_Problem(Button b, string message)
        {
            this.ButtonMessage_Clear(b);
            b.Text += CreateButtonMessage(message);
            b.BackColor = Color.Yellow;
        }

        private void PrintSelectedOrder()
        {
            ListViewPrinter listViewPrinter;
            ListView listViewProduct;
            ListView listViewOrder;

            listViewOrder = FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvOrder, true)[0];
            listViewProduct = FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvProduct, true)[0];
            if (listViewOrder != null && listViewProduct != null)
            {
                // NOTE the listViewPrinter derives the cell width from that of the listView it's printing.

                // create the document
                listViewPrinter = new ListViewPrinter()
                {
                    // set the most important settings
                    ListView = listViewProduct,
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

        private void AddItemToListViewProduct(VendordDatabase.Product product, int casesToOrder, ListView listViewProduct)
        {
            ListViewItem listViewItem;
            if (product != null)
            {
                listViewItem = new ListViewItem(product.Name);
                listViewItem.Tag = product.ID;
                listViewItem.Name = product.Name;
                listViewItem.SubItems.Add(casesToOrder.ToString());
                listViewProduct.Items.Add(listViewItem);
            }
        }

        private void AddListBoxProduct()
        {
            ListBox listBox;
            ListView listViewVendor;
            string currentVendor;            

            listBox = FormHelper.GetControlsByName<ListBox>(this.mainContent, UserInputs.LbSelect, true).FirstOrDefault<ListBox>();

            if (listBox == null)
            {
                listBox = new ListBox();
                listBox.Dock = DockStyle.Right;
                listBox.Name = UserInputs.LbSelect;
                listBox.SelectedIndexChanged += new EventHandler(this.ListBox_SelectedIndexChanged);

                listViewVendor = FormHelper
                    .GetControlsByName<ListView>(this.mainContent, UserInputs.LvVendor, true)
                    .FirstOrDefault<ListView>();

                if (listViewVendor != null)
                {
                    if (listViewVendor.FocusedItem == null)
                    {
                        // show them all
                        foreach (VendordDatabase.Product product in (new VendordDatabase()).Products)
                        {
                            listBox.Items.Add(product);
                        }
                    }
                    else
                    {
                        // filter on vendor
                        currentVendor = listViewVendor.FocusedItem.Text;
                        foreach (VendordDatabase.Product product in ((new VendordDatabase()).Products.Where(p => p.VendorName == currentVendor)))
                        {
                            listBox.Items.Add(product);
                        }                        
                    }
                    this.mainContent.Controls.Add(listBox);
                }
            }
            else
            {
                this.mainContent.Controls.Remove(listBox);
            }
        }

        private void UpdateListViewProduct()
        {
            ListView listViewOrder;
            ListView listViewVendor;
            ListView listViewProduct;
            ListViewItem selectedListViewOrderItem;
            ListViewItem selectedListViewVendorItem;
            Guid orderID;
            string vendorName;

            // clear
            listViewProduct = FormHelper.GetControlsByName<ListView>(this, UserInputs.LvProduct, true).First<ListView>();
            listViewProduct.Items.Clear();

            // defaults            
            vendorName = null;

            // retrieve the selected orderID
            listViewOrder = FormHelper.GetControlsByName<ListView>(this, UserInputs.LvOrder, true).First<ListView>();
            selectedListViewOrderItem = listViewOrder.FocusedItem;
            if (selectedListViewOrderItem != null)
            {
                orderID = new Guid(selectedListViewOrderItem.Tag.ToString());

                // retrieve the selected vendorName
                listViewVendor = FormHelper.GetControlsByName<ListView>(this, UserInputs.LvVendor, true).First<ListView>();
                selectedListViewVendorItem = listViewVendor.FocusedItem;
                if (selectedListViewVendorItem != null)
                {
                    vendorName = selectedListViewVendorItem.Text;
                }

                // update listViewProduct 
                this.AddDataToListViewProduct(listViewProduct, orderID, vendorName);
            }            

            // back
            this.EnableBackButton(this.LoadOrdersView);
        }

        private void AddDataToListViewProduct(ListView listViewProduct, Guid orderID, string vendorName)
        {
            VendordDatabase db;
            VendordDatabase.Product product;

            if (orderID != Guid.Empty && vendorName != null && vendorName.Length > 0)
            {
                db = new VendordDatabase();
                foreach (VendordDatabase.OrderProduct orderProduct in db.OrderProducts.Where(i => i.OrderID == orderID))
                {
                    product = db.Products.FirstOrDefault(p => p.ID == orderProduct.ProductID && p.VendorName.Equals(vendorName));
                    this.AddItemToListViewProduct(product, orderProduct.CasesToOrder, listViewProduct);
                }
            }
        }

        private ListView CreateListViewProduct()
        {
            ListView listViewProduct;

            listViewProduct = new ListView()
            {
                Name = UserInputs.LvProduct,
                View = View.Details,
                HideSelection = false,
                Activation = ItemActivation.OneClick,
                FullRowSelect = true,
                MultiSelect = false
            };

            // columns are required in View.Details
            listViewProduct.Columns.Add("Product");
            listViewProduct.Columns.Add("Cases to Order");
            
            listViewProduct.MouseDown += new MouseEventHandler(this.ListViewAny_MouseClick);
            listViewProduct.LabelEdit = true; // makes the ListViewItem.BeginEdit() method effective
            listViewProduct.ItemActivate += new EventHandler(ListViewProduct_ItemActivate);

            // return 
            return listViewProduct;
        }

        private void UpdateListViewVendor()
        {
            ListView listViewOrder;
            ListView listViewVendor;
            ListViewItem selectedOrder;
            Guid orderID;

            listViewVendor = FormHelper.GetControlsByName<ListView>(this, UserInputs.LvVendor, true).First<ListView>();
            listViewVendor.Items.Clear();
            
            listViewOrder = FormHelper.GetControlsByName<ListView>(this, UserInputs.LvOrder, true).First<ListView>();
            selectedOrder = listViewOrder.FocusedItem;
            if (selectedOrder != null)
            {                
                orderID = new Guid(selectedOrder.Tag.ToString());
                this.AddDataToListViewVendor(listViewVendor, orderID);
            }

            // back
            this.EnableBackButton(this.LoadOrdersView);
        }

        private void AddDataToListViewVendor(ListView listViewVendor, Guid orderID)
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
                    listViewItem.Name = vendorName;
                    listViewVendor.Items.Add(listViewItem);
                }
            }
        }

        private ListView CreateListViewVendor()
        {
            ListView listViewVendor;

            listViewVendor = new ListView()
            {
                Name = UserInputs.LvVendor,
                View = View.Details,
                HideSelection = false,
                Activation = ItemActivation.OneClick,
                FullRowSelect = true,
                MultiSelect = false
            };

            listViewVendor.Columns.Add("Vendor Name");

            listViewVendor.ItemActivate += new EventHandler(this.ListViewVendor_ItemActivate);
            listViewVendor.LabelEdit = true; // makes the ListViewItem.BeginEdit() method effective

            return listViewVendor;
        }

        private void AddDataToListViewOrder(ListView listViewOrder)
        {
            ListViewItem listViewItem;
            VendordDatabase db;

            // add list view items            
            db = new VendordDatabase();
            foreach (VendordDatabase.Order order in db.Orders)
            {
                listViewItem = new ListViewItem(order.Name);
                listViewItem.Tag = order.ID;
                listViewItem.Name = order.Name;
                listViewOrder.Items.Add(listViewItem);
            }
        }

        private ListView CreateListViewOrder()
        {
            ListView listViewOrder;

            listViewOrder = new ListView()
            {
                Name = UserInputs.LvOrder,
                View = View.Details,
                Activation = ItemActivation.OneClick,
                HideSelection = false,
                FullRowSelect = true,
                MultiSelect = false
            };

            // occurs when an ListViewItem is activated
            listViewOrder.ItemActivate += new EventHandler(this.ListViewOrder_ItemActivate);

            // add user visible columns
            listViewOrder.Columns.Add("Order Name");

            // return
            return listViewOrder;
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
            ListView listViewOrder;
            ListView listViewVendor;
            ListView listViewProduct;
            ListView[] listViews;

            // create button(s)
            btnPrintOrder = new Button() { Text = "Print Current Order" };
            btnPrintOrder.Click += new EventHandler(this.BtnPrintOrder_Click);
            btnCreateItem = new Button() { Text = "Add Product to Current Order", Name = UserInputs.BtnCreate };
            btnCreateItem.Click += new EventHandler(this.BtnCreateItem_Click);
            btnDeleteItem = new Button() { Text = "Delete Selected", Name = UserInputs.BtnDelete };
            btnDeleteItem.Click += new EventHandler(this.BtnDeleteItem_Click);

            // create listviews
            listViewOrder = this.CreateListViewOrder();
            listViewVendor = this.CreateListViewVendor();
            listViewProduct = this.CreateListViewProduct();

            // add data to the order list view
            this.AddDataToListViewOrder(listViewOrder);

            // add all controls to the form
            this.mainContent.SuspendLayout();

            // start with list views
            listViews = new ListView[] 
            { 
                listViewProduct,
                listViewVendor,
                listViewOrder
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

                lv.MouseDown += new MouseEventHandler(this.ListViewAny_MouseClick);
                lv.SelectedIndexChanged += new EventHandler(this.ListViewAny_SelectedIndexChanged_DisallowZeroSelectedItems);

                this.mainContent.Controls.Add(lv);
            }
                       
            // add button(s)            
            btnPrintOrder.Dock = DockStyle.Top;
            btnPrintOrder.Height = ButtonHeight;
            this.mainContent.Controls.Add(btnPrintOrder);

            btnDeleteItem.Dock = DockStyle.Top;
            btnDeleteItem.Height = ButtonHeight;
            this.mainContent.Controls.Add(btnDeleteItem);

            btnCreateItem.Dock = DockStyle.Top;
            btnCreateItem.Height = ButtonHeight;
            this.mainContent.Controls.Add(btnCreateItem);

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
            this.AddListBoxProduct();
        }

        private void BtnDeleteItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Coming soon.");
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView listViewProduct;
            ListView listViewVendor;
            ListView listViewOrder;
            ListViewItem listViewItemVendor;
            ListViewItem listViewItemProduct;
            ListBox listBox;
            VendordDatabase.Product product;

            if (sender != null && sender is ListBox)
            {
                listBox = sender as ListBox;
                if (listBox.SelectedItem != null && listBox.SelectedItem is VendordDatabase.Product)
                {
                    listViewOrder = FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvOrder, true).FirstOrDefault<ListView>();
                    if (listViewOrder.FocusedItem != null)
                    {
                        product = listBox.SelectedItem as VendordDatabase.Product;
                        this.SaveProductToOrder(product, new Guid(listViewOrder.FocusedItem.Tag.ToString()), 0);

                        this.UpdateListViewVendor();
                        listViewVendor = FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvVendor, true).FirstOrDefault<ListView>();
                        listViewItemVendor = listViewVendor.Items.Find(product.VendorName, false)[0];
                        listViewItemVendor.Selected = true;
                        listViewItemVendor.Focused = true;

                        this.UpdateListViewProduct();
                        listViewProduct = FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvProduct, true).FirstOrDefault<ListView>();
                        listViewItemProduct = listViewProduct.Items.Find(product.Name, false)[0];
                        listViewItemProduct.Selected = true;
                        listViewItemProduct.Focused = true;
                        EditProductCasesToOrder(listViewProduct, listViewItemProduct);                      
                    }
                }
            }
        }

        // HACK Add textbox overlay - might be better just to use a GridView instead of ListView        
        private void EditProductCasesToOrder(ListView listViewProduct, ListViewItem listViewItemProduct)
        {
            TextBox textbox;
            int x, y;

            textbox = new TextBox();
            textbox.BorderStyle = BorderStyle.FixedSingle;
            textbox.Margin = new Padding(0);

            textbox.Multiline = true;
            textbox.MinimumSize = listViewItemProduct.SubItems[1].Bounds.Size;
            textbox.Size = listViewItemProduct.SubItems[1].Bounds.Size;

            textbox.Text = listViewItemProduct.SubItems[1].Text;
            textbox.LostFocus += new EventHandler(Textbox_LostFocus);

            x =
                listViewProduct.Location.X +
                listViewItemProduct.SubItems[1].Bounds.Location.X;
            y =
                listViewProduct.Location.Y +
                listViewItemProduct.SubItems[1].Bounds.Location.Y;

            textbox.Location = new Point(x, y);

            this.mainContent.Controls.Add(textbox);
            textbox.BringToFront();
            textbox.Focus();
            textbox.SelectAll();
        }

        private void SaveProductToOrder(VendordDatabase.Product product, Guid orderID, int casesToOrder)
        {
            VendordDatabase.OrderProduct orderProduct;

            orderProduct = new VendordDatabase.OrderProduct()
            {
                OrderID = orderID,
                ProductID = product.ID,
                CasesToOrder = Convert.ToInt32(casesToOrder)
            };
            orderProduct.UpsertIntoDB(new VendordDatabase());
        }

        private void Textbox_LostFocus(object sender, EventArgs e)
        {
            (sender as Control).Parent.Controls.Remove(sender as Control);            
        }

        private void ListViewAny_SelectedIndexChanged_DisallowZeroSelectedItems(object sender, EventArgs e)
        {
            if ((sender as ListView).FocusedItem != null)
            {
                (sender as ListView).FocusedItem.Selected = true;
            }
        }

        private void ListViewAny_MouseClick(object sender, MouseEventArgs e)
        {
            string listViewItemType;
            this.selectedListView = sender as ListView;
            switch (this.selectedListView.Name)
            {
                case UserInputs.LvVendor:
                    listViewItemType = "Vendor";
                    break;

                case UserInputs.LvProduct:
                    listViewItemType = "Product";
                    break;

                case UserInputs.LvOrder:
                    listViewItemType = "Order";
                    break;

                default:
                    listViewItemType = string.Empty;
                    break;
            }

            this.ButtonMessage_Generic(FormHelper.GetControlsByName<Button>(this, UserInputs.BtnDelete, true).FirstOrDefault(), listViewItemType);
        }

        private void ListViewOrder_ItemActivate(object sender, EventArgs e)
        {            
            this.UpdateListViewVendor();
            this.UpdateListViewProduct();            
        }

        private void ListViewVendor_ItemActivate(object sender, EventArgs e)
        {            
            this.UpdateListViewProduct();            
        }

        private void ListViewProduct_ItemActivate(object sender, EventArgs e)
        {
            ListView listViewProduct;
            ListViewItem listViewItemProduct;

            listViewProduct = sender as ListView;
            listViewItemProduct = listViewProduct != null ? listViewProduct.FocusedItem : null;
            if (listViewItemProduct != null)
            {
                EditProductCasesToOrder(listViewProduct, listViewItemProduct);
            }
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
            internal const string LvOrder = "LvOrder";
            internal const string LvVendor = "LvVendor";
            internal const string LvProduct = "LvProduct";
            internal const string BtnCreate = "BtnCreate";
            internal const string BtnDelete = "BtnDelete";
            internal const string LbSelect = "LbSelect";
        }
    }
}
