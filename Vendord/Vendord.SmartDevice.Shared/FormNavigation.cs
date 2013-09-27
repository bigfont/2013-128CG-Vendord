namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    public class FormNavigation
    {
        // main nav
        internal const string HOME = "Home";
        internal const string CLOSE = "Close";
        internal const string BACK = "Back";
        // orders
        internal const string ORDERS = "Orders";
        internal const string SYNC_HANDHELD = "Sync Handheld";                
        internal const string CREATE_ORDER = "Create Order Session";
        internal const string START_OR_CONTINUE_SCANNING = "Order Session";
        internal const string VIEW_AND_EDIT_SCAN_RESULT = "Scan Result";
        internal const string COMPLETE_ORDER = "Complete Order";
        // reports
        internal const string REPORTS = "Reports";
        internal const string PRODUCTS_REPORT = "Products";
        // inventory
        internal const string INVENTORY = "Inventory";
        // state
        internal string Action;
        internal string ActionSpecifier;
        internal string CurrentView; // external code assigns to this
        // dependencies
        private Form form;
        private FormStyles styles;

        internal Dictionary<string, string> UpstreamViewDictionary = new Dictionary<string, string>()
        {
            { ORDERS, HOME },
            { START_OR_CONTINUE_SCANNING, ORDERS },
            { REPORTS, HOME },
            { PRODUCTS_REPORT, REPORTS },            
        };

        internal FormNavigation(Form form)
        {
            Action = null;
            this.form = form;
            styles = new FormStyles(form);
        }

        internal Panel CreateMainNavigationPanel(EventHandler handler)
        {
            Panel panel;
            Button btnBack;
            Button btnClose;

            panel = new Panel();

            btnBack = FormNavigation.CreateButton("Back", FormNavigation.BACK, "TODO", handler);
            btnClose = FormNavigation.CreateButton("Close", FormNavigation.CLOSE, "TODO", handler);

            panel.Controls.Add(btnBack);
            panel.Controls.Add(btnClose);            

            return panel;
        }

        internal string GetUpstreamView()
        {
            string upstreamView;
            upstreamView = null;
            if (UpstreamViewDictionary.Keys.Contains<string>(this.Action))
            {
                upstreamView = UpstreamViewDictionary[this.Action];
            }
            return upstreamView;
        }

        private void UpdateAction(string action, string actionSpecifier)
        {
            this.Action = action;
            this.ActionSpecifier = actionSpecifier;
        }

        internal void ParseActionFromSender(object sender)
        {            
            Type controlType;
            string action;
            string actionSpecifier;

            action = null;
            actionSpecifier = null;

            if (sender != null)
            {
                controlType = sender.GetType();
                if (controlType == typeof(System.Windows.Forms.Button) ||
                    controlType == typeof(System.Windows.Forms.Form))
                {                    
                    action = (sender as Control).Tag.ToString();
                    UpdateAction(action, actionSpecifier);   
                }
                else if (controlType == typeof(System.Windows.Forms.ListView))
                {
                    ListView listView;
                    int selectedIndex;
                    ListViewItem selectedItem;
                    listView = sender as ListView;
                    selectedIndex = listView.SelectedIndices[0];
                    selectedItem = listView.Items[selectedIndex];
                    action = selectedItem.Tag.ToString();
                    actionSpecifier = selectedItem.Text;
                    UpdateAction(action, actionSpecifier);
                }

                if (this.Action != null && this.Action.Equals(BACK))
                {
                    action = GetUpstreamView();
                    UpdateAction(action, actionSpecifier);
                }
            }            
        }

        internal static ListView CreateListView(string name, string action, string caption, EventHandler eventHander)
        {
            ListView listView;
            listView = new ListView();
            listView.Activation = ItemActivation.OneClick;
            listView.FullRowSelect = true;
            listView.ItemActivate += eventHander;
            listView.Name = name;
            listView.Tag = action;
            return listView;
        }

        internal static Button CreateButton(string name, string action, string caption, EventHandler eventHandler)
        {
            Button b;
            b = new Button();
            b.Name = name;            
            b.Text = name;
            b.Tag = action;
            b.Click += eventHandler;
            return b;
        }
    }
}
