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
        internal FormStyles styles;
        internal Panel mainNavigation;
        internal Panel mainContent;
        internal BarcodeAPI barcodeAPI;

        // scanning state
        private VendordDatabase.OrderSession currentOrderSession = new VendordDatabase.OrderSession();
        private VendordDatabase.Product currentProduct = new VendordDatabase.Product();
        private VendordDatabase.OrderSession_Product currentOrderSession_Product = new VendordDatabase.OrderSession_Product();

        private static class UserInputControlNames
        {
            internal const string ORDER_SESSION_NAME = "txtOrderSessionName";
            internal const string ORDER_ITEM_AMOUNT = "txtOrderItemAmount";
        }

        public MainForm()
        {
            this.Load += handleFormControlEvents;

            nav = new FormNavigation(this);
            styles = new FormStyles(this);

            mainNavigation = nav.CreateMainNavigationPanel(handleFormControlEvents);
            mainContent = new Panel();

            this.Controls.Add(mainNavigation);
            this.Controls.Add(mainContent);

            styles.StyleForm();
            styles.StyleNavigationPanel(mainNavigation);
            styles.StyleMainContentPanel(mainContent);
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

            descendents = FormHelper.GetControlsByName<TextBox>(this, UserInputControlNames.ORDER_SESSION_NAME, true);
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
            List<TextBox> textBoxes = FormHelper.GetControlsByName<TextBox>(this, UserInputControlNames.ORDER_ITEM_AMOUNT, true);

            if (textBoxes != null && textBoxes.Count > 0)
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
            Button btnWhatever;

            btnOrders = FormNavigation.CreateButton("Orders", FormNavigation.ORDERS, "TODO", handleFormControlEvents);
            btnWhatever = new Button();
            btnWhatever.Text = "Whatever";

            this.mainContent.Controls.Add(btnOrders);
            this.mainContent.Controls.Add(btnWhatever);

            styles.StyleLargeButtons(new Button[] { btnOrders, btnWhatever });

            nav.CurrentView = FormNavigation.HOME;
        }

        private void loadOrdersView()
        {
            ListView listView;
            ListViewItem listViewItem;
            listView = FormNavigation.CreateListView("Order Sessions", FormNavigation.CONTINUE_EXISTING_ORDER_SESSION, "TODO", handleFormControlEvents);

            ColumnHeader name = new ColumnHeader();
            name.Text = "Order Session Name";
            listView.Columns.Add(name);

            listView.Items.Add(new ListViewItem()
            {
                Text = "<Add New>",
                Tag = FormNavigation.CREATE_NEW_ORDER_SESSION
            });

            VendordDatabase db = new VendordDatabase();
            foreach (VendordDatabase.OrderSession order in db.OrderSessions)
            {
                // create item
                listViewItem = new ListViewItem();
                listViewItem.Text = order.Name;
                listViewItem.Tag = FormNavigation.CONTINUE_EXISTING_ORDER_SESSION;
                listViewItem.SubItems.Add(order.ID.ToString());
                // add to list view
                listView.Items.Add(listViewItem);
            }

            this.mainContent.Controls.Add(listView);

            styles.StyleListView(listView);
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
            textBox.Name = UserInputControlNames.ORDER_SESSION_NAME;
            label = new Label();
            label.Text = "Order Name";
            button = FormNavigation.CreateButton("Save", FormNavigation.SAVE_AND_START_NEW_ORDER_SESSION, "TODO", handleFormControlEvents);

            this.mainContent.Controls.Add(textBox);
            this.mainContent.Controls.Add(label);
            this.mainContent.Controls.Add(button);

            textBox.Focus();

            styles.StyleSimpleForm(textBox, label, button);
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
                txtProductAmount = new TextBox() { Name = UserInputControlNames.ORDER_ITEM_AMOUNT };
                btnSave = FormNavigation.CreateButton("Save Order", FormNavigation.SAVE_AND_STOP_SCANNING, "TODO", handleFormControlEvents);

                // add the controls to an array in the order that we want them to display
                controls = new Control[] { 
                
                    lblProductUPC, 
                    lblProductName, 
                    lblProductAmount,
                    txtProductAmount,
                    btnSave
                
                };

                // reverse that order
                controls = controls.Reverse<Control>().ToArray<Control>();

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