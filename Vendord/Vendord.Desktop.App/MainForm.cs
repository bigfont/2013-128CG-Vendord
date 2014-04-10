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
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Threading;
    using BrightIdeasSoftware;
    using Vendord.SmartDevice.Linked;

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

        private Dispatcher uiDispatcher = Dispatcher.CurrentDispatcher;

        private ListView listViewOrder;
        private ListView listViewVendor;
        private ListView listViewOrderProduct;
        private ListBox listBoxProduct;
        private Button btnDelete;

        private PrintPreviewDialog printPreview;
        private PrintDocument printDocument;

        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripProgressBar progressBar;
        private Panel mainNavigation;
        private Panel mainContent;

        private BackgroundWorker backgroundWorkerImportXml;
        private int totalRecords;
        private int insertedRecords;

        private BackgroundWorker backgroundWorkerShowProductList;
        private BackgroundWorker backgroundWorkerSyncProductsVendorsAndDepts;
        private BackgroundWorker backgroundWorkerSyncOrders;

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
            this.Text = "Country Grocer Salt Spring";

            // create background worker and it's progress reported            
            this.backgroundWorkerImportXml = new BackgroundWorker();
            this.backgroundWorkerImportXml.WorkerReportsProgress = true;
            this.backgroundWorkerImportXml.DoWork
                += new DoWorkEventHandler(this.BackgroundWorkerImportXml_DoWork);
            this.backgroundWorkerImportXml.ProgressChanged
                += new ProgressChangedEventHandler(this.BackgroundWorkerImportXml_ProgressChanged);
            this.backgroundWorkerImportXml.RunWorkerCompleted
                += new RunWorkerCompletedEventHandler(this.BackgroundWorkerImportXml_RunWorkerCompleted);

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
                if (this.listViewOrder == null)
                {
                    Control[] controls = this.Controls.Find(ControlNames.LvOrder, true);
                    if (controls != null && controls.Length > 0)
                    {
                        this.listViewOrder = controls[0] as ListView;
                    }
                }

                return this.listViewOrder;
            }

            set
            {
                this.listViewOrder = value;
            }
        }

        private ListView LvVendor
        {
            get
            {
                if (this.listViewVendor == null)
                {
                    Control[] controls = this.Controls.Find(ControlNames.LvVendor, true);
                    if (controls != null && controls.Length > 0)
                    {
                        this.listViewVendor = controls[0] as ListView;
                    }
                }

                return this.listViewVendor;
            }

            set
            {
                this.listViewVendor = value;
            }
        }

        private ListView LvOrderProduct
        {
            get
            {
                if (this.listViewOrderProduct == null)
                {
                    Control[] controls = this.Controls.Find(ControlNames.LvOrderProduct, true);
                    if (controls != null && controls.Length > 0)
                    {
                        this.listViewOrderProduct = controls[0] as ListView;
                    }
                }

                return this.listViewOrderProduct;
            }

            set
            {
                this.listViewOrderProduct = value;
            }
        }

        private ListBox LbProduct
        {
            get
            {
                if (this.listBoxProduct == null)
                {
                    Control[] controls = this.Controls.Find(ControlNames.LbProduct, true);
                    if (controls != null && controls.Length > 0)
                    {
                        this.listBoxProduct = controls[0] as ListBox;
                    }
                }

                return this.listBoxProduct;
            }

            set
            {
                this.listBoxProduct = value;
            }
        }

        private Button BtnDelete
        {
            get
            {
                if (this.btnDelete == null)
                {
                    Control[] controls = this.Controls.Find(ControlNames.BtnDelete, true);
                    if (controls != null && controls.Length > 0)
                    {
                        this.btnDelete = controls[0] as Button;
                    }
                }

                return this.btnDelete;
            }

            set
            {
                this.btnDelete = value;
            }
        }

        #region Utilities

        private bool IsValidXmlFileUpload(string[] files, out string errorMessage)
        {
            // Ensure that there is one and only one file
            errorMessage = null;
            bool isValid = true;
            if (files == null || files.Length == 0 || files[0] == null)
            {
                isValid = false;
                errorMessage = "There is no file to upload.";
            }
            else if (files.Length > 1)
            {
                isValid = false;
                errorMessage = "Please upload only one file.";
            }

            // Ensure that the file is an Excel file
            string filePath = files[0];
            string fileExtension = Path.GetExtension(filePath);
            if (!fileExtension.Equals(".xml"))
            {
                isValid = false;
                errorMessage = "Please upload only XML file types.";
            }

            // Ensure that the background work is free
            if (this.backgroundWorkerImportXml.IsBusy)
            {
                isValid = false;
                errorMessage = "The system is busy. Please try again in a few moments.";
            }

            return isValid;
        }

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
            b.BackColor = ThemeColors.Enabled;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;

            b.EnabledChanged += new EventHandler((sender, e) =>
            {
                Button s = sender as Button;
                s.BackColor = s.Enabled ? ThemeColors.Enabled : ThemeColors.Disabled;
            });
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

                long productUpc;
                string parseMe = selectedListViewOrderProductItem.Tag.ToString();
                long.TryParse(parseMe, out productUpc);

                orderProduct = new OrderProduct(queryExe)
                {
                    OrderID = new Guid(selectedListViewOrderItem.Tag.ToString()),
                    ProductUPC = productUpc
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

                long productUpc;
                string parseMe = this.SelectedListViewItem(this.LvOrderProduct).Tag.ToString();
                long.TryParse(parseMe, out productUpc);

                orderProduct = new OrderProduct(queryExe)
                {
                    OrderID = new Guid(this.SelectedListViewItem(this.LvOrder).Tag.ToString()),
                    ProductUPC = productUpc,
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
            ListViewItem targetListViewItem;
            string currentVendor;

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
                if (this.LbProduct != null && this.LbProduct.Items != null)
                {
                    this.LbProduct.Items.Clear();
                    this.AddDataToListBoxProduct(this.LbProduct, vendorName);
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
            Database db = new Database();
            DbQueryExecutor qe = new DbQueryExecutor(db.ConnectionString);
            Product p = new Product();
            p.QueryExecutor = qe;
            List<Product> products = p.SelectAllWithJoin();

            foreach (Product product in db.Products)
            {
                listBoxProduct.Items.Add(product);
            }
        }

        private ListBox CreateListBoxProduct()
        {
            // create the list box
            ListBox listBox = new ListBox { Dock = DockStyle.Right, Name = ControlNames.LbProduct };
            listBox.Margin = new Padding(25);
            listBox.DoubleClick += new EventHandler(this.ListBox_DoubleClick_AddProductToOrder);

            return listBox;
        }

        private ListView UpdateListViewOrderProduct()
        {
            // clear            
            this.LvOrderProduct.Items.Clear();

            // defaults            
            string vendorName = null;

            // retrieve selected orderID
            ListViewItem selectedListViewOrderItem = this.SelectedListViewItem(this.LvOrder);
            if (selectedListViewOrderItem != null)
            {
                Guid orderID = new Guid(selectedListViewOrderItem.Tag.ToString());
                if (orderID != null)
                {
                    // retrieve selected vendorName
                    ListViewItem selectedListViewVendorItem = this.SelectedListViewItem(this.LvVendor);
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
                listViewItem.Name = product.Upc.ToString();
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
                Name = ControlNames.LvOrderProduct,
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
            this.LvVendor.Items.Clear();

            ListViewItem selectedOrder = this.SelectedListViewItem(this.LvOrder);            
            if (selectedOrder != null)
            {
                Guid orderID = new Guid(selectedOrder.Tag.ToString());
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

            // slow query            
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
                Name = ControlNames.LvVendor,
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
                Name = ControlNames.LvOrder,
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

        #region Print Utilities

        private void PreviewPrintDocument()
        {
            // preview the document                                
            this.printPreview = new PrintPreviewDialog()
            {
                Document = this.printDocument,
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
            int upcColumnWidth = Convert.ToInt16(e.MarginBounds.Width * 0.25);
            int nameColumnWidth = Convert.ToInt16(e.MarginBounds.Width * 0.60);
            int casesToOrderColumnWidth = Convert.ToInt16(e.MarginBounds.Width * 0.15);

            // add some spaces
            this.NewLine(e, this.myFont, ref myPoint);
            this.NewLine(e, this.myFont, ref myPoint);
            this.NewLine(e, this.myFont, ref myPoint);

            e.Graphics.DrawString("Upc", this.myFont, this.myFontBrush, myPoint);
            this.EndColumn(e, upcColumnWidth, ref myPoint);

            e.Graphics.DrawString("Product Code", this.myFont, this.myFontBrush, myPoint);
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

                // upc
                var productUpc = item.Tag.ToString();
                e.Graphics.DrawString(productUpc, this.myFont, this.myFontBrush, myPoint);
                this.EndColumn(e, upcColumnWidth, ref myPoint);

                // product code
                var productCode = string.Empty; // todo
                e.Graphics.DrawString(productCode, this.myFont, this.myFontBrush, myPoint);
                this.EndColumn(e, upcColumnWidth, ref myPoint);

                // name
                var productName = item.Text;
                e.Graphics.DrawString(productName, this.myFont, this.myFontBrush, myPoint);
                this.EndColumn(e, nameColumnWidth, ref myPoint);

                // cases
                var casesToOrder = item.SubItems[1].Text.ToString(); // HACK - Magic Number.
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
                    this.printDocument = new PrintDocument();
                    this.printDocument.BeginPrint += new PrintEventHandler(this.PrintDocument_BeginPrint);

                    if (this.LvVendor.SelectedItems.Count == 0)
                    {
                        this.printDocument.PrintPage += new PrintPageEventHandler(this.PrintDocument_PrintOrderForAllVendors);
                    }
                    else
                    {
                        this.printDocument.PrintPage += new PrintPageEventHandler(this.PrintDocument_PrintOrderForSpecificVendor);
                    }

                    PrintDialog printDialog = new PrintDialog();
                    printDialog.Document = this.printDocument;
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

        #region Views

        private void UnloadCurrentView()
        {
            this.mainContent.Controls.Clear();
        }

        private void LoadOrdersView()
        {
            // create buttons
            Button btnSyncProductsVendorsAndDepts = this.ButtonFactory("Sync Products, Vendors, and Departments (after importing).");
            btnSyncProductsVendorsAndDepts.Click += new EventHandler(this.BtnSyncProductsVendorsAndDepts_Click);

            Button btnSyncOrders = this.ButtonFactory("Sync Orders (before and after ordering).");
            btnSyncOrders.Click += new EventHandler(this.BtnSyncOrders_Click);

            Button btnViewOrders = this.ButtonFactory("View Orders");
            btnViewOrders.Click += new EventHandler(this.BtnViewOrders_Click);

            Button[] buttons = new Button[] 
            { 
                btnViewOrders,
                btnSyncOrders,
                btnSyncProductsVendorsAndDepts
            };

            foreach (Button b in buttons)
            {
                b.Dock = DockStyle.Top;
                b.Height = ButtonHeight;
            }

            // create upload labels                 
            Label lblProductUpload = new Label();
            lblProductUpload.Text = "Drop Product List Here";
            lblProductUpload.DragEnter += new DragEventHandler(this.Control_DragEnter);
            lblProductUpload.DragDrop += new DragEventHandler(this.LblProductUpload_DragDrop);

            Label lblVendorUpload = new Label();
            lblVendorUpload.Text = "Drop Vendor List Here";
            lblVendorUpload.DragEnter += new DragEventHandler(this.Control_DragEnter);
            lblVendorUpload.DragDrop += new DragEventHandler(this.LblVendorUpload_DrapDrop);

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
                l.BackColor = ThemeColors.AllowDrop;
                l.DragLeave += new EventHandler(this.Control_DragLeave);
                l.DragEnter += new DragEventHandler(this.Control_DragEnter);
            }

            TableLayoutPanel pnlDragAndDrop = new TableLayoutPanel();
            pnlDragAndDrop.Dock = DockStyle.Fill;
            pnlDragAndDrop.ColumnCount = 2;
            pnlDragAndDrop.RowCount = 1;
            pnlDragAndDrop.CellBorderStyle = TableLayoutPanelCellBorderStyle.InsetDouble;
            pnlDragAndDrop.CellPaint += new TableLayoutCellPaintEventHandler(this.TblLayoutPanel_CellPaint);
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
            btnShowProductList.Name = ControlNames.BtnShowProductList;
            btnShowProductList.Click += new EventHandler(this.BtnShowProductList_Click);
            btnDeleteItem = this.ButtonFactory("Delete Selected");
            btnDeleteItem.Name = ControlNames.BtnDelete;
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
            }

            // add list views to panels
            // starting with panel products
            Button btnClearVendorFilter = this.ButtonFactory("Clear Vendor Filter");
            btnClearVendorFilter.Click += new EventHandler((sender, args) =>
            {
                this.LvVendor.SelectedIndices.Clear();
            });
            btnClearVendorFilter.Dock = DockStyle.Top;

            TableLayoutPanel pnlProducts = new TableLayoutPanel();
            pnlProducts.RowCount = 2;
            pnlProducts.ColumnCount = 1;
            pnlProducts.AutoSize = true;
            pnlProducts.Dock = DockStyle.Left;

            pnlProducts.Controls.Add(btnClearVendorFilter, 0, 0);
            pnlProducts.Controls.Add(listViewProduct, 0, 1);

            this.mainContent.Controls.Add(pnlProducts);

            TableLayoutPanel pnlVendors = new TableLayoutPanel();
            pnlVendors.RowCount = 1;
            pnlVendors.ColumnCount = 1;
            pnlVendors.AutoSize = true;
            pnlVendors.Dock = DockStyle.Left;
            pnlVendors.Controls.Add(listViewVendor);

            this.mainContent.Controls.Add(pnlVendors);

            TableLayoutPanel pnlOrders = new TableLayoutPanel();
            pnlOrders.RowCount = 1;
            pnlOrders.ColumnCount = 1;
            pnlOrders.AutoSize = true;
            pnlOrders.Dock = DockStyle.Left;
            pnlOrders.Controls.Add(listViewOrder);

            this.mainContent.Controls.Add(pnlOrders);

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
            if (this.LbProduct == null)
            {
                // get current vendor name
                string vendorName = null;
                if (this.SelectedListViewItem(this.LvVendor) != null)
                {
                    // the user has selected a vendor
                    vendorName = this.SelectedListViewItem(this.LvVendor).Text;
                }

                this.backgroundWorkerShowProductList = new BackgroundWorker();
                this.backgroundWorkerShowProductList.DoWork +=
                    new DoWorkEventHandler(this.BackgroundWorkerShowProductList_DoWork);
                this.backgroundWorkerShowProductList.WorkerReportsProgress = false;
                this.backgroundWorkerShowProductList.RunWorkerCompleted +=
                    new RunWorkerCompletedEventHandler(this.BackgroundWorkerShowProductList_RunWorkerCompleted);
                this.backgroundWorkerShowProductList.RunWorkerAsync(vendorName);
            }
            else
            {
                // show or hide
                this.LbProduct.Visible = !this.LbProduct.Visible;
            }
        }

        private void BtnDeleteItem_Click(object sender, EventArgs e)
        {
            int deletedItemIndex;
            ListView updatedListView;

            switch ((sender as Button).Tag.ToString())
            {
                case ControlNames.LvVendor:
                    MessageBox.Show("You cannot directly delete vendors. Instead, delete all associated vendor products.");
                    break;

                case ControlNames.LvOrderProduct:
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

                case ControlNames.LvOrder:

                    DialogResult yesNo = MessageBox.Show("Delete this entire order?", "Confirm delete!", MessageBoxButtons.YesNo);
                    if (yesNo == DialogResult.Yes)
                    {
                        this.DeleteSelectedOrder();
                        this.UpdateListViewOrder();
                        this.UpdateListViewVendor();
                        this.UpdateListViewOrderProduct();
                    }

                    break;
            }
        }

        private void BtnViewOrders_Click(object sender, EventArgs e)
        {
            this.UnloadCurrentView();
            this.LoadCompleteOrdersView();
        }

        private void BtnSyncOrders_Click(object sender, EventArgs e)
        {
            this.backgroundWorkerSyncOrders = new BackgroundWorker();
            this.backgroundWorkerSyncOrders.DoWork += 
                new DoWorkEventHandler(BackgroundWorkerSyncOrders_DoWork);
            this.backgroundWorkerSyncOrders.WorkerReportsProgress = false;
            this.backgroundWorkerSyncOrders.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(BackgroundWorkerSyncOrders_RunWorkerCompleted);
            this.backgroundWorkerSyncOrders.RunWorkerAsync();            
        }

        private void BtnSyncProductsVendorsAndDepts_Click(object sender, EventArgs e)
        {
            this.backgroundWorkerSyncProductsVendorsAndDepts = new BackgroundWorker();
            this.backgroundWorkerSyncProductsVendorsAndDepts.DoWork += 
                new DoWorkEventHandler(this.BackgroundWorkerSyncProductsVendorsAndDepts_DoWork);
            this.backgroundWorkerSyncProductsVendorsAndDepts.WorkerReportsProgress = false;
            this.backgroundWorkerSyncProductsVendorsAndDepts.RunWorkerCompleted += 
                new RunWorkerCompletedEventHandler(this.BackgroundWorkerSyncProductsVendorsAndDepts_RunWorkerCompleted);
            this.backgroundWorkerSyncProductsVendorsAndDepts.RunWorkerAsync();
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

                        if (this.LvOrderProduct.Items.ContainsKey(product.Upc.ToString()))
                        {
                            // do not add it but do focus on it
                            listViewItemOrderProduct = this.LvOrderProduct.Items.Find(product.Upc.ToString(), false)[0];
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
                            ListViewItem[] matchingItems = this.LvVendor.Items.Find(product.Vendor.Name, false);
                            if (matchingItems.Length > 0)
                            {
                                this.LvVendor.SelectedItems.Clear();
                                listViewItemVendor = matchingItems[0];
                                listViewItemVendor.Selected = true;
                                listViewItemVendor.Focused = true;
                            }
                        }

                        // keep updating ui                        
                        if (this.UpdateListViewOrderProduct().Items.Count > 0)
                        {
                            listViewItemOrderProduct = this.LvOrderProduct.Items.Find(product.Upc.ToString(), false)[0];
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
                this.BtnDelete,
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

        private void BackgroundWorkerSyncOrders_DoWork(object sender, DoWorkEventArgs e)
        {
            this.uiDispatcher.BeginInvoke((Action)(() =>
            {
                this.StartOrContinueStatusStrip("Syncing handheld.");
            }));

            Sync sync = new Sync();

            const string syncScopeName = "SyncOrders";
            e.Result = sync.SyncDesktopAndDeviceDatabases(syncScopeName);
        }

        private void BackgroundWorkerSyncOrders_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // this seems to work fine
            this.StopStatusStrip(e.Result.ToString());
        }

        private void BackgroundWorkerSyncProductsVendorsAndDepts_DoWork(object sender, DoWorkEventArgs e)
        {
            this.uiDispatcher.BeginInvoke((Action)(() =>
            {
                this.StartOrContinueStatusStrip("Syncing handheld.");
            }));

            Sync sync = new Sync();

            const string syncScopeName = "SyncProductsVendorsAndDepts";
            e.Result = sync.SyncDesktopAndDeviceDatabases(syncScopeName);
        }

        private void BackgroundWorkerSyncProductsVendorsAndDepts_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // this seems to work fine
            this.StopStatusStrip(e.Result.ToString());
        }

        private void BackgroundWorkerShowProductList_DoWork(object sender, DoWorkEventArgs e)
        {
            this.uiDispatcher.BeginInvoke((Action)(() =>
            {
                this.StartOrContinueStatusStrip("Loading products");
            }));

            ListBox listBox = this.CreateListBoxProduct();
            string vendorName = e.Argument != null ? e.Argument.ToString() : null;
            this.AddDataToListBoxProduct(listBox, vendorName);
            this.SizeListBoxProduct(listBox);

            this.uiDispatcher.Invoke((Action)(() =>
            {
                this.mainContent.Controls.Add(listBox);
            }));
        }

        private void BackgroundWorkerShowProductList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.StopStatusStrip(null);
        }

        private void BackgroundWorkerImportXml_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            ImportWorkerArgs args = e.Argument as ImportWorkerArgs;

            Sync sync = new Sync();
            Sync.SyncResult result;
            if (args.Importing == ImportWorkerArgs.ImportType.Product)
            {
                result = sync.PullProductsFromItRetailXmlBackup(worker, args.FilePath, ref this.totalRecords, ref this.insertedRecords);
            }
            else if (args.Importing == ImportWorkerArgs.ImportType.Vendor)
            {
                result = sync.PullVendorsFromItRetailXmlBackup(worker, args.FilePath, ref this.totalRecords, ref this.insertedRecords);
            }
        }

        private void BackgroundWorkerImportXml_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Updated {0}/{1} items - {2}% complete.", this.insertedRecords, this.totalRecords, e.ProgressPercentage);
            this.StartOrContinueStatusStrip(builder.ToString());
        }

        private void BackgroundWorkerImportXml_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.StopStatusStrip("Import complete.");
        }

        private void Control_DragLeave(object sender, EventArgs e)
        {
            (sender as Control).BackColor = ThemeColors.DragLeave;
        }

        private void Control_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                (sender as Control).BackColor = ThemeColors.DragEnter;
            }
        }

        private void LblVendorUpload_DrapDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            string errorMessage;
            if (this.IsValidXmlFileUpload(files, out errorMessage))
            {
                ImportWorkerArgs args = new ImportWorkerArgs()
                {
                    FilePath = files[0],
                    Importing = ImportWorkerArgs.ImportType.Vendor
                };
                this.backgroundWorkerImportXml.RunWorkerAsync(args);
            }
            else
            {
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            (sender as Control).BackColor = ThemeColors.AllowDrop;
        }

        private void LblProductUpload_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            string errorMessage;
            if (this.IsValidXmlFileUpload(files, out errorMessage))
            {
                ImportWorkerArgs args = new ImportWorkerArgs()
                {
                    FilePath = files[0],
                    Importing = ImportWorkerArgs.ImportType.Product
                };
                this.backgroundWorkerImportXml.RunWorkerAsync(args);
            }
            else
            {
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            (sender as Control).BackColor = ThemeColors.AllowDrop;
        }

        #endregion    
    
        #region Print Events

        private void BtnPrintOrder_Click(object sender, EventArgs e)
        {
            this.PrintSelectedOrder();
        }

        private void PrintDocument_BeginPrint(object sender, PrintEventArgs e)
        {
            if (this.printPreview != null && !this.printDocument.PrintController.IsPreview)
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

    }
}