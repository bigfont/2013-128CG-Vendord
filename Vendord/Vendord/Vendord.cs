using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Vendord
{
    public partial class VendordForm : VendordFormStyles
    {
        #region Fields

        //
        // String constants
        //        
        private const string HOME = null;
        private const string ORDERS = "Orders";
        private const string REPORTS = "Reports";
        private const string INVENTORY = "Inventory";
        private const string ORDER_SESSION = "Order Session";
        private const string EXIT = "Close";
        private const string BACK = "Back";
        private const string BUTTON_PREFIX = "btn";
        private const string PANEL_PREFIX = "pnl";

        //
        // Workflow
        // 

        private Dictionary<string, string> UpstreamActionDictionary = new Dictionary<string, string>() { 
            { ORDERS, HOME },
            { REPORTS, HOME }, 
            { INVENTORY, HOME },
            { ORDER_SESSION, ORDERS }

        };

        //
        // State
        // 
        private string LastAction;

        #endregion

        private string GetUpstreamAction()
        {
            string upstreamAction;
            upstreamAction = null;
            if (UpstreamActionDictionary.Keys.Contains<string>(this.LastAction))
            {
                upstreamAction = UpstreamActionDictionary[this.LastAction];
            }
            return upstreamAction;
        }

        private string ParseActionFromEvent(object sender)
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
                    action = GetUpstreamAction();
                }
            }

            return action;
        }

        private Button CreateButtonWithEventHandler(string action, int tabIndex, EventHandler eventHandler)
        {
            Button b = new Button();
            b.Name = BUTTON_PREFIX + action;
            b.Text = action;
            b.Tag = action;
            b.Click += eventHandler;
            return b;
        }

        public VendordForm()
        {
            LastAction = null;
            this.Load += handleEvent;
        }

        #region Views

        private void AddNavigationPanel()
        {
            Panel panel;
            Button btnBack;

            panel = new Panel();

            btnBack = CreateButtonWithEventHandler(BACK, 0, handleEvent);

            panel.Controls.Add(btnBack);

            StyleNavigationPanel(panel);

            this.Controls.Add(panel);
        }

        private void unloadCurrentView()
        {
            this.Controls.Clear();
        }

        private void loadHomeView()
        {
            Button btnOrders;
            Button btnReports;
            Button btnInventory;
            Button btnExit;

            btnOrders = new Button();
            btnReports = new Button();
            btnInventory = new Button();
            btnExit = new Button();

            btnOrders = CreateButtonWithEventHandler(ORDERS, 0, this.handleEvent);
            btnReports = CreateButtonWithEventHandler(REPORTS, 1, this.handleEvent);
            btnInventory = CreateButtonWithEventHandler(INVENTORY, 2, this.handleEvent);
            btnExit = CreateButtonWithEventHandler(EXIT, 3, this.handleEvent);

            this.Controls.Add(btnExit);
            this.Controls.Add(btnInventory);
            this.Controls.Add(btnReports);
            this.Controls.Add(btnOrders);

            StyleHomeViewButtons(new Button[] { btnOrders, btnReports, btnInventory, btnExit });
        }

        private void loadOrdersView()
        {
            ListView listView;
            ListViewItem listViewItem;

            AddNavigationPanel();

            listView = new ListView();
            listView.Activation = ItemActivation.OneClick;
            listView.FullRowSelect = true;
            listView.ItemActivate += handleEvent;

            // TODO Get this from the data source
            #region TODO
            ColumnHeader name = new ColumnHeader();
            name.Text = "Name";
            listView.Columns.Add(name);

            for (int i = 0; i < 15; ++i)
            {
                listViewItem = new ListViewItem();
                listViewItem.Text = "Foo" + i.ToString();
                listView.Items.Add(listViewItem);
                listView.Tag = ORDER_SESSION;
            }
            #endregion

            this.Controls.Add(listView);

            StyleListView(listView);
        }

        private void loadOrderSessionView()
        {
            AddNavigationPanel();
        }

        #endregion

        #region Event Handlers

        private void handleEvent(object sender, EventArgs e)
        {
            // get the action                    
            this.LastAction = ParseActionFromEvent(sender);

            // set the name of the form
            this.Text = String.Format("{0} {1}", this.Name, this.LastAction);

            // act based on the aciton
            switch (this.LastAction)
            {
                case ORDERS:
                    unloadCurrentView();
                    loadOrdersView();
                    break;

                case ORDER_SESSION:
                    unloadCurrentView();
                    loadOrderSessionView();
                    break;

                case EXIT:
                    this.Close();
                    return; // return because we want to avoid executing code after we close the form.  

                default:
                    unloadCurrentView();
                    loadHomeView();
                    break;
            }
        }

        #endregion
    }
}