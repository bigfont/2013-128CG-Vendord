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
        internal FormNavigation nav;
        internal Panel mainNavigation;
        internal Panel mainContent;
        internal BarcodeAPI barcodeAPI;

        // scanning state
        private VendordDatabase.OrderSession currentOrderSession = new VendordDatabase.OrderSession();
        private VendordDatabase.Product currentProduct = new VendordDatabase.Product();
        private VendordDatabase.OrderSession_Product currentOrderSession_Product = new VendordDatabase.OrderSession_Product();

        private static class UserInputControlNames
        {
            internal const string TXT_ORDER_SESSION_NAME = "txtOrderSessionName";
            internal const string TXT_ORDER_ITEM_AMOUNT = "txtOrderItemAmount";
        }

        public MainForm()
        {
            Control[] controls;

            this.Load += handleFormControlEvents;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;

            //
            // create main navigation panel
            //
            nav = new FormNavigation(this);
            mainNavigation = new Panel();
            mainNavigation.Dock = DockStyle.Top;

            //
            // create main content panel
            //
            mainContent = new Panel();
            mainContent.Dock = DockStyle.Fill;

            //
            // add to form - this triggers its layout event 
            // "The Layout event occurs [on a control] when child controls add added or removed... "
            // see also http://msdn.microsoft.com/en-us/library/system.windows.forms.control.layout%28v=vs.90%29.aspx
            //            
            controls = new Control[] { mainNavigation, mainContent }.Reverse().ToArray();
            foreach (Control c in controls)
            {
                this.Controls.Add(c);
            }

            //
            // Create Buttons
            //
            controls = new Control[] { 
                FormNavigation.CreateButton("Back", FormNavigation.BACK, "TODO", Color.LightGreen, handleFormControlEvents),
                FormNavigation.CreateButton("Close", FormNavigation.CLOSE, "TODO", Color.Firebrick, handleFormControlEvents)                
            }.Reverse().ToArray();

            //
            // add to panel - this triggers its layout event
            //
            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Left;
                c.Width = mainNavigation.ClientSize.Width / controls.Length;
                mainNavigation.Controls.Add(c);
            }
        }

        private void continueExistingOrderSession()
        {
            ListView listView = FormHelper.GetControlsByType<ListView>(this, true).FirstOrDefault<ListView>();

            if (listView != null)
            {
                currentOrderSession.ID = Convert.ToInt32(listView.FocusedItem.SubItems[1].Text);
                currentOrderSession.Name = listView.FocusedItem.SubItems[0].Text;
            }
        }

        private void saveNewOrderSession()
        {
            // start new order session
            List<TextBox> descendents;

            descendents = FormHelper.GetControlsByName<TextBox>(this, UserInputControlNames.TXT_ORDER_SESSION_NAME, true);
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

        private void saveCurrentScanningInput()
        {
            List<TextBox> textBoxes = FormHelper.GetControlsByName<TextBox>(this, UserInputControlNames.TXT_ORDER_ITEM_AMOUNT, true);

            if (textBoxes != null && textBoxes.Count > 0 && textBoxes.First<TextBox>().Text.Length > 0)
            {
                VendordDatabase.OrderSession_Product orderSessionProduct = new VendordDatabase.OrderSession_Product()
                {
                    OrderSessionID = currentOrderSession.ID,
                    ProductID = currentProduct.ID,
                    CasesToOrder = Convert.ToInt16(textBoxes.FirstOrDefault<TextBox>().Text)
                };
                orderSessionProduct.UpsertIntoDB();
            }
        }

        #region Views

        private void unloadCurrentView()
        {
            mainContent.Controls.Clear();
        }

        private void loadHomeView()
        {
            Button btnOrders;            
            Button[] buttons;

            btnOrders = FormNavigation.CreateButton("Orders", FormNavigation.ORDERS, "TODO", Color.LightGreen, handleFormControlEvents);                     

            this.mainContent.Controls.Add(btnOrders);

            // style
            buttons = new Button[] { btnOrders };
            foreach (Button b in buttons)
            {
                b.BringToFront();                
                b.Dock = DockStyle.Top;
                b.Height = b.Parent.ClientSize.Height / buttons.Count();
            }

            nav.CurrentView = FormNavigation.HOME;
        }

        private void loadOrdersView()
        {
            ListView listView;
            ListViewItem listViewItem;
            VendordDatabase db;

            //
            // create list view
            listView = FormNavigation.CreateListView("Order Sessions", null, null, handleFormControlEvents);
            listView.Items.Add(new ListViewItem()
            {
                Text = "<Add New>",
                Tag = FormNavigation.CREATE_NEW_ORDER_SESSION,
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
                    Tag = FormNavigation.CONTINUE_EXISTING_ORDER_SESSION,
                    ImageIndex = 0
                };
                listViewItem.SubItems.Add(order.ID.ToString());
                listView.Items.Add(listViewItem);
            }

            // set the listview's layout     
            listView.View = View.SmallIcon;
            listView.Dock = DockStyle.Top;

            //
            // add icon
            //
            ImageList imageList;
            SolidBrush brush;
            Bitmap myBitmap;
            Graphics myBitmapGraphics;
            Size mySize = new Size(30, 30);

            brush = new SolidBrush(Color.LightGreen);
            myBitmap = new Bitmap(mySize.Width, mySize.Height);
            myBitmapGraphics = Graphics.FromImage(myBitmap);
            myBitmapGraphics.FillRectangle(brush, 0, 0, mySize.Width, mySize.Height);
            imageList = new ImageList();
            imageList.Images.Add(myBitmap);
            imageList.ImageSize = mySize;
            listView.SmallImageList = imageList;            

            // add the list view to the main content - this triggers its layout event
            this.mainContent.Controls.Add(listView);
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
                lblInstructions }.Reverse<Control>().ToArray<Control>();

            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Top;
                mainContent.Controls.Add(c);
            }

            barcodeAPI = new BarcodeAPI(barcodeScanner_OnStatus, barcodeScanner_OnScan);
            barcodeAPI.Scan();
        }

        private void loadCreateNewOrderView()
        {
            TextBox textBox;
            Label label;
            Button button;

            textBox = new TextBox();
            textBox.Name = UserInputControlNames.TXT_ORDER_SESSION_NAME;

            label = new Label();
            label.Text = "Order Name";

            button = FormNavigation.CreateButton("Save", FormNavigation.SAVE_AND_START_NEW_ORDER_SESSION, "TODO", Color.LightGreen, handleFormControlEvents);

            this.mainContent.Controls.Add(textBox);
            this.mainContent.Controls.Add(label);
            this.mainContent.Controls.Add(button);

            textBox.Focus();

            Control[] controls = new Control[] { label, textBox, button };
            foreach (Control c in controls)
            {
                c.BringToFront();
                c.Dock = DockStyle.Top;
                c.Height = c.Parent.ClientSize.Height / controls.Count();
            }
        }

        private void loadOrderProductInputView(ScanData scanData)
        {
            // declare 
            Label lblProductUPC, lblProductName, lblProductAmount;
            TextBox txtProductAmount;
            Button btnSave;
            Control[] controls;
            VendordDatabase db;

            // select scanned product from DB
            db = new VendordDatabase();
            currentProduct = db.Products.First<VendordDatabase.Product>(p => p.UPC.Equals(scanData.Text));

            if (currentProduct != null)
            {
                // instantiate the controls that will display
                lblProductUPC = new Label() { Text = currentProduct.UPC };
                lblProductName = new Label() { Text = currentProduct.Name };
                lblProductAmount = new Label() { Text = "Cases to Order:" };

                txtProductAmount = new TextBox() { Name = UserInputControlNames.TXT_ORDER_ITEM_AMOUNT };
                txtProductAmount.KeyPress += new KeyPressEventHandler(digitOnlyTextBox_KeyPress);

                btnSave = FormNavigation.CreateButton("Save Order", FormNavigation.SAVE_AND_STOP_SCANNING, "TODO", Color.LightGreen, handleFormControlEvents);

                // add the controls to an array in the order that we want them to display
                controls = new Control[] { 
                
                    lblProductUPC, 
                    lblProductName, 
                    lblProductAmount,
                    txtProductAmount,
                    btnSave
                
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

            // set the current view
            nav.CurrentView = FormNavigation.VIEW_AND_EDIT_SCAN_RESULT;

            // get ready for another scan
            barcodeAPI.Scan();
        }

        #endregion

        #region Event Handlers

        private void digitOnlyTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void handleFormControlEvents(object sender, EventArgs e)
        {
            // set last action
            nav.ParseActionFromSender(sender);

            // set the name of the form
            this.Text = String.Format("{0} {1}", this.Name, "");

            // act based on the aciton
            switch (nav.Action)
            {
                case FormNavigation.ORDERS:
                    unloadCurrentView();
                    loadOrdersView();
                    break;

                case FormNavigation.CREATE_NEW_ORDER_SESSION:
                    unloadCurrentView();
                    loadCreateNewOrderView();
                    break;

                case FormNavigation.SAVE_AND_START_NEW_ORDER_SESSION:
                    saveNewOrderSession();
                    unloadCurrentView();
                    loadOrderScanningView();
                    break;

                case FormNavigation.CONTINUE_EXISTING_ORDER_SESSION:
                    continueExistingOrderSession();
                    unloadCurrentView();
                    loadOrderScanningView();
                    break;

                case FormNavigation.SAVE_AND_STOP_SCANNING:
                    saveCurrentScanningInput();
                    unloadCurrentView();
                    loadHomeView();
                    break;

                case FormNavigation.CLOSE:
                    this.Close();
                    return; // return because we want to avoid executing code after we close the form.  

                default:
                    unloadCurrentView();
                    loadHomeView();
                    break;
            }
        }

        private void barcodeScanner_OnScan(ScanDataCollection scanDataCollection)
        {
            // Get ScanData
            ScanData scanData = scanDataCollection.GetFirst;

            switch (scanData.Result)
            {
                case Results.SUCCESS:
                    saveCurrentScanningInput();
                    unloadCurrentView();
                    loadOrderProductInputView(scanData);
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