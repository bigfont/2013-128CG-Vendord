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

    public class MainForm : MainFormStyles
    {
        //
        // String constants
        //        
        private const string HOME = null;
        private const string ORDERS = "Orders";
        private const string SYNC_HANDHELD = "Sync Handheld";
        private const string REPORTS = "Reports";
        private const string PRODUCTS_REPORT = "Products";
        private const string CLOSE = "Close";
        private const string BACK = "Back";
        private const string BUTTON_PREFIX = "btn";
        private const string PANEL_PREFIX = "pnl";

        //
        // State
        // 
        private string lastAction;

        private Dictionary<string, string> upstreamActionDictionary = new Dictionary<string, string>()
        {
            { ORDERS, HOME },
            { REPORTS, HOME },
            { PRODUCTS_REPORT, REPORTS }

        };

        private string getUpstreamAction()
        {
            string upstreamAction;
            upstreamAction = null;
            if (upstreamActionDictionary.Keys.Contains<string>(this.lastAction))
            {
                upstreamAction = upstreamActionDictionary[this.lastAction];
            }
            return upstreamAction;
        }

        private string parseActionFromEvent(object sender)
        {
            string action;
            Control control;

            control = sender as Control;

            action = null;
            if (control.Tag != null)
            {
                action = control.Tag.ToString();

                if (action.Equals(BACK))
                {
                    action = getUpstreamAction();
                }
            }

            return action;
        }

        private Button createButtonWithEventHandler(string action, int tabIndex, EventHandler eventHandler)
        {
            Button b = new Button();
            b.Name = BUTTON_PREFIX + action;
            b.Text = action;
            b.Tag = action;
            b.Click += eventHandler;
            return b;
        }

        public MainForm()
        {
            lastAction = null;
            this.Load += handleFormControlEvents;
        }

        private void addNavigationPanel()
        {
            Panel panel;
            Button btnBack;
            Button btnClose;

            panel = new Panel();

            btnBack = createButtonWithEventHandler(BACK, 0, handleFormControlEvents);
            btnClose = createButtonWithEventHandler(CLOSE, 1, handleFormControlEvents);

            panel.Controls.Add(btnBack);
            panel.Controls.Add(btnClose);

            this.Controls.Add(panel);
            
            StyleNavigationPanel(panel);
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

            StyleDataGridView(dataGridView);
        }

        private void loadReportsView()
        {
            Button btnProductsReport;
            btnProductsReport = createButtonWithEventHandler(PRODUCTS_REPORT, 0, handleFormControlEvents);
            this.Controls.Add(btnProductsReport);
            StyleLargeButtons(new Button[] { btnProductsReport });
        }

        private void syncHandheld()
        {
            DatabaseSync sync = new DatabaseSync();
            sync.SyncDesktopAndDeviceDatabases();
        }

        private void loadOrdersView()
        {
            Button btnSyncHandheld;

            btnSyncHandheld = createButtonWithEventHandler(SYNC_HANDHELD, 0, handleFormControlEvents);

            this.Controls.Add(btnSyncHandheld);

            StyleLargeButtons(new Button[] { btnSyncHandheld });
        }

        private void loadHomeView()
        {
            Button btnOrders;
            Button btnReports;

            btnOrders = createButtonWithEventHandler(ORDERS, 0, handleFormControlEvents);
            btnReports = createButtonWithEventHandler(REPORTS, 1, handleFormControlEvents);

            this.Controls.Add(btnOrders);
            this.Controls.Add(btnReports);

            StyleLargeButtons(new Button[] { btnOrders, btnReports });
        }

        private void handleFormControlEvents(object sender, EventArgs e)
        {
            // get the action                    
            this.lastAction = parseActionFromEvent(sender);

            // set the name of the form
            this.Text = String.Format("{0} {1}", this.Name, this.lastAction);

            // act based on the aciton
            switch (this.lastAction)
            {
                case ORDERS:
                    unloadCurrentView();
                    addNavigationPanel();
                    loadOrdersView();
                    break;

                case SYNC_HANDHELD:
                    syncHandheld();
                    break;

                case REPORTS:
                    unloadCurrentView();
                    addNavigationPanel();
                    loadReportsView();
                    break;

                case PRODUCTS_REPORT:
                    unloadCurrentView();
                    addNavigationPanel();
                    loadProductsReport();
                    break;

                case CLOSE:
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
