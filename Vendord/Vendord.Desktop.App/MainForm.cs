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
            styles.StyleForm();
        }

        private void unloadCurrentView()
        {
            this.Controls.Clear();
            nav.AddNavigationPanel(this, handleFormControlEvents);
        }

        private void dataGridView_OrderSessionDetails_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            DataGridView dataGridView;
            string columnHeaderName;

            dataGridView = (sender as DataGridView);
            columnHeaderName = dataGridView.Columns[e.ColumnIndex].Name;

            switch (columnHeaderName)
            {
                case "OrderSessionID":
                    e.Value = db.OrderSession_Products[e.RowIndex].OrderSessionID;
                    break;

                case "ProductID":
                    e.Value = db.OrderSession_Products[e.RowIndex].ProductID;
                    break;


                case "CasesToOrder":
                    e.Value = db.OrderSession_Products[e.RowIndex].CasesToOrder;
                    break;

                default:
                    e.Value = "Default";
                    break;
            }
        }

        private void dataGridView_OrderSessions_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            DataGridView dataGridView;
            string columnHeaderName;

            dataGridView = (sender as DataGridView);
            columnHeaderName = dataGridView.Columns[e.ColumnIndex].Name;

            switch (columnHeaderName)
            {
                case "ID":
                    e.Value = db.OrderSessions[e.RowIndex].ID;
                    break;

                case "Name":
                    e.Value = db.OrderSessions[e.RowIndex].Name;
                    break;

                default:
                    e.Value = "Default";
                    break;
            }
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

            this.Controls.Add(dataGridView);

            styles.StyleDataGridView(dataGridView);

            nav.CurrentView = FormNavigation.PRODUCTS_REPORT;
        }

        private void loadReportsView()
        {
            Button btnProductsReport;
            btnProductsReport = FormHelper.CreateButton(FormNavigation.PRODUCTS_REPORT, "TODO", handleFormControlEvents);
            this.Controls.Add(btnProductsReport);
            styles.StyleLargeButtons(new Button[] { btnProductsReport });
            nav.CurrentView = FormNavigation.REPORTS;
        }

        private void loadCompleteOrderView()
        {
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

            this.Controls.Add(dataGridView_orderSessions);
            this.Controls.Add(dataGridView_orderSessionDetails);

            styles.StyleDataGridViews(new DataGridView[] { dataGridView_orderSessions, dataGridView_orderSessionDetails });

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

            btnSyncHandheld = FormHelper.CreateButton(FormNavigation.SYNC_HANDHELD, FormNavigation.SYNC_HANDHELD_TOOLTIP, handleFormControlEvents);
            btnCompleteOrder = FormHelper.CreateButton(FormNavigation.COMPLETE_ORDER, "TODO", handleFormControlEvents);

            this.Controls.Add(btnSyncHandheld);
            this.Controls.Add(btnCompleteOrder);

            styles.StyleLargeButtons(new Button[] { btnSyncHandheld, btnCompleteOrder });

            nav.CurrentView = FormNavigation.ORDERS;
        }

        private void loadHomeView()
        {
            Button btnOrders;
            Button btnReports;

            btnOrders = FormHelper.CreateButton(FormNavigation.ORDERS, "TODO", handleFormControlEvents);
            btnReports = FormHelper.CreateButton(FormNavigation.REPORTS, "TODO", handleFormControlEvents);

            this.Controls.Add(btnOrders);
            this.Controls.Add(btnReports);

            styles.StyleLargeButtons(new Button[] { btnOrders, btnReports });

            nav.CurrentView = FormNavigation.HOME;
        }

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
