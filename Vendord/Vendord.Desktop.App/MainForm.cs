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

    public class MainForm : Form
    {
        //
        // String constants
        //        
        private const string HOME = null;
        private const string SYNC_HANDHELD = "Sync Handheld";
        private const string EXIT = "Close";
        private const string BACK = "Back";
        private const string BUTTON_PREFIX = "btn";
        private const string PANEL_PREFIX = "pnl";

        //
        // State
        // 
        private string LastAction;

        private Dictionary<string, string> UpstreamActionDictionary = new Dictionary<string, string>() { };

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

        public MainForm()
        {                        
            LastAction = null;
            this.Load += handleFormControlEvents;
        }

        private void unloadCurrentView()
        {
            this.Controls.Clear();
        }

        private void loadHomeView()
        {
            Button btnSyncHandheld;

            btnSyncHandheld = CreateButtonWithEventHandler(SYNC_HANDHELD, 0, handleFormControlEvents);

            this.Controls.Add(btnSyncHandheld);
        }

        private void handleFormControlEvents(object sender, EventArgs e)
        {
            // get the action                    
            this.LastAction = ParseActionFromEvent(sender);

            // set the name of the form
            this.Text = String.Format("{0} {1}", this.Name, this.LastAction);

            // act based on the aciton
            switch (this.LastAction)
            {
                case SYNC_HANDHELD:
                    syncHandheld();
                    break;

                default:
                    unloadCurrentView();
                    loadHomeView();
                    break;
            }
        }

        private void syncHandheld()
        {
            SyncUtil sync = new SyncUtil();
            sync.SyncDesktopAndDeviceDatabases();

        }
    }
}
