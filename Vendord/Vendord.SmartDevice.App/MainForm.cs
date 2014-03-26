[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.SmartDevice.App
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Symbol.Barcode2;
    using Vendord.SmartDevice.Linked;

    public class MainForm : Form
    {
        private const int ButtonHeight = 50;
        private const int NumberOfNavigationButtons = 2;
        private const string TextBack = "Save and Back";
        private const string TextClose = "Save and Close";
        private const string TextContinue = "Save and Continue";

        private Panel mainNavigation;
        private Panel mainContent;

        private Button btnBack;

        private Back backDelegate;

        private Save saveDelegate;

        // scanning specific fields
        private BarcodeAPI barcodeAPI;

        private Order currentOrder = null;
        private Product currentProduct = null;

        public MainForm()
        {
            Control[] controls;

            this.Load += new EventHandler(this.MainForm_Load);
            this.Closing += new CancelEventHandler(this.MainForm_Closing);
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;

            // create main navigation panel           
            this.mainNavigation = new Panel();
            this.mainNavigation.Dock = DockStyle.Top;
            this.mainNavigation.Height = ButtonHeight;

            // create main content panel
            this.mainContent = new Panel();
            this.mainContent.Dock = DockStyle.Fill;

            // add to form           
            this.SuspendLayout();
            controls = new Control[] { this.mainContent, this.mainNavigation };
            foreach (Control c in controls)
            {
                this.Controls.Add(c);
            }

            this.ResumeLayout();

            // Create Buttons
            Button btnClose;

            this.btnBack = new Button() { Text = TextBack };
            this.btnBack.Click += new EventHandler(this.BtnBack_Click);

            btnClose = new Button() { Text = TextClose };
            btnClose.Click += new EventHandler(this.BtnClose_Click);

            // add to panel - this triggers its layout event
            controls = new Button[] { btnClose, this.btnBack };

            this.mainNavigation.SuspendLayout();

            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Left;
                c.Height = ButtonHeight;
                c.Width = this.mainNavigation.ClientSize.Width / controls.Length;
                this.mainNavigation.Controls.Add(c);
            }

            this.mainNavigation.ResumeLayout();
        }

        private delegate void Back();

        private delegate void Save();

        #region Utilities

        private void SetBackColorForAllChildControls<T>(Control controlToSearch, Color color, bool searchDescendents) where T : Control
        {
            foreach (T control in controlToSearch.Controls)
            {
                control.BackColor = color;
                if (searchDescendents)
                {
                    SetBackColorForAllChildControls<T>(control, color, true);
                }
            }
        }

        private void UpdateCurrentOrder()
        {
            ListView listView;
            Guid orderID;
            Database db;

            // reset the currentOrder to null
            this.currentOrder = null;

            // get the id of the selected order
            listView = FormHelper.GetControlsByType<ListView>(this, true).FirstOrDefault<ListView>();
            if (listView != null && listView.FocusedItem != null && listView.FocusedItem.SubItems.Count == 2)
            {
                orderID = new Guid(listView.FocusedItem.SubItems[1].Text);
                db = new Database();

                // update the currentOrder
                this.currentOrder = db.Orders.FirstOrDefault(o => o.Id == orderID);
            }
        }

        private void DeleteSelectedOrder()
        {
            if (this.currentOrder != null)
            {
                this.currentOrder.AddToTrash();
                this.currentOrder = null;
            }
        }

        private void SaveNewOrder()
        {
            List<TextBox> textBoxes;

            textBoxes = FormHelper.GetControlsByName<TextBox>(this, USER_INPUTS.TxtOrderName, true);
            if (textBoxes != null && textBoxes.Count > 0 && textBoxes.FirstOrDefault().Text.Length > 0)
            {
                Database db = new Database();
                DbQueryExecutor queryExe = new DbQueryExecutor(db.ConnectionString);

                Order newOrder = new Order(queryExe)
                {
                    Name = textBoxes.FirstOrDefault<TextBox>().Text
                };
                newOrder.UpsertIntoDb(db);
                this.currentOrder = newOrder;
            }
        }

        private void SaveNewProductOrderAmount()
        {
            List<TextBox> textBoxes;
            TextBox targetTextBox;
            textBoxes = FormHelper.GetControlsByName<TextBox>(this, USER_INPUTS.TxtCasesToOrder, true);

            if (textBoxes != null && textBoxes.Count > 0)
            {
                // note the default for a textbox is null
                targetTextBox = textBoxes.FirstOrDefault<TextBox>();

                // note textbox is disabled if the product is not in the database
                if (targetTextBox != null &&
                    targetTextBox.Text.Length > 0 &&
                    targetTextBox.Enabled)
                {
                    Database db = new Database();
                    DbQueryExecutor queryExe = new DbQueryExecutor(db.ConnectionString);

                    OrderProduct orderProduct = new OrderProduct(queryExe)
                    {
                        OrderID = this.currentOrder.Id,
                        ProductUPC = this.currentProduct.Upc,
                        CasesToOrder = Convert.ToInt32(textBoxes.FirstOrDefault<TextBox>().Text)
                    };
                    orderProduct.UpsertIntoDb();
                }
            }
        }

        private Bitmap CreateListViewSmallImage()
        {
            SolidBrush brush;
            Bitmap myBitmap;
            Graphics myBitmapGraphics;
            Size mySize = new Size(50, 50);

            brush = new SolidBrush(Color.Gray);
            myBitmap = new Bitmap(mySize.Width, mySize.Height);
            myBitmapGraphics = Graphics.FromImage(myBitmap);
            myBitmapGraphics.FillRectangle(brush, 0, 0, mySize.Width, mySize.Height);

            return myBitmap;
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
            ListView listOrders;
            ListViewItem listViewItem;
            Bitmap myBitmap;
            ImageList myImageList;
            Database db;
            Panel pnlSecondaryNav;
            Button[] buttons;
            Button btnCreate;
            Button btnDelete;
            Button btnContinue;

            // create buttons and navigation panel
            pnlSecondaryNav = new Panel();

            btnCreate = new Button() { Text = "Create" };
            btnCreate.Click += new EventHandler(this.BtnCreateNewOrder_Click);

            btnDelete = new Button() { Text = "Delete" };
            btnDelete.Click += new EventHandler(this.BtnDeleteExistingOrder_Click);

            btnContinue = new Button() { Text = "Continue" };
            btnContinue.Click += new EventHandler(this.BtnContinueExistingOrder_Click);

            // create list view
            listOrders = new ListView() { Activation = ItemActivation.OneClick, FullRowSelect = true };

            // populate list view
            db = new Database();
            foreach (Order order in db.Orders)
            {
                // create item and add it to the list view
                listViewItem = new ListViewItem()
                {
                    Text = order.Name,
                    ImageIndex = 0
                };
                listViewItem.SubItems.Add(order.Id.ToString());
                listOrders.Items.Add(listViewItem);
            }

            // add icon
            myBitmap = this.CreateListViewSmallImage();
            myImageList = new ImageList();
            myImageList.Images.Add(myBitmap);
            myImageList.ImageSize = myBitmap.Size;
            listOrders.SmallImageList = myImageList;
            listOrders.LargeImageList = myImageList;

            // set listview's layout and style
            listOrders.View = View.LargeIcon;
            listOrders.Dock = DockStyle.Top;

            buttons = new Button[] 
            { 
                btnContinue,
                btnDelete,
                btnCreate
            };

            this.SuspendLayout();

            listOrders.Dock = DockStyle.Fill;
            this.mainContent.Controls.Add(listOrders);

            pnlSecondaryNav.Dock = DockStyle.Top;
            pnlSecondaryNav.Height = ButtonHeight;
            this.mainContent.Controls.Add(pnlSecondaryNav);

            foreach (Button b in buttons)
            {
                b.Dock = DockStyle.Left;
                b.Height = ButtonHeight;
                b.Width = pnlSecondaryNav.ClientSize.Width / buttons.Length;
                pnlSecondaryNav.Controls.Add(b);
            }

            // colors
            this.SetBackColorForAllChildControls<Control>(this, Color.White, true);

            this.ResumeLayout();

            // back
            this.DisableBackButton();
        }

        private void LoadCreateNewOrderView()
        {
            TextBox textBox;
            Label label;
            Button button;

            label = new Label();
            label.Text = "Order Name";

            textBox = new TextBox();
            textBox.Name = USER_INPUTS.TxtOrderName;

            button = new Button() { Text = TextContinue };
            button.Click += new EventHandler(this.BtnSaveNewOrder_Click);

            // add and layout
            this.mainContent.SuspendLayout();

            label.Dock = DockStyle.Top;
            textBox.Dock = DockStyle.Top;
            button.Dock = DockStyle.Top;
            button.Height = ButtonHeight;

            Control[] controls = new Control[] { button, textBox, label };
            foreach (Control c in controls)
            {
                this.mainContent.Controls.Add(c);
            }

            textBox.Focus();

            this.mainContent.ResumeLayout();

            // colors
            this.SetBackColorForAllChildControls<Control>(this, Color.White, true);

            // back
            this.btnBack.Enabled = true;
            this.backDelegate = this.LoadOrdersView;
            this.saveDelegate = this.SaveNewOrder;
        }

        private void LoadOrderScanningView()
        {
            // only load the order scanning view if we have a selected order.
            if (this.currentOrder != null)
            {
                Label lblOrderName = new Label() { Text = "Order Name:" + this.currentOrder.Name };
                Label lblInstructions = new Label() { Text = "Start scanning." };

                Control[] controls = new Control[] 
                { 
                    lblOrderName, 
                    lblInstructions 
                }.Reverse<Control>().ToArray<Control>();

                foreach (Control c in controls)
                {
                    c.Dock = DockStyle.Top;
                    this.mainContent.Controls.Add(c);
                }

                this.barcodeAPI = new BarcodeAPI(this.BarcodeScanner_OnStatus, this.BarcodeScanner_OnScan);
                this.barcodeAPI.Scan();

                // colors
                this.SetBackColorForAllChildControls<Control>(this, Color.White, true);

                // back
                this.btnBack.Enabled = true;
                this.backDelegate = this.LoadOrdersView;
            }
        }

        private void LoadScanningErrorView()
        {
            Label lblMessage = new Label();
            lblMessage.Text = @"Error. Try one of these options: 
                    1. go back and restart the order, 
                    2. close the program and restart the order, 
                    3. restart the device and restart the order.";
            lblMessage.Dock = DockStyle.Top;
            this.mainContent.Controls.Add(lblMessage);
        }

        private void LoadOrderScanningResultView(ScanData scanData)
        {
            // get the upc, and remove the last digit,
            // because the country grocer database doesn't use the last digit
            string scannedUpc = scanData.Text;
            string trimmedUpc = scannedUpc.Trim().Substring(0, scannedUpc.Length - 1);
            long databaseUpc = Convert.ToInt64(trimmedUpc);

            // populate controls with default values
            Label lblProductUPC = new Label() { Text = "Upc: " };
            Label lblProductName = new Label() { Text = "Name: " };
            Label lblProductPrice = new Label() { Text = "Price: $" };
            Label lblProductAmount = new Label() { Text = "Cases to Order: " };
            Label lblHelp = new Label() { Text = "Help: " };

            TextBox txtCasesToOrder = new TextBox()
            {
                Name = USER_INPUTS.TxtCasesToOrder,
                Text = Constants.DefaultCasesToOrder.ToString()
            };

            // get the appropriate product from the database
            Database db = new Database();
            DbQueryExecutor qe = new DbQueryExecutor(db.ConnectionString);
            this.currentProduct = (new Product(qe)).SelectOne(databaseUpc);

            lblProductUPC.Text += trimmedUpc;

            // if it is in the database
            if (this.currentProduct == null)
            {
                lblProductName.Text += "This Upc code is not in the database.";
                txtCasesToOrder.Enabled = false;
                lblHelp.Text += "Keep scanning.";
            }
            else
            {
                // set its name
                lblProductName.Text += this.currentProduct.Name;
                lblProductPrice.Text += this.currentProduct.Price.ToString();
                lblHelp.Text += "Enter cases to order. Then keep scanning.";

                // enable the textbox for input and setup events
                txtCasesToOrder.Enabled = true;
                txtCasesToOrder.KeyPress += new KeyPressEventHandler(this.TxtValidateInt32_KeyPress);
                txtCasesToOrder.KeyPress += new KeyPressEventHandler(this.TxtClearDefaultValue_KeyPress);

                // check if this product is already in this order
                OrderProduct orderProduct =
                        db.OrderProducts.FirstOrDefault<OrderProduct>(op =>
                        op.OrderID == this.currentOrder.Id && op.ProductUPC == this.currentProduct.Upc);
                if (orderProduct != null)
                {
                    // it is so use it's existing amountToOrder
                    txtCasesToOrder.Text = orderProduct.CasesToOrder.ToString();
                }
            }

            // add the controls to an array in the order that we want them to display
            Control[] controls = new Control[]
            { 
                lblProductUPC, 
                lblProductName, 
                lblProductPrice,
                lblProductAmount,
                txtCasesToOrder,
                lblHelp
            }.Reverse<Control>().ToArray<Control>();

            // add the controls to the mainDisplay
            // whilst choosing a dock style
            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Top;
                this.mainContent.Controls.Add(c);
            }

            // set focus - we must do this after controls are in the form, apparently
            txtCasesToOrder.Focus();

            // get ready for another scan
            this.barcodeAPI.Scan();

            // colors
            this.SetBackColorForAllChildControls<Control>(this, Color.White, true);

            // back
            this.btnBack.Enabled = true;
            this.backDelegate = this.LoadOrdersView;
            this.saveDelegate = this.SaveNewProductOrderAmount;
        }

        #endregion

        #region Event Handlers

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
            this.Dispose();
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

        private void BtnCreateNewOrder_Click(object sender, EventArgs e)
        {
            this.UnloadCurrentView();
            this.LoadCreateNewOrderView();
        }

        private void BtnDeleteExistingOrder_Click(object sender, EventArgs e)
        {
            this.UpdateCurrentOrder();
            if (this.currentOrder != null)
            {
                this.DeleteSelectedOrder();
                this.UnloadCurrentView();
                this.LoadOrdersView();

                // TODO Also delete associated OrderProducts
            }
            else
            {
                MessageBox.Show("Please select an order.");
            }
        }

        private void BtnContinueExistingOrder_Click(object sender, EventArgs e)
        {
            this.UpdateCurrentOrder();
            if (this.currentOrder != null)
            {
                this.UnloadCurrentView();
                this.LoadOrderScanningView();
            }
            else
            {
                MessageBox.Show("Please select an order.");
            }
        }

        private void BtnSaveNewOrder_Click(object sender, EventArgs e)
        {
            this.SaveNewOrder();
            this.UnloadCurrentView();
            this.LoadOrderScanningView();
        }

        private void TxtClearDefaultValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt;
            txt = sender as TextBox;
            if (txt != null && !txt.Modified)
            {
                txt.Text = string.Empty;
            }
        }

        private void TxtValidateInt32_KeyPress(object sender, KeyPressEventArgs e)
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
        }

        private void BarcodeScanner_OnScan(ScanDataCollection scanDataCollection)
        {
            // Get ScanData
            ScanData scanData = scanDataCollection.GetFirst;

            switch (scanData.Result)
            {
                case Results.SUCCESS:
                    // be sure to save the order
                    // before unloading the current view
                    // because the current view contains the order amount
                    this.SaveNewProductOrderAmount();
                    this.UnloadCurrentView();
                    this.LoadOrderScanningResultView(scanData);
                    break;

                case Results.CREATEEVENT_FAILED:
                    this.LoadScanningErrorView();
                    break;

                default:
                    break;
            }
        }

        private void BarcodeScanner_OnStatus(StatusData statusData)
        {
            int i = 0;
            i++;
        }

        #endregion

        private static class USER_INPUTS
        {
            internal const string TxtOrderName = "TxtOrderName";
            internal const string TxtCasesToOrder = "TxtCasesToOrder";
        }
    }
}