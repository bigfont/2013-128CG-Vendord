namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    using System.Drawing;
    public class FormNavigation
    {
        // main nav
        internal const string HOME = "HOME";
        internal const string CLOSE = "CLOSE";
        internal const string BACK = "BACK";
        // orders
        internal const string ORDERS = "Orders";
        internal const string SYNC_HANDHELD = "SYNC_HANDHELD";
        internal const string CREATE_NEW_ORDER_SESSION = "CREATE_NEW_ORDER_SESSION";
        internal const string SAVE_AND_START_NEW_ORDER_SESSION = "SAVE_AND_START_NEW_ORDER_SESSION";
        internal const string CONTINUE_EXISTING_ORDER_SESSION = "CONTINUE_EXISTING_ORDER_SESSION";
        internal const string VIEW_ORDER_DETAILS = "VIEW_ORDER_DETAILS";
        internal const string INPUT_PRODUCT_AMOUNT = "INPUT_PRODUCT_AMOUNT";
        internal const string SAVE_AND_STOP_SCANNING = "SAVE_AND_STOP_SCANNING";
        internal const string VIEW_AND_EDIT_SCAN_RESULT = "VIEW_AND_EDIT_SCAN_RESULT";
        internal const string COMPLETE_ORDER = "COMPLETE_ORDER";
        // reports
        internal const string REPORTS = "Reports";
        internal const string PRODUCTS_REPORT = "PRODUCTS_REPORT";
        // inventory
        internal const string INVENTORY = "INVENTORY";
        // state
        internal string Action;
        internal string ActionSpecifier;
        internal string CurrentView; // external code assigns to this, that's why VS underlines it in green
        // dependencies
        private Form form;        

        internal Dictionary<string, string> UpstreamViewDictionary = new Dictionary<string, string>()
        {
            { ORDERS, HOME },
            { CONTINUE_EXISTING_ORDER_SESSION, ORDERS },
            { REPORTS, HOME },
            { PRODUCTS_REPORT, REPORTS },            
        };

        internal FormNavigation(Form form)
        {
            Action = null;
            this.form = form;            
        }

        internal Panel CreateMainNavigationPanel(EventHandler handler)
        {
            Panel panel;
            Button btnBack;
            Button btnClose;

            panel = new Panel();

            btnBack = FormNavigation.CreateButton("Back", FormNavigation.BACK, "TODO", Color.LightGreen, handler);
            btnClose = FormNavigation.CreateButton("Close", FormNavigation.CLOSE, "TODO", Color.Firebrick, handler);

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
                    if (listView.Tag == null)
                    {
                        action = selectedItem.Tag.ToString();
                        actionSpecifier = selectedItem.Text;
                    }
                    else
                    {
                        action = listView.Tag.ToString();
                    }
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
            listView.Name = name;

            if (eventHander != null)
            {
                listView.ItemActivate += eventHander;
            }
            if (caption != null)
            {
                // set caption
            }
            if (action != null)
            {
                listView.Tag = action;
            }

            return listView;
        }

        internal static Button CreateButton(string name, string action, string caption, Color backColor, EventHandler eventHandler)
        {
            Button b;
            b = new Button();
            b.Name = name;
            b.Text = name;
            b.Tag = action;
            b.Click += eventHandler;
            b.BackColor = backColor;
            return b;
        }
    }
}
