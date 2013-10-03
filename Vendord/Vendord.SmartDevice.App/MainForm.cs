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

        // user inputs control names
        private static class UserInputs
        {
            internal const string TXT_ORDER_SESSION_NAME = "txtOrderSessionName";
            internal const string TXT_ORDER_ITEM_AMOUNT = "txtOrderItemAmount";
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

            //
            // create main content panel
            //
            mainContent = new Panel();
            mainContent.Dock = DockStyle.Fill;

            //
            // add to form - this triggers its layout event 
            //           
            controls = new Control[] { mainNavigation, mainContent }.Reverse().ToArray();
            foreach (Control c in controls)
            {
                this.Controls.Add(c);
            }

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
            controls = new Control[] { btnBack, btnClose }.Reverse().ToArray();
            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Left;
                c.Width = mainNavigation.ClientSize.Width / controls.Length;
                mainNavigation.Controls.Add(c);
            }
        }

        #region Utilities

        private void continueExistingOrder()
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

            descendents = FormHelper.GetControlsByName<TextBox>(this, UserInputs.TXT_ORDER_SESSION_NAME, true);
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
            textBoxes = FormHelper.GetControlsByName<TextBox>(this, UserInputs.TXT_ORDER_ITEM_AMOUNT, true);

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

            this.mainContent.Controls.Add(btnOrders);

            // style
            buttons = new Button[] { 
                
                btnOrders 
            
            }.Reverse().ToArray();

            foreach (Button b in buttons)
            {
                b.Dock = DockStyle.Top;
                b.Height = b.Parent.ClientSize.Height / buttons.Count();
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

            //
            // create list view
            lvOrders = new ListView() { Activation = ItemActivation.OneClick, FullRowSelect = true };
            lvOrders.ItemActivate += new EventHandler(lvOrders_ItemActivate);

            //
            // enable add new
            //
            lvOrders.Items.Add(new ListViewItem()
            {
                Text = "<Add New>",
                ImageIndex = 0
            });

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

            // add the list view to the main content - this triggers its layout event
            this.mainContent.Controls.Add(lvOrders);

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
            textBox.Name = UserInputs.TXT_ORDER_SESSION_NAME;

            button = new Button() { Text = "Save" };
            button.Click += new EventHandler(btnSaveNewOrder_Click);

            //
            // add and layout
            //
            this.mainContent.SuspendLayout();

            label.Dock = DockStyle.Top;
            textBox.Dock = DockStyle.Top;
            button.Dock = DockStyle.Fill;

            Control[] controls = new Control[] { label, textBox, button }.Reverse().ToArray();
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

            // select scanned product from DB
            db = new VendordDatabase();
            
            currentScannedProduct = db.Products.FirstOrDefault<VendordDatabase.Product>(p => p.UPC.Equals(scanData.Text));

            if (currentScannedProduct != null)
            {
                // instantiate the controls that will display
                lblProductUPC = new Label() { Text = currentScannedProduct.UPC };
                lblProductName = new Label() { Text = currentScannedProduct.Name };
                lblProductAmount = new Label() { Text = "Cases to Order:" };
                lblInstruction = new Label() { Text = "Enter amount and/or keep scanning." };

                txtProductAmount = new TextBox() { Name = UserInputs.TXT_ORDER_ITEM_AMOUNT };
                txtProductAmount.KeyPress += new KeyPressEventHandler(int32TextBox_Validate_KeyPress);

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

                // set focus to the text box
                txtProductAmount.Focus();
            }

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

        private void lvOrders_ItemActivate(object sender, EventArgs e)
        {
            ListView lvOrders;
            ListViewItem selectedItem;

            lvOrders = sender as ListView;
            selectedItem = lvOrders.Items[lvOrders.SelectedIndices[0]];
            if (selectedItem.Index == 0)
            {
                unloadCurrentView();
                loadCreateNewOrderView();
            }
            else
            {
                continueExistingOrder();
                unloadCurrentView();
                loadOrderScanningView();
            }
        }

        private void btnSaveNewOrder_Click(object sender, EventArgs e)
        {
            saveNewOrder();
            unloadCurrentView();
            loadOrderScanningView();
        }

        private void int32TextBox_Validate_KeyPress(object sender, KeyPressEventArgs e)
        {
            // default to disallowing input
            e.Handled = true;

            // whitelist
            if (char.IsDigit(e.KeyChar))
            {
                // it's a digit
                try
                {
                    Convert.ToInt32((sender as TextBox).Text + e.KeyChar);
                    // and it's within 32 bits 
                    // so allow it
                    e.Handled = false;
                }
                catch (OverflowException) 
                { 
                    // do nothing
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