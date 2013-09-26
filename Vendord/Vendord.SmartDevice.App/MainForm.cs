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

    public partial class MainForm : MainFormStyles
    {
        internal FormNavigation nav;
        internal FormStyles styles;

        public MainForm()
        {
            this.Load += handleFormControlEvents;
            nav = new FormNavigation(this);
            styles = new FormStyles(this);
            styles.StyleForm();
        }

        #region Views

        private void unloadCurrentView()
        {
            this.Controls.Clear();
            nav.AddNavigationPanel(this, handleFormControlEvents);
        }

        private void loadHomeView()
        {
            Button btnOrders;

            btnOrders = FormHelper.CreateButton(FormNavigation.ORDERS, "TODO", handleFormControlEvents);

            this.Controls.Add(btnOrders);

            styles.StyleLargeButtons(new Button[] { btnOrders });

            nav.CurrentView = FormNavigation.HOME;
        }

        private void loadOrdersView()
        {
            ListView listView;
            ListViewItem listViewItem;

            listView = new ListView();
            listView.Activation = ItemActivation.OneClick;
            listView.FullRowSelect = true;
            listView.ItemActivate += handleFormControlEvents;

            // TODO Get the list of orders from the data source
            #region TODO
            ColumnHeader name = new ColumnHeader();
            name.Text = "Name";
            listView.Columns.Add(name);

            for (int i = 0; i < 15; ++i)
            {
                listViewItem = new ListViewItem();
                listViewItem.Text = "Foo" + i.ToString();
                listView.Items.Add(listViewItem);
                listView.Name = FormNavigation.ORDER_SESSION;
            }
            #endregion

            this.Controls.Add(listView);

            StyleListView(listView);
        }

        private void loadOrderSessionView()
        {
            BarcodeAPI scanner = new BarcodeAPI(barcodeScanner_OnStatus, barcodeScanner_OnScan);
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
                this.Controls.Add(listView);
                StyleListView(listView);
            }
            nav.CurrentView = FormNavigation.SCAN_RESULT;
        }

        #endregion

        #region Event Handlers

        private void handleFormControlEvents(object sender, EventArgs e)
        {
            // set last action
            nav.ParseActionFromEventSender(sender);

            // set the name of the form
            this.Text = String.Format("{0} {1}", this.Name, "");

            // act based on the aciton
            switch (nav.LastAction)
            {
                case FormNavigation.ORDERS:
                    unloadCurrentView();
                    loadOrdersView();
                    break;

                case FormNavigation.ORDER_SESSION:
                    unloadCurrentView();
                    loadOrderSessionView();
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