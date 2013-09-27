namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    internal static class FormHelper
    {

#if FULL_FRAMEWORK

        internal static DataGridView CreateReadOnlyDataGridView(string action, DataGridViewCellValueEventHandler cellValueEventHandler)
        {
            DataGridView dataGridView;
           
            dataGridView = new DataGridView();            
            dataGridView.VirtualMode = true;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.CellValueNeeded += new DataGridViewCellValueEventHandler(cellValueEventHandler);

            dataGridView.Name = action;
            dataGridView.Tag = action;

            return dataGridView;
        }

#endif

    }
}
