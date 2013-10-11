namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    internal static class FormHelper
    {
        internal static List<T> GetControlsByType<T>(Control controlToSearch, bool searchDescendents) where T : class
        {
            List<T> result;
            result = new List<T>();
            foreach (Control c in controlToSearch.Controls)
            {
                if (c.GetType() == typeof(T))
                {
                    result.Add(c as T);
                }
                if (searchDescendents)
                { 
                    result.AddRange(GetControlsByType<T>(c, true));
                }
            }
            return result;
        }

        internal static List<T> GetControlsByName<T>(
            Control controlToSearch, string nameOfControlsToFind, bool searchDescendants)
            where T : class
        {
            List<T> result;
            result = new List<T>();
            foreach (Control c in controlToSearch.Controls)
            {
                if (c.Name == nameOfControlsToFind && c.GetType() == typeof(T))
                {
                    result.Add(c as T);
                }
                if (searchDescendants)
                {
                    result.AddRange(GetControlsByName<T>(c, nameOfControlsToFind, true));
                }
            }
            return result;
        }

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
