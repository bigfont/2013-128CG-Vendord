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
    public partial class Vendord : ResponsiveForm
    {
        public Vendord()
        {
            InitializeComponent();
        }

        #region Event Handlers

        private void Vendord_Load(object sender, EventArgs e)
        {
            SetFormSizeAndLocation();
            SetButtonSizesAndLocations(new Button[] { button1, button2, button3 });
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        #endregion


    }
}