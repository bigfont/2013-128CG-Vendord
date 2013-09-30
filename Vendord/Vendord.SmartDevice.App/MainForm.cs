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
            int orderSessionID;
            string orderSessionName;
            if (listView != null)
            {
                // current order session id
                orderSessionID = Convert.ToInt32(listView.FocusedItem.SubItems[0].Text.ToString());
                currentOrderSession.ID = orderSessionID;

                // current order session name
                orderSessionName = listView.FocusedItem.Text;
                currentOrderSession.Name = orderSessionName;                
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
                newOrderSession.InsertIntoDB();
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
                    OrderSessionID = 0,
                    ProductID = 0,
                    CasesToOrder = Convert.ToInt16(textBoxes.FirstOrDefault<TextBox>().Text)
                };
                orderSessionProduct.InsertIntoDB();
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
            Label label = new Label();
            label.Text = "Order: " + currentOrderSession.Name + "\n\n Start scanning.";

            mainContent.Controls.Add(label);

            BarcodeAPI scanner = new BarcodeAPI(barcodeScanner_OnStatus, barcodeScanner_OnScan);
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

        private void loadScanResultView(ScanData scanData)
        {
            ListView listView;
            TextBox textBox;
            Label label;
            VendordDatabase db;
            Button button;
            VendordDatabase.Product product;

            db = new VendordDatabase();
            product = db.Products.First<VendordDatabase.Product>(p => p.UPC.Equals(scanData.Text));
            if (product != null)
            {
                listView = new ListView();
                listView.Columns.Add(new ColumnHeader() { Text = "Product" });
                listView.Items.Add(new ListViewItem(product.UPC));
                listView.Items.Add(new ListViewItem(product.Name));

                textBox = new TextBox();
                textBox.Name = UserInputControlNames.ORDER_ITEM_AMOUNT;
                label = new Label();
                label.Text = "Amount";
                button = FormNavigation.CreateButton("Save Order", FormNavigation.SAVE_AND_STOP_SCANNING, "TODO", handleFormControlEvents);

                this.mainContent.Controls.Add(listView);
                this.mainContent.Controls.Add(label);
                this.mainContent.Controls.Add(textBox);
                this.mainContent.Controls.Add(button);

                textBox.Focus();

                styles.StyleListView(listView);

                styles.StyleSimpleForm(textBox, label, button);
            }
            nav.CurrentView = FormNavigation.VIEW_AND_EDIT_SCAN_RESULT;
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
                    loadOrderScanningView();
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
                    unloadCurrentView();
                    loadScanResultView(scanData);
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