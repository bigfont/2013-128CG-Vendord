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
    using System.Drawing.Printing;
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
        private const string OrderFor = "Country Grocer Salt Spring";
        private const string Dept = "Grocery";

        private Panel mainNavigation;
        private Panel mainContent;
        private int listViewItemIndexToPrintNext = 0;
        private Font myFont = new Font(FontFamily.GenericSerif, 12.0F);
        private Brush myFontBrush = Brushes.Black;
        private Point myPoint = new Point(0, 0);

        private Button btnBack;

        private Back backDelegate;

        private Save saveDelegate; // TODO Assign to saveDelegate when we have something to save on the desktop

        private float myHeaderFontSize = 12.0F;
        private float myHeaderTextInset = 6.0F;
        private float myHeaderBoarderThickness = 1.0F;
        private float myHeaderPaddingThickness = 5.0F;
        private float myHeaderHeightCorrection = 10.0F;

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

        private ListView LvOrder
        {
            get { return FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvOrder, false)[0]; }
        }

        private ListView LvVendor
        {
            get { return FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvVendor, false)[0]; }
        }

        private ListView LvOrderProduct
        {
            get { return FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvOrderProduct, false)[0]; }
        }

        #region Utilities

        private ListViewItem GetSelectedListViewItem(ListView listView)
        {
            ListViewItem result;
            result = null;
            if (listView.SelectedItems.Count != 0)
            {
                result = listView.SelectedItems[0];
            }
            return result;
        }

        private void EditOrderProductCasesToOrder(ListView listViewOrderProduct, ListViewItem listViewItemOrderProduct)
        {
            TextBox textbox;
            int x, y;

            textbox = new TextBox();
            textbox.BorderStyle = BorderStyle.FixedSingle;
            textbox.Margin = new Padding(0);

            textbox.Multiline = true;
            textbox.MinimumSize = listViewItemOrderProduct.SubItems[1].Bounds.Size;
            textbox.Size = listViewItemOrderProduct.SubItems[1].Bounds.Size;

            textbox.Text = listViewItemOrderProduct.SubItems[1].Text;
            textbox.Tag = new TagProperties() { OriginalText = textbox.Text };

            textbox.LostFocus += new EventHandler(this.TextboxCasesToOrder_LostFocus_SaveChanges);
            textbox.KeyPress += new KeyPressEventHandler(this.Textbox_KeyPress_WhiteList);

            x =
                listViewOrderProduct.Location.X +
                listViewItemOrderProduct.SubItems[1].Bounds.Location.X;
            y =
                listViewOrderProduct.Location.Y +
                listViewItemOrderProduct.SubItems[1].Bounds.Location.Y;

            textbox.Location = new Point(x, y);

            this.mainContent.Controls.Add(textbox);
            textbox.BringToFront();
            textbox.Focus();
            textbox.SelectAll();
        }

        private int DeleteSelectedOrderProduct()
        {
            ListViewItem selectedListViewOrderItem;
            ListViewItem selectedListViewOrderProductItem;
            OrderProduct orderProduct;

            selectedListViewOrderItem = GetSelectedListViewItem(this.LvOrder);
            selectedListViewOrderProductItem = GetSelectedListViewItem(this.LvOrderProduct);

            if (selectedListViewOrderItem != null && selectedListViewOrderItem.Tag != null &&
                selectedListViewOrderProductItem != null && selectedListViewOrderProductItem.Tag != null)
            {
                orderProduct = new OrderProduct()
                {
                    OrderID = new Guid(selectedListViewOrderItem.Tag.ToString()),
                    ProductUPC = selectedListViewOrderProductItem.Tag.ToString()
                };
                orderProduct.AddToTrash(new Database());
            }

            return selectedListViewOrderProductItem.Index;
        }

        private void DeleteSelectedOrder()
        {
            Order order;

            order = new Order()
            {
                ID = new Guid(GetSelectedListViewItem(this.LvOrder).Tag.ToString())
            };
            order.AddToTrash(new Database());
        }

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

        private void ButtonMessage_Generic(Button b, string message, string tag)
        {
            if (b != null)
            {
                this.ButtonMessage_Clear(b);
                b.Text += this.CreateButtonMessage(message);
                b.Tag = tag;
            }
        }

        private void ButtonMessage_Done(Button b, string message)
        {
            if (b != null)
            {
                this.ButtonMessage_Clear(b);
                b.Text += this.CreateButtonMessage(message);
                b.BackColor = Color.LightGreen;
            }
        }

        private void ButtonMessage_Problem(Button b, string message)
        {
            if (b != null)
            {
                this.ButtonMessage_Clear(b);
                b.Text += this.CreateButtonMessage(message);
                b.BackColor = Color.Yellow;
            }
        }
        
        private ListViewPrinter CreateListViewPrinterFromListViewOrderProduct_Deprecated()
        {
            ListViewPrinter listViewPrinter;

            string supplier;
            StringBuilder header;

            // get the name of the supplier
            supplier = this.LvVendor.SelectedItems[0].Text;

            // build the report header
            header = new StringBuilder();
            header.Append("Order for: " + OrderFor.ToUpper());
            header.Append("\nDept: " + Dept.ToUpper());
            header.Append("\nSupplier: " + supplier.ToUpper());
            header.Append("\nOrder Created: " + DateTime.Now.ToLongDateString());

            // NOTE the listViewPrinter derives the cell width from that of the listView it's printing.

            // create the document
            listViewPrinter = new ListViewPrinter()
            {
                // set the most important settings
                ListView = this.LvOrderProduct,
                Header = header.ToString(),
                IsShrinkToFit = false
            };

            listViewPrinter.HeaderFormat = new BlockFormat()
            {
                Font = new Font(FontFamily.GenericSansSerif, this.myHeaderFontSize, FontStyle.Regular, GraphicsUnit.Point),
                CanWrap = true,
                MinimumTextHeight = (this.myHeaderFontSize * 4) + (this.myHeaderTextInset * 2) + (this.myHeaderBoarderThickness * 2) + (this.myHeaderPaddingThickness * 2) + this.myHeaderHeightCorrection
            };
            listViewPrinter.HeaderFormat.SetTextInset(Sides.All, this.myHeaderTextInset);
            listViewPrinter.HeaderFormat.SetBorder(Sides.All, this.myHeaderBoarderThickness, new SolidBrush(Color.Black));
            listViewPrinter.HeaderFormat.SetPadding(Sides.All, this.myHeaderPaddingThickness);

            return listViewPrinter;
        }

        private void PreviewPrintDocument(PrintDocument printDocument)
        {
            // preview the document                                
            PrintPreviewDialog printPreview = new PrintPreviewDialog()
            {
                Document = printDocument,
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

        private void AddSelectedVendorOrderToThePrintDocument(PrintPageEventArgs e)
        {
            string vendorName;
            string productName;

            vendorName = this.LvVendor.SelectedItems[0].Text;
            e.Graphics.DrawString(vendorName, this.myFont, this.myFontBrush, this.myPoint);

            foreach (ListViewItem item in this.LvOrderProduct.Items)
            {
                this.myPoint.Y += 50;

                productName = item.Text;
                e.Graphics.DrawString(productName, this.myFont, this.myFontBrush, this.myPoint);
            }
        }

        private void PrintSelectedOrder()
        {
            if (this.LvOrder != null && this.LvOrderProduct != null & this.LvVendor != null)
            {
                if (this.LvOrder.SelectedItems[0] != null && this.LvVendor.Items != null && this.LvVendor.Items.Count > 0)
                {
                    PrintDocument printDocument = new PrintDocument();
                    if (this.LvVendor.SelectedItems.Count == 0)
                    {
                        printDocument.PrintPage += new PrintPageEventHandler(this.PrintDocument_PrintOrderForAllVendors);
                    }
                    else
                    {
                        printDocument.PrintPage += new PrintPageEventHandler(this.PrintDocument_PrintOrderForSpecificVendor);
                    }

                    this.PreviewPrintDocument(printDocument);
                }
                else
                {
                    MessageBox.Show("Please select an order");
                }
            }
        }

        private void SaveCasesToOrderFromTextboxAndRemoveFromUI(TextBox textbox)
        {
            OrderProduct orderProduct;

            /*
             * Code Contract
             * TextBox must have text.
             * Text must have changed.
             */
            if (textbox.Text.Length > 0 &&
                !(textbox.Tag as TagProperties).OriginalText.Equals(textbox.Text))
            {
                // save the amount to order            
                orderProduct = new OrderProduct()
                {
                    OrderID = new Guid(this.LvOrder.SelectedItems[0].Tag.ToString()),
                    ProductUPC = this.LvOrderProduct.SelectedItems[0].Tag.ToString(),
                    CasesToOrder = Convert.ToInt32(textbox.Text)
                };
                orderProduct.UpsertIntoDB(new Database());

                // update the UI - this is a performance hit :-(                
                this.UpdateListViewOrderProduct();
            }

            textbox.Parent.Controls.Remove(textbox);
        }

        private void UpdateListBoxProduct()
        {
            ListBox listBox;
            ListViewItem targetListViewItem;
            string currentVendor;

            listBox = FormHelper.GetControlsByName<ListBox>(this.mainContent, UserInputs.LbSelect, true).FirstOrDefault<ListBox>();

            if (this.LvVendor != null)
            {
                targetListViewItem = GetSelectedListViewItem(this.LvVendor);
                currentVendor = targetListViewItem != null ? targetListViewItem.Text : null;
                if (listBox != null && listBox.Items != null)
                {
                    listBox.Items.Clear();
                    this.AddDataToListBoxProduct(listBox, currentVendor);
                }
            }
        }

        private List<Product> FilterProductListOnVendorName(List<Product> products, string vendorName)
        {
            List<Product> filteredCopy;
            filteredCopy = products.ToList<Product>();
            if (vendorName != null && vendorName.Length > 0)
            {
                filteredCopy.RemoveAll(p => !p.VendorName.Equals(vendorName));
            }

            return filteredCopy;
        }

        private void AddDataToListBoxProduct(ListBox listBoxProduct, string vendorName)
        {
            List<Product> filteredProducts;
            filteredProducts = this.FilterProductListOnVendorName((new Database()).Products, vendorName);
            foreach (Product product in filteredProducts)
            {
                listBoxProduct.Items.Add(product);
            }
        }

        private void AddListBoxProduct()
        {
            ListBox listBox;
            string currentVendor;

            listBox = FormHelper.GetControlsByName<ListBox>(this.mainContent, UserInputs.LbSelect, true).FirstOrDefault<ListBox>();

            if (listBox == null)
            {
                listBox = new ListBox();
                listBox.Dock = DockStyle.Right;
                listBox.Name = UserInputs.LbSelect;
                listBox.DoubleClick += new EventHandler(this.ListBox_DoubleClick_AddProductToOrder);

                if (this.LvVendor != null)
                {
                    currentVendor = this.LvVendor.SelectedItems[0] != null ? this.LvVendor.FocusedItem.Text : null;
                    this.AddDataToListBoxProduct(listBox, currentVendor);
                    this.mainContent.Controls.Add(listBox);
                }
            }
            else
            {
                this.mainContent.Controls.Remove(listBox);
            }
        }

        private ListView UpdateListViewOrderProduct()
        {
            ListViewItem selectedListViewVendorItem;
            Guid orderID;
            string vendorName;

            // clear            
            this.LvOrderProduct.Items.Clear();

            // defaults            
            vendorName = null;

            // retrieve selected orderID            
            orderID = new Guid(GetSelectedListViewItem(this.LvOrder).Tag.ToString());

            if (orderID != null)
            {
                // retrieve selected vendorName
                selectedListViewVendorItem = GetSelectedListViewItem(this.LvVendor);
                if (selectedListViewVendorItem != null)
                {
                    vendorName = selectedListViewVendorItem.Text;
                }
                          

                // update listViewProduct 
                this.AddDataToListViewOrderProduct(this.LvOrderProduct, orderID, vendorName);
            }

            // back
            this.EnableBackButton(this.LoadOrdersView);

            return this.LvOrderProduct;
        }

        private void AddItemToListViewOrderProduct(Product product, int casesToOrder, ListView listViewOrderProduct)
        {
            ListViewItem listViewItem;
            if (product != null)
            {
                listViewItem = new ListViewItem(product.Name);
                listViewItem.Tag = product.UPC;
                listViewItem.Name = product.UPC;
                listViewItem.SubItems.Add(casesToOrder.ToString());
                listViewOrderProduct.Items.Add(listViewItem);
            }
        }

        private void AddDataToListViewOrderProduct(ListView listViewOrderProduct, Guid orderID, string vendorName)
        {
            Database db;
            Product product;
            List<Product> filteredProducts;

            filteredProducts = this.FilterProductListOnVendorName((new Database()).Products, vendorName);
            if (orderID != Guid.Empty)
            {
                db = new Database();
                foreach (OrderProduct orderProduct in db.OrderProducts.Where(i => i.OrderID == orderID))
                {
                    product = filteredProducts.FirstOrDefault(p => p.UPC == orderProduct.ProductUPC);
                    this.AddItemToListViewOrderProduct(product, orderProduct.CasesToOrder, listViewOrderProduct);
                }
            }
        }

        private ListView CreateListViewOrderProduct()
        {
            ListView listViewProduct;

            listViewProduct = new ListView()
            {
                Name = UserInputs.LvOrderProduct,
                View = View.Details,
                HideSelection = false,
                Activation = ItemActivation.Standard,
                FullRowSelect = true,
                MultiSelect = false
            };

            // columns are required in View.Details
            listViewProduct.Columns.Add("Product");
            listViewProduct.Columns.Add("Cases to Order");

            listViewProduct.ItemActivate += new EventHandler(this.ListViewOrderProduct_ItemActivate_EditCasesToOrder);

            // return 
            return listViewProduct;
        }

        private ListView UpdateListViewVendor()
        {
            ListViewItem selectedOrder;
            Guid orderID;

            this.LvVendor.Items.Clear();

            selectedOrder = GetSelectedListViewItem(this.LvOrder);
            if (selectedOrder != null)
            {
                orderID = new Guid(selectedOrder.Tag.ToString());
                this.AddDataToListViewVendor(this.LvVendor, orderID);
            }

            // back
            this.EnableBackButton(this.LoadOrdersView);

            return this.LvVendor;
        }

        private void AddDataToListViewVendor(ListView listViewVendor, Guid orderID)
        {
            ListViewItem listViewItem;
            Database db;

            db = new Database();

            var vendorNames =
                from p in db.Products
                join op in db.OrderProducts on p.UPC equals op.ProductUPC
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

            listViewVendor.SelectedIndexChanged += new EventHandler(this.ListViewVendor_SelectedIndexChanged);

            return listViewVendor;
        }

        private ListView UpdateListViewOrder()
        {
            this.LvOrder.Items.Clear();
            this.AddDataToListViewOrder(this.LvOrder);
            return this.LvOrder;
        }

        private void AddDataToListViewOrder(ListView listViewOrder)
        {
            ListViewItem listViewItem;
            Database db;

            // add list view items            
            db = new Database();
            foreach (Order order in db.Orders)
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
            listViewOrder.SelectedIndexChanged += new EventHandler(this.ListViewOrder_SelectedIndexChanged);

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
            listViewProduct = this.CreateListViewOrderProduct();

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

                lv.GotFocus += new EventHandler(ListViewAny_GotFocus);                                

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
            int deletedItemIndex;
            ListView updatedListView;

            switch ((sender as Button).Tag.ToString())
            {
                case UserInputs.LvVendor:
                    MessageBox.Show("You cannot directly delete vendors. Instead, delete all associated vendor products.");
                    break;

                case UserInputs.LvOrderProduct:
                    deletedItemIndex = this.DeleteSelectedOrderProduct();
                    updatedListView = this.UpdateListViewOrderProduct();
                    if (updatedListView.Items.Count > 0)
                    {
                        updatedListView.SelectedIndices.Clear();
                        updatedListView.SelectedIndices.Add(0);
                    }

                    if (updatedListView.Items.Count == 0)
                    {
                        if (this.UpdateListViewVendor().Items.Count == 0)
                        {
                            this.UpdateListViewOrder();
                        }
                    }

                    break;

                case UserInputs.LvOrder:
                    this.DeleteSelectedOrder();
                    this.UpdateListViewOrder();
                    this.UpdateListViewVendor();
                    break;
            }
        }

        private void ListBox_DoubleClick_AddProductToOrder(object sender, EventArgs e)
        {
            ListViewItem listViewItemVendor;
            ListViewItem listViewItemOrderProduct;
            ListBox listBox;
            Product product;
            OrderProduct orderProduct;

            if (sender != null && sender is ListBox)
            {
                listBox = sender as ListBox;
                if (listBox.SelectedItem != null && listBox.SelectedItem is Product)
                {
                    if (this.LvOrder.SelectedItems[0] != null)
                    {
                        product = listBox.SelectedItem as Product;

                        if (this.LvOrderProduct.Items.ContainsKey(product.UPC))
                        {
                            // do not add it but do focus on it
                            listViewItemOrderProduct = this.LvOrderProduct.Items.Find(product.UPC, false)[0];
                        }
                        else
                        {
                            // save
                            orderProduct = new OrderProduct()
                            {
                                OrderID = new Guid(this.LvOrder.SelectedItems[0].Tag.ToString()),
                                ProductUPC = product.UPC,
                                CasesToOrder = Constants.DefaultCasesToOrder
                            };
                            orderProduct.UpsertIntoDB(new Database());
                        }

                        // update ui                        
                        if (this.UpdateListViewVendor().Items.Count > 0)
                        {
                            listViewItemVendor = this.LvVendor.Items.Find(product.VendorName, false)[0];
                            this.LvVendor.SelectedItems.Clear();
                            listViewItemVendor.Selected = true;
                            listViewItemVendor.Focused = true;
                        }

                        // keep updating ui                        
                        if (this.UpdateListViewOrderProduct().Items.Count > 0)
                        {
                            listViewItemOrderProduct = this.LvOrderProduct.Items.Find(product.UPC, false)[0];
                            this.LvOrderProduct.SelectedItems.Clear();
                            listViewItemOrderProduct.Selected = true;
                            listViewItemOrderProduct.Focused = true;
                        }
                    }
                }
            }
        }

        private void Textbox_KeyPress_WhiteList(object sender, KeyPressEventArgs e)
        {
            // use a whitelist approach by disallowing all input
            e.Handled = true;

            if (FormHelper.KeyPressIsControlKey(e))
            {
                e.Handled = false;
            }

            if (FormHelper.KeyPressIsDigit(e) && FormHelper.TextboxValueIsInt32(sender, e))
            {
                e.Handled = false;
            }

            // save on return
            if (e.KeyChar == (char)Keys.Return)
            {
                // cause the textbox to lose focus thereby triggering its LostFocus event
                this.LvOrderProduct.Focus();
            }
        }        

        private void TextboxCasesToOrder_LostFocus_SaveChanges(object sender, EventArgs e)
        {
            this.SaveCasesToOrderFromTextboxAndRemoveFromUI(sender as TextBox);
        }

        private void ListViewAny_GotFocus(object sender, EventArgs e)
        {
            this.ButtonMessage_Generic(
                FormHelper.GetControlsByName<Button>(this, UserInputs.BtnDelete, true).FirstOrDefault(),
                (sender as ListView).Name.Replace("Lv", string.Empty),
                (sender as ListView).Name);
        }

        private void ListViewOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateListViewVendor();
            this.UpdateListViewOrderProduct();
            this.UpdateListBoxProduct();
        }

        private void ListViewVendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateListViewOrderProduct();
            this.UpdateListBoxProduct();
        }

        private void ListViewOrderProduct_ItemActivate_EditCasesToOrder(object sender, EventArgs e)
        {
            ListView listViewProduct;
            ListViewItem listViewItemProduct;

            listViewProduct = sender as ListView;
            listViewItemProduct = listViewProduct != null ? listViewProduct.SelectedItems[0] : null;
            if (listViewItemProduct != null)
            {
                this.EditOrderProductCasesToOrder(listViewProduct, listViewItemProduct);
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

        private void PrintDocument_PrintOrderForSpecificVendor(object sender, PrintPageEventArgs e)
        {
            if (this.LvVendor != null && this.LvVendor.SelectedItems != null && this.LvVendor.SelectedItems.Count != 0)
            {
                this.AddSelectedVendorOrderToThePrintDocument(e);
            }
        }        
        
        private void PrintDocument_PrintOrderForAllVendors(object sender, PrintPageEventArgs e)
        {
            // ensure that LvVendor has items
            if (this.LvVendor != null && this.LvVendor.Items != null && this.LvVendor.Items.Count > 0)
            {
                // select the next list view item to print
                if (this.listViewItemIndexToPrintNext < this.LvVendor.Items.Count)
                {
                    this.LvVendor.Items[this.listViewItemIndexToPrintNext].Selected = true;
                }

                // Print the next vendor's order
                this.AddSelectedVendorOrderToThePrintDocument(e);

                // increment the list view item to print next
                this.listViewItemIndexToPrintNext++;

                // keeping printing?
                if (this.listViewItemIndexToPrintNext < this.LvVendor.Items.Count)
                {
                    // keep print
                    e.HasMorePages = true;
                }
                else
                {
                    // reset
                    this.listViewItemIndexToPrintNext = 0;
                }
            }
        }

        #endregion

        private class UserInputs
        {
            internal const string ListViewPrefix = "Lv";
            internal const string ButtonPrefix = "Btn";
            internal const string ListBoxPrefix = "Lb";

            internal const string LvOrder = "LvOrder";
            internal const string LvVendor = "LvVendor";
            internal const string LvOrderProduct = "LvOrderProduct";

            internal const string BtnCreate = "BtnCreate";
            internal const string BtnDelete = "BtnDelete";

            internal const string LbSelect = "LbSelect";
        }

        private class TagProperties
        {
            public string OriginalText { get; set; }
        }
    }
}
