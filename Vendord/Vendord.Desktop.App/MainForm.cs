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
    using Vendord.SmartDevice.DAL;
    using System.Collections.ObjectModel;
    using Vendord.SmartDevice.App;

    public class MainForm : Form
    {
        FormNavigation nav;
        FormStyles styles;
        Database db;

        Database.Product product = new Database.Product();

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

        private void dataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
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

                default:
                    e.Value = "Default";
                    break;
            }
        }

        private void loadProductsReportView()
        {
            db = new Database(Constants.DATABASE_NAME);
            IEnumerable<Database.Product> products;
            DataGridView dataGridView;

            db = new Database(Constants.DATABASE_NAME);
            products = db.Products;

            dataGridView = new DataGridView();
            dataGridView.VirtualMode = true;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.AllowUserToAddRows = false;           

            dataGridView.Columns.Add("ID", "ID");
            dataGridView.Columns.Add("Name", "Name");

            dataGridView.RowCount = products.Count();

            dataGridView.CellValueNeeded += new DataGridViewCellValueEventHandler(dataGridView_CellValueNeeded);            

            this.Controls.Add(dataGridView);

            styles.StyleDataGridView(dataGridView);

            nav.CurrentView = FormNavigation.PRODUCTS_REPORT;
        }

        private void loadReportsView()
        {
            Button btnProductsReport;
            btnProductsReport = FormHelper.CreateButtonWithEventHandler(FormNavigation.PRODUCTS_REPORT, 0, handleFormControlEvents);
            this.Controls.Add(btnProductsReport);
            styles.StyleLargeButtons(new Button[] { btnProductsReport });
            nav.CurrentView = FormNavigation.REPORTS;
        }

        private void syncHandheld()
        {
            DatabaseSync sync;
            DatabaseSync.SyncResultMessage syncResult;

            sync = new DatabaseSync();
            syncResult = sync.SyncDesktopAndDeviceDatabases();

            MessageBox.Show(syncResult.Caption, syncResult.Message, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void loadOrdersView()
        {
            Button btnSyncHandheld;

            btnSyncHandheld = FormHelper.CreateButtonWithEventHandler(FormNavigation.SYNC_HANDHELD, 0, handleFormControlEvents);

            this.Controls.Add(btnSyncHandheld);

            styles.StyleLargeButtons(new Button[] { btnSyncHandheld });

            nav.CurrentView = FormNavigation.ORDERS;
        }

        private void loadHomeView()
        {
            Button btnOrders;
            Button btnReports;

            btnOrders = FormHelper.CreateButtonWithEventHandler(FormNavigation.ORDERS, 0, handleFormControlEvents);
            btnReports = FormHelper.CreateButtonWithEventHandler(FormNavigation.REPORTS, 1, handleFormControlEvents);

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
