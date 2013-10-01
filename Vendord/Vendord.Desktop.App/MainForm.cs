namespace Vendord.Desktop.App
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Collections.ObjectModel;
    using Vendord.SmartDevice.Shared;

    public class MainForm : Form
    {
        internal FormNavigation nav;
        internal FormStyles styles;
        internal Panel mainNavigation;
        internal Panel mainContent;

        // order viewing state
        private VendordDatabase.OrderSession currentOrderSession = new VendordDatabase.OrderSession();
        private VendordDatabase.Product currentProduct = new VendordDatabase.Product();
        private VendordDatabase.OrderSession_Product currentOrderSession_Product = new VendordDatabase.OrderSession_Product();

        // database
        private VendordDatabase _db;
        private VendordDatabase db
        {
            get
            {
                if (_db == null)
                {
                    _db = new VendordDatabase();
                }
                return _db;
            }
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

        private void unloadCurrentView()
        {
            this.mainContent.Controls.Clear();
        }

        private void dataGridView_Products_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            DataGridView dataGridView;
            string columnHeaderName;

            dataGridView = (sender as DataGridView);
            columnHeaderName = dataGridView.Columns[e.ColumnIndex].Name;

            switch (columnHeaderName)
            {
                case "ID":
                    e.Value = db.Products[e.RowIndex].ID;
                    break;

                case "Name":
                    e.Value = db.Products[e.RowIndex].Name;
                    break;

                case "UPC":
                    e.Value = db.Products[e.RowIndex].UPC;
                    break;

                default:
                    e.Value = "Default";
                    break;
            }
        }

        private void loadProductsReportView()
        {
            DataGridView dataGridView = FormHelper.CreateReadOnlyDataGridView(null, dataGridView_Products_CellValueNeeded);

            dataGridView.Columns.Add("ID", "ID");
            dataGridView.Columns.Add("Name", "Name");
            dataGridView.Columns.Add("UPC", "UPC");
            dataGridView.RowCount = db.Products.Count(); // add RowCount after adding columns, lest we get an extra column

            this.mainContent.Controls.Add(dataGridView);

            styles.StyleDataGridView(dataGridView);

            nav.CurrentView = FormNavigation.PRODUCTS_REPORT;
        }

        private void loadReportsView()
        {
            Button btnProductsReport;
            btnProductsReport = FormNavigation.CreateButton("Products", FormNavigation.PRODUCTS_REPORT, "TODO", handleFormControlEvents);
            this.mainContent.Controls.Add(btnProductsReport);
            styles.StyleLargeButtons(new Button[] { btnProductsReport });
            nav.CurrentView = FormNavigation.REPORTS;
        }

        private ListView createOrderSessionDetailsListView()
        {
            ListView listViewDetails;
            ListViewItem listViewItem;
            VendordDatabase.Product product;

            listViewDetails = new ListView();
            listViewDetails.View = View.Details;

            // columns are required in View.Details
            listViewDetails.Columns.Add("Product");
            listViewDetails.Columns.Add("Cases to Order");

            foreach (VendordDatabase.OrderSession_Product order_product in db.OrderSession_Products.Where(i => i.OrderSessionID == currentOrderSession.ID))
            {
                product = db.Products.First(p => p.ID == order_product.ProductID);

                listViewItem = new ListViewItem(product.Name);                
                listViewItem.SubItems.Add(order_product.CasesToOrder.ToString());
                listViewDetails.Items.Add(listViewItem);
            }
            return listViewDetails;
        }

        private ListView createOrderSessionMasterListView()
        {
            ListView listViewMaster;            
            ListViewItem listViewItem;
            bool isFirst;

            listViewMaster = new ListView();
            listViewMaster.View = View.Details;

            // columns are required in View.Details
            listViewMaster.Columns.Add("Order Session");

            isFirst = true;
            foreach (VendordDatabase.OrderSession orderSession in db.OrderSessions)
            {
                listViewItem = new ListViewItem(orderSession.Name);
                listViewMaster.Items.Add(listViewItem);
                if (isFirst)
                {
                    listViewItem.Selected = true;
                    currentOrderSession = orderSession;
                }
            }                        

            return listViewMaster;
        }

        private void loadCompleteOrderView()
        {            
            ListView listViewMaster;
            ListView listViewDetails;            
            Control[] controls;

            listViewMaster = createOrderSessionMasterListView();
            listViewDetails = createOrderSessionDetailsListView();

            controls = new Control[] { listViewMaster, listViewDetails }.Reverse().ToArray();
            foreach (Control c in controls)
            {
                c.Dock = DockStyle.Top;
                mainContent.Controls.Add(c);
            }                                              

            /*
            
            DataGridView dataGridView_orderSessions;
            DataGridView dataGridView_orderSessionDetails;

            dataGridView_orderSessions = FormHelper.CreateReadOnlyDataGridView(null, dataGridView_OrderSessions_CellValueNeeded);
            dataGridView_orderSessionDetails = FormHelper.CreateReadOnlyDataGridView(null, dataGridView_OrderSessionDetails_CellValueNeeded);

            dataGridView_orderSessions.Columns.Add("Name", "Order Session Name");
            dataGridView_orderSessions.RowCount = db.OrderSessions.Count();

            dataGridView_orderSessionDetails.Columns.Add("Name", "Product Name");
            dataGridView_orderSessionDetails.Columns.Add("CasesToOrder", "Cases to Order");
            dataGridView_orderSessionDetails.RowCount = db.OrderSession_Products.Count(r =>
                r.OrderSessionID == db.OrderSessions.FirstOrDefault().ID);

            this.mainContent.Controls.Add(dataGridView_orderSessions);
            this.mainContent.Controls.Add(dataGridView_orderSessionDetails);

            styles.StyleDataGridViews(new DataGridView[] { dataGridView_orderSessions, dataGridView_orderSessionDetails });
            
            */

            nav.CurrentView = FormNavigation.COMPLETE_ORDER;
        }

        private void syncHandheld()
        {
            DatabaseSync sync;
            DatabaseSync.SyncResultMessage syncResult;

            sync = new DatabaseSync();
            syncResult = sync.SyncDesktopAndITRetailDatabase();
            syncResult = sync.SyncDesktopAndDeviceDatabases();

            MessageBox.Show(syncResult.Caption, syncResult.Message, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void loadOrdersView()
        {
            Button btnSyncHandheld;
            Button btnCompleteOrder;

            btnSyncHandheld = FormNavigation.CreateButton("Sync with IT Retail", FormNavigation.SYNC_HANDHELD, "TODO", handleFormControlEvents);
            btnCompleteOrder = FormNavigation.CreateButton("Complete Order", FormNavigation.COMPLETE_ORDER, "TODO", handleFormControlEvents);

            this.mainContent.Controls.Add(btnSyncHandheld);
            this.mainContent.Controls.Add(btnCompleteOrder);

            styles.StyleLargeButtons(new Button[] { btnSyncHandheld, btnCompleteOrder });

            nav.CurrentView = FormNavigation.ORDERS;
        }

        private void loadHomeView()
        {
            Button btnOrders;
            Button btnReports;

            btnOrders = FormNavigation.CreateButton("Orders", FormNavigation.ORDERS, "TODO", handleFormControlEvents);
            btnReports = FormNavigation.CreateButton("Reports", FormNavigation.REPORTS, "TODO", handleFormControlEvents);

            this.mainContent.Controls.Add(btnOrders);
            this.mainContent.Controls.Add(btnReports);

            styles.StyleLargeButtons(new Button[] { btnOrders, btnReports });

            nav.CurrentView = FormNavigation.HOME;
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

                case FormNavigation.SYNC_HANDHELD:
                    syncHandheld();
                    break;

                case FormNavigation.COMPLETE_ORDER:
                    unloadCurrentView();
                    syncHandheld();
                    loadCompleteOrderView();
                    break;

                case FormNavigation.REPORTS:
                    unloadCurrentView();
                    loadReportsView();
                    break;

                case FormNavigation.PRODUCTS_REPORT:
                    unloadCurrentView();
                    loadProductsReportView();
                    break;

                case FormNavigation.CLOSE:
                    this.Close();
                    return;

                default:
                    unloadCurrentView();
                    loadHomeView();
                    break;
            }
        }
    }
}
