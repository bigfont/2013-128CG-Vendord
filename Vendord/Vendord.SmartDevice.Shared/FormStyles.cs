namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Drawing;
    using System.Windows.Forms;

    // Resources
    // http://stackoverflow.com/questions/6466129/dock-anchor-and-fluid-layouts-in-windows-forms-applications
    // http://msdn.microsoft.com/en-us/library/ms951306.aspx (Windows Forms Layout)
    internal class FormStyles
    {
        private const int DATA_GRID_VIEWS_PER_ROW = 2;
        private const int NAVIGATION_PANEL_HEIGHT_RATIO = 1/5;
        private const int BUTTONS_PER_ROW = 2;
        private const int BUTTONS_PER_COLUMN = 2;
        private Form form;

        private int NavigationPanelHeight
        {
            get
            {
                return form.ClientSize.Height * NAVIGATION_PANEL_HEIGHT_RATIO;
            }
        }

        public FormStyles(Form form)
        {
            this.form = form;            
        }

        internal void StyleForm()
        {
            form.WindowState = FormWindowState.Maximized;
        }

        internal void StyleNavigationPanel(Panel panel)
        {
            panel.BringToFront();
            panel.Dock = DockStyle.Top;                     
            foreach (Button b in panel.Controls)
            {
                b.BringToFront();
                b.Dock = DockStyle.Left;
                b.Width = panel.ClientSize.Width / 2;                
            }
        }

        internal void StyleMainContentPanel(Panel panel)
        {
            panel.BringToFront();
            panel.Dock = DockStyle.Fill;
        }

        internal void StyleLargeButtons(Button[] buttons)
        {            
            foreach (Button b in buttons)
            {
                b.BringToFront();
                b.Dock = DockStyle.Top;
                b.Height = b.Parent.ClientSize.Height / buttons.Count();                
            }
        }

        internal void StyleListView(ListView listView)
        {
            listView.Location = new System.Drawing.Point(0, 0);
            listView.Size = form.ClientSize;
            listView.View = View.Details; // displays column headers            
            listView.FullRowSelect = true;
            listView.HeaderStyle = ColumnHeaderStyle.None;
            listView.BringToFront();
            listView.Dock = DockStyle.Top;
            for (int i = 0; i < listView.Columns.Count; ++i)
            {
                listView.Columns[i].Width = listView.ClientSize.Width;
            }
        }

        internal void StyleSimpleForm(TextBox textBox, Label label, Button button)
        {
            Control[] controls = new Control[] { label, textBox, button };
            foreach (Control c in controls)
            {
                c.BringToFront();
                c.Dock = DockStyle.Top;
                c.Height = c.Parent.ClientSize.Height / controls.Count();
            }
        }

#if FULL_FRAMEWORK

        private void ThemeDataGridView(DataGridView dataGridView)
        {            
            dataGridView.GridColor = Color.Blue;
            dataGridView.BorderStyle = BorderStyle.Fixed3D;
            dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Raised;
            dataGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Sunken;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;         
            dataGridView.DefaultCellStyle.BackColor = Color.AliceBlue;
        }

        private void SetDataGridViewColumnsEqualWidth(DataGridView dataGridView)
        {
            for (int i = 0; i < dataGridView.Columns.Count; ++i)
            {
                dataGridView.Columns[i].Width = dataGridView.ClientSize.Width / dataGridView.Columns.Count;
            }
        }

        internal void StyleDataGridViews(DataGridView[] dataGridViews)
        {
            int x;
            int y;

            x = 0;
            y = NavigationPanelHeight;

            for (int i = 0; i < dataGridViews.Length; ++i)
            {
                DataGridView dataGridView = dataGridViews[i];
                ThemeDataGridView(dataGridView);
                dataGridView.Width = form.ClientSize.Width / DATA_GRID_VIEWS_PER_ROW;
                dataGridView.Height = form.ClientSize.Height - NavigationPanelHeight;
                dataGridView.Location = new Point(x, y);
                SetDataGridViewColumnsEqualWidth(dataGridView);

                x += dataGridView.Width; // for the next one in the row
            }
        }

        internal void StyleDataGridView(DataGridView dataGridView)
        {
            ThemeDataGridView(dataGridView);                        

            // Position
            dataGridView.Location = new Point(0, NavigationPanelHeight);

            // Widths
            dataGridView.Width = form.ClientSize.Width;
            dataGridView.Height = form.ClientSize.Height - NavigationPanelHeight;
            SetDataGridViewColumnsEqualWidth(dataGridView);
        }

#endif

    }
}
