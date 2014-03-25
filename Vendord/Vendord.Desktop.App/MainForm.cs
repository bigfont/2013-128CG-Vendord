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
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using BrightIdeasSoftware;
    using Vendord.SmartDevice.Linked;
    using System.Windows.Threading;

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
        private const string DefaultStatusLabelText = "Status";
        private const int DefaultStatusMin = 0;
        private const int DefaultStatusMax = 1;
        private const int DefaultStatusValue = 1;
 
        private static class Colors
        {
            internal static Color AllowDrop = Color.Yellow;
            internal static Color DragLeave = Color.Yellow;
            internal static Color DragEnter = Color.YellowGreen;
        }

        private Dispatcher UiDispatcher = Dispatcher.CurrentDispatcher;

        PrintPreviewDialog printPreview;
        PrintDocument printDocument;

        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripProgressBar progressBar;
        private Panel mainNavigation;
        private Panel mainContent;

        private BackgroundWorker importXmlBackgroundWorker;
        private int totalRecords;
        private int insertedRecords;

        private BackgroundWorker showProductListBackgroundWorker;
        private BackgroundWorker syncHandheldBackgroundWorker;

        private int listViewItemIndexToPrintNext = 0;
        private Font myFont = new Font(FontFamily.GenericSerif, 12.0F);
        private Brush myFontBrush = Brushes.Black;

        private Button btnBack;

        private Back backDelegate;        

        private Save saveDelegate; // TODO Assign to saveDelegate when we have something to save on the desktop

        public MainForm()
        {
            Application.EnableVisualStyles();

            Control[] controls;

            this.Load += new EventHandler(this.MainForm_Load);
            this.Closing += new CancelEventHandler(this.MainForm_Closing);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.MinimumSize = new Size(FormWidthMinimum, FormHeightMinimum);
            this.BackColor = Color.White;

            // create background worker and it's progress reported            
            this.importXmlBackgroundWorker = new BackgroundWorker();
            this.importXmlBackgroundWorker.WorkerReportsProgress = true;
            this.importXmlBackgroundWorker.DoWork
                += new DoWorkEventHandler(this.ImportXmlBackgroundWorker_DoWork);
            this.importXmlBackgroundWorker.ProgressChanged
                += new ProgressChangedEventHandler(this.ImportXmlBackgroundWorker_ProgressChanged);
            this.importXmlBackgroundWorker.RunWorkerCompleted
                += new RunWorkerCompletedEventHandler(this.ImportXmlBackgroundWorker_RunWorkerCompleted);

            // add a status strip
            this.statusStrip = new StatusStrip();
            this.statusStrip.Dock = DockStyle.Top;
            this.statusStrip.SizingGrip = false;
            this.statusStrip.BackColor = Color.Transparent;
            this.statusLabel = new ToolStripStatusLabel(DefaultStatusLabelText);
            this.progressBar = new ToolStripProgressBar() { AutoSize = false, Dock = DockStyle.Fill, Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 0 };
            this.statusStrip.Items.AddRange(new ToolStripItem[] { this.statusLabel, this.progressBar });

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
            controls = new Control[] { this.mainContent, this.mainNavigation, this.statusStrip };
            foreach (Control c in controls)
            {
                this.Controls.Add(c);
            }

            this.ResumeLayout();

            // Create Buttons
            Button btnClose;

            this.btnBack = this.ButtonFactory("Back");
            this.btnBack.Click += new EventHandler(this.BtnBack_Click);

            btnClose = this.ButtonFactory("Save and Close");
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
            get
            {
                ListView result = null;
                List<ListView> listViews
                    = FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvOrder, false);
                if (listViews != null && listViews.Count > 0)
                {
                    result = listViews[0];
                }
                return result;
            }
        }

        private ListView LvVendor
        {
            get
            {
                ListView result = null;
                List<ListView> listViews
                    = FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvVendor, false);
                if (listViews != null && listViews.Count > 0)
                {
                    result = listViews[0];
                }
                return result;
            }
        }

        private ListView LvOrderProduct
        {
            get { return FormHelper.GetControlsByName<ListView>(this.mainContent, UserInputs.LvOrderProduct, false)[0]; }
        }

        #region Utilities

        private void StopStatusStrip(string message)
        {
            this.Enabled = true;
            this.statusLabel.Text = message == null || message.Length == 0 ? DefaultStatusLabelText : message;
            this.progressBar.Style = ProgressBarStyle.Continuous;
            this.progressBar.MarqueeAnimationSpeed = 0;
            this.progressBar.Value = 0;
        }

        private void StartOrContinueStatusStrip(string message)
        {
            this.Enabled = false;
            this.statusLabel.Text = message ?? DefaultStatusLabelText;
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 30;
        }

        private Button ButtonFactory(string text)
        {
            Button b = new Button();
            b.Text = text;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private ListViewItem SelectedListViewItem(ListView listView)
        {
            ListViewItem result;
            if (listView != null && listView.SelectedItems.Count != 0)
            {
                result = listView.SelectedItems[0];
            }
            else
            {
                result = null;
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

            selectedListViewOrderItem = this.SelectedListViewItem(this.LvOrder);
            selectedListViewOrderProductItem = this.SelectedListViewItem(this.LvOrderProduct);

            if (selectedListViewOrderItem != null && selectedListViewOrderItem.Tag != null &&
                selectedListViewOrderProductItem != null && selectedListViewOrderProductItem.Tag != null)
            {
                Database db = new Database();
                DbQueryExecutor queryExe = new DbQueryExecutor(db.ConnectionString);

                orderProduct = new OrderProduct(queryExe)
                {
                    OrderID = new Guid(selectedListViewOrderItem.Tag.ToString()),
                    ProductUPC = selectedListViewOrderProductItem.Tag.ToString()
                };
                orderProduct.AddToTrash();
            }

            return selectedListViewOrderProductItem.Index;
        }

        private void DeleteSelectedOrder()
        {
            Database db = new Database();
            DbQueryExecutor queryExe = new DbQueryExecutor(db.ConnectionString);

            Order order = new Order(queryExe)
            {
                Id = new Guid(this.SelectedListViewItem(this.LvOrder).Tag.ToString())
            };
            order.AddToTrash();
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

        #region Print

        private void PreviewPrintDocument()
        {
            // preview the document                                
            this.printPreview = new PrintPreviewDialog()
            {
                Document = printDocument,
                UseAntiAlias = true
            };
            this.printPreview.PrintPreviewControl.Zoom = PrintPreviewZoom;

            // maximize if PrintPreviewDialog can act as a Form
            if (this.printPreview is Form)
            {
                (this.printPreview as Form).WindowState = FormWindowState.Maximized;
            }

            this.printPreview.ShowDialog();
        }

        private void NewLine(PrintPageEventArgs e, Font myFont, ref Point myPoint)
        {
            myPoint.X = e.MarginBounds.Left;
            myPoint.Y += myFont.Height;
        }

        private void EndColumn(PrintPageEventArgs e, int columnWidth, ref Point myPoint)
        {
            myPoint.X += columnWidth;
        }

        private void AddTheListView(PrintPageEventArgs e, ref Point myPoint)
        {
            // columns
            string productUpc;
            string productName;
            string casesToOrder;

            int upcColumnWidth = Convert.ToInt16(e.MarginBounds.Width * 0.25);
            int nameColumnWidth = Convert.ToInt16(e.MarginBounds.Width * 0.60);
            int casesToOrderColumnWidth = Convert.ToInt16(e.MarginBounds.Width * 0.15);

            // add some spaces
            this.NewLine(e, this.myFont, ref myPoint);
            this.NewLine(e, this.myFont, ref myPoint);
            this.NewLine(e, this.myFont, ref myPoint);

            e.Graphics.DrawString("Upc", this.myFont, this.myFontBrush, myPoint);
            this.EndColumn(e, upcColumnWidth, ref myPoint);

            e.Graphics.DrawString("Name", this.myFont, this.myFontBrush, myPoint);
            this.EndColumn(e, nameColumnWidth, ref myPoint);

            e.Graphics.DrawString("Cases", this.myFont, this.myFontBrush, myPoint);
            // no need to end the column

            // add the rows
            foreach (ListViewItem item in this.LvOrderProduct.Items)
            {
                // new line
                this.NewLine(e, this.myFont, ref myPoint);
                productUpc = item.Tag.ToString();
                e.Graphics.DrawString(productUpc, this.myFont, this.myFontBrush, myPoint);
                this.EndColumn(e, upcColumnWidth, ref myPoint);

                productName = item.Text;
                e.Graphics.DrawString(productName, this.myFont, this.myFontBrush, myPoint);
                this.EndColumn(e, nameColumnWidth, ref myPoint);

                casesToOrder = item.SubItems[1].Text.ToString(); // HACK - Magic Number.
                e.Graphics.DrawString(casesToOrder, this.myFont, this.myFontBrush, myPoint);
                // no need to end the column
            }
        }

        private void AddTheHeader(PrintPageEventArgs e, ref Point myPoint)
        {
            // declare fields
            string orderFor;
            string dept;
            string vendor;
            string orderCreated;
            int columnWidth = e.MarginBounds.Width / 2;

            // instantiate fields
            orderFor = "Order For: Country Grocer Salt Spring";
            dept = "Dept: TODO - Add dept";
            vendor = "Vendor: ";
            if (this.SelectedListViewItem(this.LvVendor) != null)
            {
                vendor += this.SelectedListViewItem(this.LvVendor).Text;
            }

            orderCreated = "Order Created: " + DateTime.Now.ToLongDateString();

            // draw strings
            e.Graphics.DrawString(orderFor, this.myFont, this.myFontBrush, myPoint);
            this.EndColumn(e, columnWidth, ref myPoint);

            e.Graphics.DrawString(dept, this.myFont, this.myFontBrush, myPoint);
            this.NewLine(e, this.myFont, ref myPoint);

            e.Graphics.DrawString(vendor, this.myFont, this.myFontBrush, myPoint);
            this.EndColumn(e, columnWidth, ref myPoint);

            e.Graphics.DrawString(orderCreated, this.myFont, this.myFontBrush, myPoint);
        }

        private void AddSelectedVendorOrderToThePrintDocument(PrintPageEventArgs e)
        {
            Point myPoint;
            myPoint = new Point(e.MarginBounds.Left, e.MarginBounds.Top);
            this.AddTheHeader(e, ref myPoint);
            this.AddTheListView(e, ref myPoint);
        }

        private void PrintSelectedOrder()
        {
            if (this.LvOrder != null && this.LvOrderProduct != null & this.LvVendor != null)
            {
                if (this.SelectedListViewItem(this.LvOrder) == null || this.LvVendor.Items == null)
                {
                    MessageBox.Show("Please select an order");
                }
                else if (this.LvVendor.Items.Count == 0)
                {
                    MessageBox.Show("Please select an order that has items.");
                }
                else
                {
                    printDocument = new PrintDocument();
                    printDocument.BeginPrint += new PrintEventHandler(this.PrintDocument_BeginPrint);

                    if (this.LvVendor.SelectedItems.Count == 0)
                    {
                        printDocument.PrintPage += new PrintPageEventHandler(this.PrintDocument_PrintOrderForAllVendors);
                    }
                    else
                    {
                        printDocument.PrintPage += new PrintPageEventHandler(this.PrintDocument_PrintOrderForSpecificVendor);
                    }

                    PrintDialog printDialog = new PrintDialog();
                    printDialog.Document = printDocument;
                    printDialog.UseEXDialog = true;
                    DialogResult result = printDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        this.PreviewPrintDocument();
                    }
                }
            }
        }

        #endregion

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
                Database db = new Database();
                DbQueryExecutor queryExe = new DbQueryExecutor(db.ConnectionString);

                orderProduct = new OrderProduct(queryExe)
                {
                    OrderID = new Guid(this.SelectedListViewItem(this.LvOrder).Tag.ToString()),
                    ProductUPC = this.SelectedListViewItem(this.LvOrderProduct).Tag.ToString(),
                    CasesToOrder = Convert.ToInt32(textbox.Text)
                };
                orderProduct.UpsertIntoDb();

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
                // get current vendor name
                string vendorName = null;
                if (this.SelectedListViewItem(this.LvVendor) != null)
                {
                    // the user has selected a vendor
                    vendorName = this.SelectedListViewItem(this.LvVendor).Text;
                }

                targetListViewItem = this.SelectedListViewItem(this.LvVendor);
                currentVendor = targetListViewItem != null ? targetListViewItem.Text : null;
                if (listBox != null && listBox.Items != null)
                {
                    listBox.Items.Clear();
                    this.AddDataToListBoxProduct(listBox, vendorName);
                }
            }
        }

        private List<Product> FilterProductListOnVendorName(List<Product> products, string vendorName)
        {
            List<Product> filteredCopy;
            filteredCopy = products.ToList<Product>();
            if (vendorName != null && vendorName.Length > 0)
            {
                filteredCopy.RemoveAll(p => !p.Vendor.Name.Equals(vendorName));
            }

            return filteredCopy;
        }

        private void SizeListBoxProduct(ListBox listBox)
        {
            Graphics graphics = this.CreateGraphics();
            var maxItemWidth = (
                from object item in listBox.Items
                select graphics.MeasureString(item.ToString(), this.Font)
                    into mySize
                    select (int)mySize.Width).Concat(new[] { 0 }).Max();

            listBox.Width = maxItemWidth + (SystemInformation.VerticalScrollBarWidth * 3);
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

        private ListBox CreateListBoxProduct()
        {
            // create the list box
            ListBox listBox = new ListBox { Dock = DockStyle.Right, Name = UserInputs.LbSelect };
            listBox.DoubleClick += new EventHandler(this.ListBox_DoubleClick_AddProductToOrder);

            return listBox;
        }

        private ListView UpdateListViewOrderProduct()
        {
            ListViewItem selectedListViewOrderItem;
            ListViewItem selectedListViewVendorItem;
            Guid orderID;
            string vendorName;

            // clear            
            this.LvOrderProduct.Items.Clear();

            // defaults            
            vendorName = null;

            // retrieve selected orderID
            selectedListViewOrderItem = this.SelectedListViewItem(this.LvOrder);
            if (selectedListViewOrderItem != null)
            {
                orderID = new Guid(selectedListViewOrderItem.Tag.ToString());
                if (orderID != null)
                {
                    // retrieve selected vendorName
                    selectedListViewVendorItem = this.SelectedListViewItem(this.LvVendor);
                    if (selectedListViewVendorItem != null)
                    {
                        vendorName = selectedListViewVendorItem.Text;
                    }

                    // update listViewProduct 
                    this.AddDataToListViewOrderProduct(this.LvOrderProduct, orderID, vendorName);
                }
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
                listViewItem.Tag = product.Upc;
                listViewItem.Name = product.Upc;
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
                    product = filteredProducts.FirstOrDefault(p => p.Upc == orderProduct.ProductUPC);
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

            selectedOrder = this.SelectedListViewItem(this.LvOrder);
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
                join op in db.OrderProducts on p.Upc equals op.ProductUPC
                join v in db.Vendors on p.Vendor.Id equals v.Id
                where op.OrderID == orderID
                group v by v.Name into g
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
                listViewItem.Tag = order.Id;
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

        private void LoadOrdersView()
        {            
            // create buttons
            Button btnSyncHandheld = this.ButtonFactory("Sync Handheld (before and after Scanning)");
            btnSyncHandheld.Click += new EventHandler(this.BtnSyncHandheld_Click);

            Button btnViewOrders = this.ButtonFactory("View Orders");
            btnViewOrders.Click += new EventHandler(this.BtnViewOrders_Click);

            Button[] buttons = new Button[] 
            { 
                btnViewOrders,
                btnSyncHandheld
            };

            foreach (Button b in buttons)
            {
                b.Dock = DockStyle.Top;
                b.Height = ButtonHeight;
            }

            // create upload labels                 
            Label lblProductUpload = new Label();
            lblProductUpload.Text = "Drop Product List Here";

            // enable drag and drop
            lblProductUpload.DragEnter += new DragEventHandler(this.LblProductUpload_DragEnter);
            lblProductUpload.DragDrop += new DragEventHandler(this.LblProductUpload_DragDrop);

            Label lblVendorUpload = new Label();
            lblVendorUpload.Text = "Drop Vendor List Here";

            Label[] labels = new Label[]
            {
                lblVendorUpload, 
                lblProductUpload
            };            

            foreach (Label l in labels)
            {
                l.TextAlign = ContentAlignment.MiddleCenter;
                l.Dock = DockStyle.Fill;
                l.AllowDrop = true;
                l.BackColor = Colors.AllowDrop;
                l.DragLeave += new EventHandler(this.Control_DragLeave);
                l.DragEnter += new DragEventHandler(this.Control_DragEnter);
            }

            TableLayoutPanel pnlDragAndDrop = new TableLayoutPanel();
            pnlDragAndDrop.Dock = DockStyle.Fill;
            pnlDragAndDrop.ColumnCount = 2;
            pnlDragAndDrop.RowCount = 1;
            pnlDragAndDrop.CellBorderStyle = TableLayoutPanelCellBorderStyle.InsetDouble;
            pnlDragAndDrop.CellPaint += new TableLayoutCellPaintEventHandler(TblLayoutPanel_CellPaint);
            for (int i = 0; i < pnlDragAndDrop.ColumnCount * pnlDragAndDrop.RowCount; ++i)
            {
                pnlDragAndDrop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            }
            pnlDragAndDrop.Controls.Add(lblVendorUpload, 0, 0);
            pnlDragAndDrop.Controls.Add(lblProductUpload, 1, 0);

            // add controls
            this.mainContent.Controls.Add(pnlDragAndDrop);
            this.mainContent.Controls.AddRange(buttons);

            // back
            this.DisableBackButton();
        }

        private void LoadCompleteOrdersView()
        {
            Button btnPrintOrder;
            Button btnShowProductList;
            Button btnDeleteItem;
            ListView listViewOrder;
            ListView listViewVendor;
            ListView listViewProduct;
            ListView[] listViews;

            // create button(s)
            btnPrintOrder = this.ButtonFactory("Print Current Order");
            btnPrintOrder.Click += new EventHandler(this.BtnPrintOrder_Click);
            btnShowProductList = this.ButtonFactory("Show Product List");
            btnShowProductList.Name = UserInputs.BtnShowProductList;
            btnShowProductList.Click += new EventHandler(this.BtnShowProductList_Click);
            btnDeleteItem = this.ButtonFactory("Delete Selected");
            btnDeleteItem.Name = UserInputs.BtnDelete;
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

                // enter occurs on single click, for instance, and probably other ways too.
                lv.Enter += new EventHandler(this.ListViewAny_Enter);

                this.mainContent.Controls.Add(lv);
            }

            // add button(s)            
            btnPrintOrder.Dock = DockStyle.Top;
            btnPrintOrder.Height = ButtonHeight;
            this.mainContent.Controls.Add(btnPrintOrder);

            btnDeleteItem.Dock = DockStyle.Top;
            btnDeleteItem.Height = ButtonHeight;
            this.mainContent.Controls.Add(btnDeleteItem);

            btnShowProductList.Dock = DockStyle.Top;
            btnShowProductList.Height = ButtonHeight;
            this.mainContent.Controls.Add(btnShowProductList);

            this.mainContent.ResumeLayout();

            // back
            this.EnableBackButton(this.LoadOrdersView);
        }

        #endregion

        #region Events

        private void TblLayoutPanel_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            // Add a border around each cell
            e.Graphics.DrawLine(Pens.Black, e.CellBounds.Location, new Point(e.CellBounds.Right, e.CellBounds.Top));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.LoadOrdersView();
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

        private void BtnShowProductList_Click(object sender, EventArgs e)
        {
            ListBox listBox
                = FormHelper.GetControlsByName<ListBox>(this.mainContent, UserInputs.LbSelect, true).FirstOrDefault<ListBox>();

            if (listBox == null)
            {
                // get current vendor name
                string vendorName = null;
                if (this.SelectedListViewItem(this.LvVendor) != null)
                {
                    // the user has selected a vendor
                    vendorName = this.SelectedListViewItem(this.LvVendor).Text;
                }

                this.showProductListBackgroundWorker = new BackgroundWorker();
                this.showProductListBackgroundWorker.DoWork += new DoWorkEventHandler(ShowProductListBackgroundWorker_DoWork);
                this.showProductListBackgroundWorker.WorkerReportsProgress = false;
                this.showProductListBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ShowProductListBackgroundWorker_RunWorkerCompleted);
                this.showProductListBackgroundWorker.RunWorkerAsync(vendorName);
            }
            else
            {
                this.mainContent.Controls.Remove(listBox);
            }
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

        private void BtnViewOrders_Click(object sender, EventArgs e)
        {
            this.UnloadCurrentView();
            this.LoadCompleteOrdersView();
        }

        private void BtnSyncHandheld_Click(object sender, EventArgs e)
        {
            this.syncHandheldBackgroundWorker = new BackgroundWorker();
            this.syncHandheldBackgroundWorker.DoWork += new DoWorkEventHandler(SyncHandheldBackgroundWorker_DoWork);
            this.syncHandheldBackgroundWorker.WorkerReportsProgress = false;
            this.syncHandheldBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SyncHandheldBackgroundWorker_RunWorkerCompleted);
            this.syncHandheldBackgroundWorker.RunWorkerAsync();
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
                    if (this.SelectedListViewItem(this.LvOrder) != null)
                    {
                        product = listBox.SelectedItem as Product;

                        if (this.LvOrderProduct.Items.ContainsKey(product.Upc))
                        {
                            // do not add it but do focus on it
                            listViewItemOrderProduct = this.LvOrderProduct.Items.Find(product.Upc, false)[0];
                        }
                        else
                        {
                            // save
                            Database db = new Database();
                            DbQueryExecutor queryExe = new DbQueryExecutor(db.ConnectionString);

                            orderProduct = new OrderProduct(queryExe)
                            {
                                OrderID = new Guid(this.SelectedListViewItem(this.LvOrder).Tag.ToString()),
                                ProductUPC = product.Upc,
                                CasesToOrder = Constants.DefaultCasesToOrder
                            };
                            orderProduct.UpsertIntoDb();
                        }

                        // update ui                        
                        if (this.UpdateListViewVendor().Items.Count > 0)
                        {
                            listViewItemVendor = this.LvVendor.Items.Find(product.Vendor.Name, false)[0];
                            this.LvVendor.SelectedItems.Clear();
                            listViewItemVendor.Selected = true;
                            listViewItemVendor.Focused = true;
                        }

                        // keep updating ui                        
                        if (this.UpdateListViewOrderProduct().Items.Count > 0)
                        {
                            listViewItemOrderProduct = this.LvOrderProduct.Items.Find(product.Upc, false)[0];
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

        private void ListViewAny_Enter(object sender, EventArgs e)
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
            ////this.UpdateListBoxProduct(); // this is really slow
        }

        private void ListViewVendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateListViewOrderProduct();
            ////this.UpdateListBoxProduct(); // this is really slow
        }

        private void ListViewOrderProduct_ItemActivate_EditCasesToOrder(object sender, EventArgs e)
        {
            ListView listViewProduct;
            ListViewItem listViewItemProduct;

            listViewProduct = sender as ListView;
            listViewItemProduct = listViewProduct != null ? this.SelectedListViewItem(listViewProduct) : null;
            if (listViewItemProduct != null)
            {
                this.EditOrderProductCasesToOrder(listViewProduct, listViewItemProduct);
            }
        }

        private void SyncHandheldBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UiDispatcher.BeginInvoke((Action)(() =>
            {
                this.StartOrContinueStatusStrip("Syncing handheld.");
            }));

            Sync sync = new Sync();
            e.Result = sync.MergeDesktopAndDeviceDatabases();
        }

        private void SyncHandheldBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // this seems to work fine
            this.StopStatusStrip(e.Result.ToString());
        }

        private void ShowProductListBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UiDispatcher.BeginInvoke((Action)(() =>
            {
                this.StartOrContinueStatusStrip("Loading products");
            }));

            ListBox listBox = this.CreateListBoxProduct();
            string vendorName = e.Argument != null ? e.Argument.ToString() : null;
            this.AddDataToListBoxProduct(listBox, vendorName);
            this.SizeListBoxProduct(listBox);

            UiDispatcher.Invoke((Action)(() =>
            {
                this.mainContent.Controls.Add(listBox);
            }));
        }

        private void ShowProductListBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.StopStatusStrip(null);
        }

        private void ImportXmlBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Sync sync = new Sync();
            sync.PullProductsFromItRetailDatabase(worker, e.Argument.ToString(), ref this.totalRecords, ref this.insertedRecords);
        }

        private void ImportXmlBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Updated {0}/{1} products - {2}% complete.", this.insertedRecords, this.totalRecords, e.ProgressPercentage);
            this.StartOrContinueStatusStrip(builder.ToString());
        }

        private void ImportXmlBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.StopStatusStrip("Import complete.");
        }

        private void Control_DragLeave(object sender, EventArgs e)
        {
            (sender as Control).BackColor = Colors.DragLeave;
        }

        private void Control_DragEnter(object sender, DragEventArgs e)
        {
            (sender as Control).BackColor = Colors.DragEnter;
        }

        private void LblProductUpload_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;

            }
        }

        private void LblProductUpload_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Ensure that there is one and only one file
            string errorMessage = null;
            if (files == null || files.Length == 0 || files[0] == null)
            {
                errorMessage = "There is no file to upload.";
            }
            else if (files.Length > 1)
            {
                errorMessage = "Please upload only one file.";
            }

            // Ensure that the file is an Excel file
            string filePath = files[0];
            string fileExtension = Path.GetExtension(filePath);
            if (!fileExtension.Equals(".xml"))
            {
                errorMessage = "Please upload only XML file types.";
            }

            // Ensure that the background work is free
            if (this.importXmlBackgroundWorker.IsBusy)
            {
                errorMessage = "The system is busy. Please try again in a few moments.";
            }

            // Run if appropriate; otherwise show an error message
            if (errorMessage == null)
            {
                this.importXmlBackgroundWorker.RunWorkerAsync(files[0]);
            }
            else
            {
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Print

        private void BtnPrintOrder_Click(object sender, EventArgs e)
        {
            this.PrintSelectedOrder();
        }

        private void PrintDocument_BeginPrint(object sender, PrintEventArgs e)
        {
            if (this.printPreview != null && !printDocument.PrintController.IsPreview)
            {
                this.printPreview.Close();
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

        #endregion

        private class UserInputs
        {
            internal const string ListViewPrefix = "Lv";
            internal const string ButtonPrefix = "Btn";
            internal const string ListBoxPrefix = "Lb";

            internal const string LvOrder = "LvOrder";
            internal const string LvVendor = "LvVendor";
            internal const string LvOrderProduct = "LvOrderProduct";

            internal const string BtnShowProductList = "BtnCreate";
            internal const string BtnDelete = "BtnDelete";

            internal const string LbSelect = "LbSelect";
        }

        private class TagProperties
        {
            public string OriginalText { get; set; }
        }
        
    }
}
