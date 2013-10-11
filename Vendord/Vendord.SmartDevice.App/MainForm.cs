namespace Vendord.SmartDevice.App
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;
    using Symbol.Barcode2;
    using System.IO;
    using Vendord.SmartDevice.Shared;

    public class MainForm : Form
    {
        private short DEFAULT_PRODUCT_AMOUNT = 1;
        private int BUTTON_HEIGHT = 50;
        private int NUMBER_OF_NAV_BUTTONS = 2;
        private static class USER_INPUTS
        {
            internal const string TXT_ORDER_SESSION_NAME = "TXT_ORDER_SESSION_NAME";
            internal const string TXT_ORDER_ITEM_AMOUNT = "TXT_ORDER_ITEM_AMOUNT";
        }

        private Panel mainNavigation;        
        private Panel mainContent;

        private Button btnBack;
        private delegate void Back();
        private Back BackDelegate;
        private delegate void Save();
        private Save SaveDelegate;

        // scanning specific fields
        private BarcodeAPI barcodeAPI;
        private VendordDatabase.OrderSession currentOrderSession
            = new VendordDatabase.OrderSession();
        private VendordDatabase.Product currentScannedProduct
            = new VendordDatabase.Product();        

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
            mainNavigation.Height = BUTTON_HEIGHT;

            //
            // create main content panel
            //
            mainContent = new Panel();
            mainContent.Dock = DockStyle.Fill;

            //
            // add to form 
            //           
            this.SuspendLayout();
            controls = new Control[] { mainContent, mainNavigation };
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
            controls = new Button[] { btnClose, btnBack };

            this.mainNavigation.SuspendLayout();
            
            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Left;
                c.Height = BUTTON_HEIGHT;
                c.Width = this.mainNavigation.ClientSize.Width / controls.Length;
                mainNavigation.Controls.Add(c);
            }
            this.mainNavigation.ResumeLayout();
        }

        #region Utilities

        private void deleteSelectedOrder()
        {
            ListView listView = FormHelper.GetControlsByType<ListView>(this, true).FirstOrDefault<ListView>();

            if (listView != null)
            {
                currentOrderSession.ID = Convert.ToInt32(listView.FocusedItem.SubItems[1].Text);
                currentOrderSession.Delete();
                currentOrderSession = null;
            }
        }

        private void continueSelectedOrder()
        {
            ListView listView = FormHelper.GetControlsByType<ListView>(this, true).FirstOrDefault<ListView>();

            if (listView != null)
            {
                currentOrderSession.ID = Convert.ToInt32(listView.FocusedItem.SubItems[1].Text);
                currentOrderSession.Name = listView.FocusedItem.SubItems[0].Text;
            }
        }

        private void saveNewOrder()
        {
            // start new order session
            List<TextBox> descendents;

            descendents = FormHelper.GetControlsByName<TextBox>(this, USER_INPUTS.TXT_ORDER_SESSION_NAME, true);
            if (descendents != null && descendents.Count > 0)
            {
                VendordDatabase.OrderSession newOrderSession = new VendordDatabase.OrderSession()
                {
                    Name = descendents.FirstOrDefault<TextBox>().Text
                };
                newOrderSession.UpsertIntoDB();
                currentOrderSession.ID = newOrderSession.ID;
                currentOrderSession.Name = newOrderSession.Name;
            }
        }

        private void saveNewProductOrderAmount()
        {
            List<TextBox> textBoxes;
            textBoxes = FormHelper.GetControlsByName<TextBox>(this, USER_INPUTS.TXT_ORDER_ITEM_AMOUNT, true);

            if (textBoxes != null && textBoxes.Count > 0 && textBoxes.First<TextBox>().Text.Length > 0)
            {
                VendordDatabase.OrderSession_Product orderSessionProduct = new VendordDatabase.OrderSession_Product()
                {
                    OrderSessionID = currentOrderSession.ID,
                    ProductID = currentScannedProduct.ID,
                    CasesToOrder = Convert.ToInt32(textBoxes.FirstOrDefault<TextBox>().Text)
                };
                orderSessionProduct.UpsertIntoDB();
            }
        }

        private Bitmap createListViewSmallImage()
        {
            SolidBrush brush;
            Bitmap myBitmap;
            Graphics myBitmapGraphics;
            Size mySize = new Size(30, 30);

            brush = new SolidBrush(Color.Gray);
            myBitmap = new Bitmap(mySize.Width, mySize.Height);
            myBitmapGraphics = Graphics.FromImage(myBitmap);
            myBitmapGraphics.FillRectangle(brush, 0, 0, mySize.Width, mySize.Height);

            return myBitmap;
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
            mainContent.Controls.Clear();
        }

        private void loadHomeView()
        {
            Button btnOrders;
            Button[] buttons;

            btnOrders = new Button() { Text = "Orders" };
            btnOrders.Click += new EventHandler(btnOrders_Click);            

            // style
            buttons = new Button[] { 
                
                btnOrders 
            
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
            disableBackButton();
        }

        private void loadOrdersView()
        {
            ListView lvOrders;
            ListViewItem listViewItem;
            Bitmap myBitmap;
            ImageList myImageList;
            VendordDatabase db;
            Panel pnlSecondaryNav;
            Button[] buttons;
            Button btnCreate;
            Button btnDelete;
            Button btnContinue;

            // create buttons and navigation panel
            pnlSecondaryNav = new Panel();

            btnCreate = new Button() { Text = "Create" };
            btnCreate.Click += new EventHandler(btnCreateNewOrder_Click);

            btnDelete = new Button() { Text = "Delete" };
            btnDelete.Click += new EventHandler(btnDeleteExistingOrder_Click);

            btnContinue = new Button() { Text = "Continue" };
            btnContinue.Click += new EventHandler(btnContinueExistingOrder_Click);

            //
            // create list view
            lvOrders = new ListView() { Activation = ItemActivation.OneClick, FullRowSelect = true };            

            // 
            // populate list view
            //
            db = new VendordDatabase();
            foreach (VendordDatabase.OrderSession order in db.OrderSessions)
            {
                // create item and add it to the list view
                listViewItem = new ListViewItem()
                {
                    Text = order.Name,
                    ImageIndex = 0
                };
                listViewItem.SubItems.Add(order.ID.ToString());
                lvOrders.Items.Add(listViewItem);
            }

            //
            // add icon
            //
            myBitmap = createListViewSmallImage();
            myImageList = new ImageList();
            myImageList.Images.Add(myBitmap);
            myImageList.ImageSize = myBitmap.Size;
            lvOrders.SmallImageList = myImageList;

            // set listview's layout and style
            lvOrders.View = View.List;
            lvOrders.Dock = DockStyle.Top;

            buttons = new Button[] { 
            
                btnContinue,
                btnDelete,
                btnCreate
            
            };

            this.SuspendLayout();

            lvOrders.Dock = DockStyle.Fill;
            this.mainContent.Controls.Add(lvOrders);

            pnlSecondaryNav.Dock = DockStyle.Top;
            pnlSecondaryNav.Height = BUTTON_HEIGHT;
            this.mainContent.Controls.Add(pnlSecondaryNav);

            foreach (Button b in buttons)
            {
                b.Dock = DockStyle.Left;
                b.Height = BUTTON_HEIGHT;
                b.Width = pnlSecondaryNav.ClientSize.Width / buttons.Length;
                pnlSecondaryNav.Controls.Add(b);
            }           

            this.ResumeLayout();

            //
            // back
            //
            enableBackButton(loadHomeView);
        }

        private void loadCreateNewOrderView()
        {
            TextBox textBox;
            Label label;
            Button button;

            label = new Label();
            label.Text = "Order Name";

            textBox = new TextBox();
            textBox.Name = USER_INPUTS.TXT_ORDER_SESSION_NAME;

            button = new Button() { Text = "Save" };
            button.Click += new EventHandler(btnSaveNewOrder_Click);

            //
            // add and layout
            //
            this.mainContent.SuspendLayout();

            label.Dock = DockStyle.Top;
            textBox.Dock = DockStyle.Top;
            button.Dock = DockStyle.Top;
            button.Height = BUTTON_HEIGHT;
            
            Control[] controls = new Control[] { button, textBox, label };
            foreach (Control c in controls)
            {
                this.mainContent.Controls.Add(c);
            }

            textBox.Focus();

            this.mainContent.ResumeLayout();

            //
            // back
            //
            btnBack.Enabled = true;
            BackDelegate = loadOrdersView;
            SaveDelegate = saveNewOrder;
        }

        private void loadOrderScanningView()
        {
            Label lblOrderSessionName;
            Label lblInstructions;
            Control[] controls;

            lblOrderSessionName = new Label() { Text = "Order Name:" + currentOrderSession.Name };
            lblInstructions = new Label() { Text = "Start scanning." };

            controls = new Control[] { 

                lblOrderSessionName, 
                lblInstructions 

            }.Reverse<Control>().ToArray<Control>();

            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Top;
                mainContent.Controls.Add(c);
            }

            barcodeAPI = new BarcodeAPI(barcodeScanner_OnStatus, barcodeScanner_OnScan);
            barcodeAPI.Scan();

            //
            // back
            //
            btnBack.Enabled = true;
            BackDelegate = loadOrdersView;
        }

        private void loadOrderScanningResultView(ScanData scanData)
        {
            // declare 
            Label lblProductUPC, lblProductName, lblProductAmount, lblInstruction;
            TextBox txtProductAmount;
            Control[] controls;
            VendordDatabase db;

            // populate controls with default values
            lblProductUPC = new Label() { Text = scanData.Text };
            lblProductName = new Label() { Text = "This UPC code is not in the database." };
            lblProductAmount = new Label() { Text = "Cases to Order:" };
            txtProductAmount = new TextBox() { Name = USER_INPUTS.TXT_ORDER_ITEM_AMOUNT, Enabled = false, Text = DEFAULT_PRODUCT_AMOUNT.ToString() };
            lblInstruction = new Label() { Text = "Enter amount. Keep scanning to continue and save." };            

            // add values for product that is in the database
            db = new VendordDatabase();
            currentScannedProduct = db.Products.FirstOrDefault<VendordDatabase.Product>(p => p.UPC.Equals(scanData.Text));
            if (currentScannedProduct != null)
            {
                lblProductName.Text = currentScannedProduct.Name;
                txtProductAmount.Enabled = true;
                txtProductAmount.KeyPress += new KeyPressEventHandler(txtValidateInt32_KeyPress);
                txtProductAmount.KeyPress += new KeyPressEventHandler(txtClearDefaultValue_KeyPress);

            }

            // add the controls to an array in the order that we want them to display
            controls = new Control[] { 
                
                    lblProductUPC, 
                    lblProductName, 
                    lblProductAmount,
                    txtProductAmount,
                    lblInstruction
                
                }.Reverse<Control>().ToArray<Control>();

            // add the controls to the mainDisplay
            // whilst choosing a dock style
            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Top;
                this.mainContent.Controls.Add(c);
            }

            // set focus - we must do this after controls are in the form, apparently
            txtProductAmount.Focus();

            // get ready for another scan
            barcodeAPI.Scan();

            //
            // back
            //
            btnBack.Enabled = true;
            BackDelegate = loadOrdersView;
            SaveDelegate = saveNewProductOrderAmount;
        }

        #endregion

        #region Event Handlers

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

        private void btnCreateNewOrder_Click(object sender, EventArgs e)
        {
            unloadCurrentView();
            loadCreateNewOrderView();
        }

        private void btnDeleteExistingOrder_Click(object sender, EventArgs e)
        {
            deleteSelectedOrder();
            unloadCurrentView();
            loadOrdersView();

            // TODO Also delete associated ordersession_products
        }

        private void btnContinueExistingOrder_Click(object sender, EventArgs e)
        {
            continueSelectedOrder();
            unloadCurrentView();
            loadOrderScanningView();
        }

        private void btnSaveNewOrder_Click(object sender, EventArgs e)
        {
            saveNewOrder();
            unloadCurrentView();
            loadOrderScanningView();
        }

        private void txtClearDefaultValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt;
            txt = sender as TextBox;
            if (txt != null && !txt.Modified)
            {
                txt.Text = String.Empty;
            }
        }

        private void txtValidateInt32_KeyPress(object sender, KeyPressEventArgs e)
        {
            // use a whitelist approach by disallowing all input
            e.Handled = true;

            // whitelist controls
            if (char.IsControl(e.KeyChar))
            {
                // it's a control; ergo allow it
                e.Handled = false;
            }

            // whitelist digits
            if (char.IsDigit(e.KeyChar))
            {
                // it's a digit
                try
                {
                    Convert.ToInt32((sender as TextBox).Text + e.KeyChar);
                    // the method didn't throw an overflow exception; so it's within 32 bits; ergo allow it                    
                    e.Handled = false;
                }
                catch (OverflowException)
                {
                    // catch and continue
                }
            }
        }

        private void barcodeScanner_OnScan(ScanDataCollection scanDataCollection)
        {
            // Get ScanData
            ScanData scanData = scanDataCollection.GetFirst;

            switch (scanData.Result)
            {
                case Results.SUCCESS:
                    saveNewProductOrderAmount();
                    unloadCurrentView();
                    loadOrderScanningResultView(scanData);
                    break;

                default:
                    break;
            }

        }

        private void barcodeScanner_OnStatus(StatusData statusData)
        {
            int i = 0;
            i++;
        }

        #endregion
    }
}