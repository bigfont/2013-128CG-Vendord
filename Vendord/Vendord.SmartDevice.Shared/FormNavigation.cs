namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    public class FormNavigation
    {
        //
        // String constants
        //        
        internal const string HOME = "Home";
        internal const string ORDERS = "Orders";
        internal const string SYNC_HANDHELD = "Sync Handheld";
        internal const string SYNC_HANDHELD_TOOLTIP = "Update the product list on the handheld with the product list from IT Retail Professional.";
        internal const string COMPLETE_ORDER = "Complete Order";
        internal const string REPORTS = "Reports";
        internal const string PRODUCTS_REPORT = "Products";
        internal const string CLOSE = "Close";
        internal const string BACK = "Back";

        internal string LastAction;
        internal string CurrentView; // external code assigns to this
        private Form form;
        private FormStyles styles;

        internal Dictionary<string, string> UpstreamViewDictionary = new Dictionary<string, string>()
        {
            { ORDERS, HOME },
            { REPORTS, HOME },
            { PRODUCTS_REPORT, REPORTS }
        };

        internal FormNavigation(Form form)
        {
            LastAction = null;
            this.form = form;
            styles = new FormStyles(form);
        }

        internal void AddNavigationPanel(Form form, EventHandler handler)
        {
            Panel panel;
            Button btnBack;
            Button btnClose;

            panel = new Panel();

            btnBack = FormHelper.CreateButton(FormNavigation.BACK, "TODO", handler);
            btnClose = FormHelper.CreateButton(FormNavigation.CLOSE, "TODO", handler);

            if (LastAction == null)
            {
                btnBack.Enabled = false;
            }

            panel.Controls.Add(btnBack);
            panel.Controls.Add(btnClose);

            form.Controls.Add(panel);

            styles.StyleNavigationPanel(panel);
        }

        internal string GetUpstreamView()
        {
            string upstreamView;
            upstreamView = null;
            if (UpstreamViewDictionary.Keys.Contains<string>(this.LastAction))
            {
                upstreamView = UpstreamViewDictionary[this.LastAction];
            }
            return upstreamView;
        }

        internal void ParseActionFromEventSender(object sender)
        {
            string action;
            Control control;

            control = sender as Control;

            action = null;
            if (control.Name != null)
            {
                action = control.Name.ToString();

                if (action.Equals(BACK))
                {
                    action = GetUpstreamView();
                }
            }

            this.LastAction = action;
        }
    }
}
