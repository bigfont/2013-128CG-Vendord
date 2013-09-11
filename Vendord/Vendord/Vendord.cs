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
    public class Vendord : ResponsiveForm
    {        
        public Vendord()
        {
            InitializeComponent();
        }

        #region User Interface

        //
        // String constants
        //
        private const string EXIT = "Close";
        private const string ORDERS = "Orders";
        private const string REPORTS = "Reports";
        private const string INVENTORY = "Inventory";

        // 
        // Controls
        //
        private System.Windows.Forms.Button btnOrders;
        private System.Windows.Forms.Button btnReports;
        private System.Windows.Forms.Button btnInventory;
        private System.Windows.Forms.Button btnExit;

        private void InitializeComponent()
        {
            //
            // Avoid the layout system repeatedly reactity to changes
            //
            this.SuspendLayout();
          
            //
            // Buttons
            //
            this.btnOrders = new System.Windows.Forms.Button();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnInventory = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();  

            this.btnOrders = CreateButtonWithEventHandler(ORDERS, 0, this.btn_Click);
            this.btnReports = CreateButtonWithEventHandler(REPORTS, 1, this.btn_Click);
            this.btnInventory = CreateButtonWithEventHandler(INVENTORY, 2, this.btn_Click);
            this.btnExit = CreateButtonWithEventHandler(EXIT, 3, this.btn_Click);

            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnInventory);
            this.Controls.Add(this.btnReports);
            this.Controls.Add(this.btnOrders);
            
            SetButtonSizesAndLocations(new Button[] { btnOrders, btnReports, btnInventory, btnExit });

            // 
            // Vendord
            // 

            this.Name = "Vendord";
            this.Text = "Vendord";
            this.Load += new System.EventHandler(this.Vendord_Load);

            SetFormSizeAndLocation();

            //
            // Layout
            //
            this.ResumeLayout(false);

        }       

        #endregion

        #region Event Handlers

        private void ChangeFormTextProperty(string area)
        {
            this.Text = String.Format("{0} {1}", this.Name, area);
        }

        private void unloadCurrentView()
        {
            this.Controls.Clear();
        }

        private void loadOrdersView()
        {
            
        }

        private void Vendord_Load(object sender, EventArgs e)
        {
            ChangeFormTextProperty("Home");
        }        

        private void btn_Click(object sender, EventArgs e)
        {
            string btnText;
            
            btnText = (sender as Button).Text;
            switch(btnText)
            {
                case EXIT:
                    this.Close();
                    break;

                case ORDERS:
                    unloadCurrentView();
                    loadOrdersView();
                    break;

                default:
                    break;
            }

            ChangeFormTextProperty(btnText);
        }

        #endregion

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}