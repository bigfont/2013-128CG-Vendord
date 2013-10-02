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
        internal Panel mainNavigation;
        internal Panel mainContent;

        private static class UserInputControlNames
        {
            internal const string LV_MASTER = "lvMaster";
            internal const string LV_DETAILS = "lvDetails";
        }

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

            mainNavigation = nav.CreateMainNavigationPanel(handleFormControlEvents);
            mainContent = new Panel();

            this.Controls.Add(mainNavigation);
            this.Controls.Add(mainContent);

            this.WindowState = FormWindowState.Maximized;

            mainNavigation.BringToFront();
            mainNavigation.Dock = DockStyle.Top;
            foreach (Button b in mainNavigation.Controls)
            {
                b.BringToFront();
                b.Dock = DockStyle.Left;
                b.Width = mainNavigation.ClientSize.Width / 2;
            }

            mainContent.BringToFront();
            mainContent.Dock = DockStyle.Fill;
        }

        #region Utilities

        private void NotImplemented()
        {
            MessageBox.Show("Coming soon", "This feature is not complete.", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void syncHandheld()
        {
            DatabaseSync sync;
            DatabaseSync.SyncResultMessage syncResult;

            sync = new DatabaseSync();
            syncResult = sync.SyncDesktopAndITRetailDatabase();
            syncResult = sync.SyncDesktopAndDeviceDatabases();

            MessageBox.Show(syncResult.Caption, syncResult.Message, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void addDataToListViewDetails(ListView listViewDetails, int orderSessionID)
        {
            ListViewItem listViewItem;
            VendordDatabase.Product product;
            listViewDetails.Items.Clear();
            foreach (VendordDatabase.OrderSession_Product order_product in db.OrderSession_Products.Where(i => i.OrderSessionID == orderSessionID))
            {
                product = db.Products.First(p => p.ID == order_product.ProductID);

                listViewItem = new ListViewItem(product.Name);
                listViewItem.SubItems.Add(order_product.CasesToOrder.ToString());
                listViewDetails.Items.Add(listViewItem);
            }
        }

        private ListView createOrderSessionDetailsListView(int orderSessionID)
        {
            ListView listViewDetails;

            listViewDetails = FormNavigation.CreateListView(UserInputControlNames.LV_DETAILS, null, null, null);
            listViewDetails.View = View.Details;

            // columns are required in View.Details
            listViewDetails.Columns.Add("Product");
            listViewDetails.Columns.Add("Cases to Order");

            // return 
            return listViewDetails;
        }

        private void addDataToListViewMaster(ListView listViewMaster)
        {
            ListViewItem listViewItem;
            // add list view items            
            foreach (VendordDatabase.OrderSession orderSession in db.OrderSessions)
            {
                listViewItem = new ListViewItem(orderSession.Name);
                listViewItem.SubItems.Add(orderSession.ID.ToString());
                listViewMaster.Items.Add(listViewItem);
            }
        }

        private ListView createOrderSessionMasterListView()
        {
            ListView listViewMaster;

            listViewMaster = FormNavigation.CreateListView(UserInputControlNames.LV_MASTER, FormNavigation.VIEW_ORDER_DETAILS, "TODO", handleFormControlEvents);
            listViewMaster.View = View.LargeIcon; // nice to have - tweak this

            // columns are required in View.Details
            listViewMaster.Columns.Add("Order Session");

            // add data
            addDataToListViewMaster(listViewMaster);

            // return
            return listViewMaster;
        }

        #endregion

        #region Views

        private void unloadCurrentView()
        {
            this.mainContent.Controls.Clear();
        }

        private void loadProductsReportView()
        {
            NotImplemented();
        }

        private void loadReportsView()
        {
            Button btnProductsReport;
            Button[] buttons; 

            btnProductsReport = FormNavigation.CreateButton("Products", FormNavigation.PRODUCTS_REPORT, "TODO", Color.LightGreen, handleFormControlEvents);
            this.mainContent.Controls.Add(btnProductsReport);
            
            
            buttons = new Button[] { btnProductsReport };
            foreach (Button b in buttons)
            {
                b.BringToFront();
                b.Dock = DockStyle.Top;
                b.Height = b.Parent.ClientSize.Height / buttons.Count();
            }

            nav.CurrentView = FormNavigation.REPORTS;
        }

        private void updateCompleteOrderView()
        {
            ListView listViewMaster;
            ListView listViewDetails;
            int orderSessionID;

            listViewMaster = FormHelper.GetControlsByName<ListView>(this, UserInputControlNames.LV_MASTER, true).First<ListView>();
            orderSessionID = Convert.ToInt32(listViewMaster.SelectedItems[0].SubItems[1].Text);

            listViewDetails = FormHelper.GetControlsByName<ListView>(this, UserInputControlNames.LV_DETAILS, true).First<ListView>();
            addDataToListViewDetails(listViewDetails, orderSessionID);
        }

        private void loadCompleteOrderView()
        {
            ListView listViewMaster;
            ListView listViewDetails;
            int orderSessionID;
            Control[] controls;

            listViewMaster = createOrderSessionMasterListView();
            orderSessionID = Convert.ToInt32(listViewMaster.Items[0].SubItems[1].Text);
            listViewDetails = createOrderSessionDetailsListView(orderSessionID);
            addDataToListViewDetails(listViewDetails, orderSessionID);

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

        private void loadOrdersView()
        {
            Button btnSyncHandheld;
            Button btnCompleteOrder;
            Button[] buttons;

            btnSyncHandheld = FormNavigation.CreateButton("Sync with IT Retail", FormNavigation.SYNC_HANDHELD, "TODO", Color.LightGreen, handleFormControlEvents);
            btnCompleteOrder = FormNavigation.CreateButton("Complete Order", FormNavigation.COMPLETE_ORDER, "TODO", Color.LightGreen, handleFormControlEvents);

            this.mainContent.Controls.Add(btnSyncHandheld);
            this.mainContent.Controls.Add(btnCompleteOrder);

            buttons = new Button[] { btnSyncHandheld, btnCompleteOrder };
            foreach (Button b in buttons)
            {
                b.BringToFront();
                b.Dock = DockStyle.Top;
                b.Height = b.Parent.ClientSize.Height / buttons.Count();
            }

            nav.CurrentView = FormNavigation.ORDERS;
        }

        private void loadHomeView()
        {
            Button btnOrders;
            Button btnReports;
            Button[] buttons;

            btnOrders = FormNavigation.CreateButton("Orders", FormNavigation.ORDERS, "TODO", Color.LightGreen, handleFormControlEvents);
            btnReports = FormNavigation.CreateButton("Reports", FormNavigation.REPORTS, "TODO", Color.LightGreen, handleFormControlEvents);

            this.mainContent.Controls.Add(btnOrders);
            this.mainContent.Controls.Add(btnReports);

            buttons = new Button[] { btnOrders, btnReports };
            foreach (Button b in buttons)
            {
                b.BringToFront();
                b.Dock = DockStyle.Top;
                b.Height = b.Parent.ClientSize.Height / buttons.Count();
            }

            nav.CurrentView = FormNavigation.HOME;
        }

        #endregion

        #region Events

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

                case FormNavigation.VIEW_ORDER_DETAILS:
                    updateCompleteOrderView();
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

        #endregion
    }
}
