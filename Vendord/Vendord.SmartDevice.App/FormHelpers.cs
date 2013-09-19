using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Vendord.SmartDevice.App
{
    internal static class FormHelper
    {
        internal static Button CreateButton(string name, EventHandler eventHandler)
        {
            Button b = new Button();
            b.Name = name;
            b.Text = name;            
            b.Click += eventHandler;
            return b;
        }

        internal static DataGridView CreateReadOnlyDataGridView(string name, DataGridViewCellValueEventHandler cellValueEventHandler)
        {
            DataGridView dataGridView;
           
            dataGridView = new DataGridView();            
            dataGridView.VirtualMode = true;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.CellValueNeeded += new DataGridViewCellValueEventHandler(cellValueEventHandler);

            dataGridView.Name = name;

            return dataGridView;
        }
    }
}
