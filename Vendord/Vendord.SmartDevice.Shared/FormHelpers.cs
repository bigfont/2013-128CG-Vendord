namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    internal static class FormHelper
    {
        internal static Button CreateButton(string name, string caption, EventHandler eventHandler)
        {
            Button b;
            
            b = new Button();
            b.Name = name;
            b.Text = name;            
            b.Click += eventHandler;


#if FULL_FRAMEWORK

            ToolTip t;
            t = new ToolTip();
            t.SetToolTip(b, caption);

#endif

            return b;
        }

#if FULL_FRAMEWORK

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

#endif 

    }
}
