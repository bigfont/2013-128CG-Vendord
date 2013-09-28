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

        private void saveOrderSession()
        {
            List<TextBox> descendents = FormHelper.GetControlByName<TextBox>(this, "txtOrderSessionName", true);
            VendordDatabase.OrderSession orderSession = new VendordDatabase.OrderSession()
            {
                Name = descendents.FirstOrDefault<TextBox>().Text
            };
            orderSession.InsertIntoDB();
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
            listView = FormNavigation.CreateListView("Order Sessions", FormNavigation.START_OR_CONTINUE_SCANNING, "TODO", handleFormControlEvents);

            ColumnHeader name = new ColumnHeader();
            name.Text = "Order Session Name";
            listView.Columns.Add(name);

            listView.Items.Add(new ListViewItem()
            {
                Text = "<Add New>",
                Tag = FormNavigation.CREATE_ORDER
            });

            VendordDatabase db = new VendordDatabase();            
            foreach(VendordDatabase.OrderSession order in db.OrderSessions)
            {
                listView.Items.Add(new ListViewItem()
                {
                    Text = order.Name,
                    Tag = FormNavigation.START_OR_CONTINUE_SCANNING
                });
            }            

            this.mainContent.Controls.Add(listView);

            styles.StyleListView(listView);
        }

        private void loadOrderScanningView()
        {
            BarcodeAPI scanner = new BarcodeAPI(barcodeScanner_OnStatus, barcodeScanner_OnScan);
        }

        private void loadCreateNewOrderView()
        {
            TextBox textBox;
            Label label;
            Button button;

            textBox = new TextBox();
            textBox.Name = "txtOrderSessionName";
            label = new Label();
            label.Text = "Order Name";
            button = FormNavigation.CreateButton("Save", FormNavigation.START_OR_CONTINUE_SCANNING, "TODO", handleFormControlEvents);

            this.mainContent.Controls.Add(textBox);
            this.mainContent.Controls.Add(label);
            this.mainContent.Controls.Add(button);

            textBox.Focus();

            styles.StyleSimpleForm(textBox, label, button);
        }

        private void loadScanResultView(ScanData scanData)
        {
            ListView listView;
            VendordDatabase db;
            VendordDatabase.Product product;

            db = new VendordDatabase();
            product = db.Products.First<VendordDatabase.Product>(p => p.UPC.Equals(scanData.Text));
            if (product != null)
            {
                listView = new ListView();
                listView.Columns.Add(new ColumnHeader() { Text = "Product" });
                listView.Items.Add(new ListViewItem(product.UPC));
                listView.Items.Add(new ListViewItem(product.Name));
                this.mainContent.Controls.Add(listView);
                styles.StyleListView(listView);
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

                case FormNavigation.CREATE_ORDER:
                    unloadCurrentView();
                    loadCreateNewOrderView();
                    break;

                case FormNavigation.START_OR_CONTINUE_SCANNING:                    
                    saveOrderSession();
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