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
        }

        private void loadProductsReport()
        {
            Database db = new Database(Constants.DATABASE_NAME);
            IEnumerable<Product> products = db.Products;

            DataGridView dataGridView = new DataGridView();
            dataGridView.DataSource = products;

            this.Controls.Add(dataGridView);

            styles.StyleDataGridView(dataGridView);
        }

        private void loadReportsView()
        {
            Button btnProductsReport;
            btnProductsReport = FormHelper.CreateButtonWithEventHandler(FormNavigation.PRODUCTS_REPORT, 0, handleFormControlEvents);
            this.Controls.Add(btnProductsReport);
            styles.StyleLargeButtons(new Button[] { btnProductsReport });
        }

        private void syncHandheld()
        {
            DatabaseSync sync = new DatabaseSync();
            sync.SyncDesktopAndDeviceDatabases();
        }

        private void loadOrdersView()
        {
            Button btnSyncHandheld;

            btnSyncHandheld = FormHelper.CreateButtonWithEventHandler(FormNavigation.SYNC_HANDHELD, 0, handleFormControlEvents);

            this.Controls.Add(btnSyncHandheld);

            styles.StyleLargeButtons(new Button[] { btnSyncHandheld });
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
                    nav.AddNavigationPanel(this, handleFormControlEvents);
                    loadOrdersView();
                    break;

                case FormNavigation.SYNC_HANDHELD:
                    syncHandheld();
                    break;

                case FormNavigation.REPORTS:
                    unloadCurrentView();
                    nav.AddNavigationPanel(this, handleFormControlEvents);
                    loadReportsView();
                    break;

                case FormNavigation.PRODUCTS_REPORT:
                    unloadCurrentView();
                    nav.AddNavigationPanel(this, handleFormControlEvents);
                    loadProductsReport();
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
