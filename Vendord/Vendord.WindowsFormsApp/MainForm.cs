namespace Vendord.WindowsFormsApp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Vendord.SQLServerCE;

    public class MainForm : Form
    {
        //
        // State
        // 
        private string LastAction;

        public MainForm()
        {
            Database db = new Database("VendordDB");
            IEnumerable<OrderSession> orderSessions = db.OrderSessions;

            LastAction = null;
            this.Load += handleFormControlEvents;
        }
    }
}
