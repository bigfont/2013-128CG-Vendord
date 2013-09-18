using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Vendord.SmartDevice.App
{
    public class FormNavigation
    {
        //
        // String constants
        //        
        internal const string HOME = "Home";
        internal const string ORDERS = "Orders";
        internal const string SYNC_HANDHELD = "Sync Handheld";
        internal const string REPORTS = "Reports";
        internal const string PRODUCTS_REPORT = "Products";
        internal const string CLOSE = "Close";
        internal const string BACK = "Back";
        internal const string BUTTON_PREFIX = "btn";
        internal const string PANEL_PREFIX = "pnl";

        internal string LastAction;
        internal string CurrentView;
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

            btnBack = FormHelper.CreateButtonWithEventHandler(FormNavigation.BACK, 0, handler);
            btnClose = FormHelper.CreateButtonWithEventHandler(FormNavigation.CLOSE, 1, handler);

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
            if (control.Tag != null)
            {
                action = control.Tag.ToString();

                if (action.Equals(BACK))
                {
                    action = GetUpstreamView();
                }
            }

            this.LastAction = action;
        }
    }
}
