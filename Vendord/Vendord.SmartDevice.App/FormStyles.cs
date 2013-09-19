using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Vendord.SmartDevice.App
{
    internal class FormStyles
    {
        private const int DATA_GRID_VIEWS_PER_ROW = 2;
        private const int NAVIGATION_PANELS_PER_ROW = 5;
        private const int BUTTONS_PER_ROW = 2;
        private const int BUTTONS_PER_COLUMN = 2;
        private const int emSizeLarge = 20;
        private const int emSizeMedium = 16;
        private const int emSizeSmall = 12;
        private Font fontSmall;
        private Font fontDefault;
        private Font fontLarge;
        private Form form;

        public int NavigationPanelHeight
        {
            get
            {
                return form.ClientSize.Height / NAVIGATION_PANELS_PER_ROW;
            }
        }

        public FormStyles(Form form)
        {
            this.form = form;
            SetFonts();
        }

        internal void SetFonts()
        {
            FontFamily family;
            FontStyle style;

            family = FontFamily.GenericSansSerif;
            style = FontStyle.Regular;

            this.fontLarge = new Font(family, emSizeLarge, style);
            this.fontDefault = new Font(family, emSizeMedium, style);
            this.fontSmall = new Font(family, emSizeSmall, style);
        }

        internal void StyleForm()
        {
            form.WindowState = FormWindowState.Maximized;
            form.Font = fontDefault;
        }

        internal void StyleNavigationPanel(Panel panel)
        {
            Size panelSize;
            Size buttonSize;

            panelSize = new Size(form.ClientSize.Width, NavigationPanelHeight);
            buttonSize = new Size(form.ClientSize.Width / BUTTONS_PER_ROW, NavigationPanelHeight);

            panel.Size = panelSize;

            int x = 0;
            int y = 0;
            foreach (Button b in panel.Controls)
            {
                b.Size = buttonSize;
                b.Font = fontDefault;
                b.Location = new Point(x, y);
                x += b.Width;
            }
        }

        internal void StyleLargeButtons(Button[] buttons)
        {
            int width;
            int height;
            int x;
            int y;
            System.Drawing.Size size;
            Button b;

            height = form.ClientSize.Height / BUTTONS_PER_COLUMN;
            width = form.ClientSize.Width / BUTTONS_PER_ROW;
            size = new System.Drawing.Size(width, height);

            x = 0;
            y = NavigationPanelHeight;

            for (int i = 0; i < buttons.Length; ++i)
            {
                // set the size
                b = buttons[i];

                b.Size = size;
                b.Location = new System.Drawing.Point(x, y);

                if (i == 0 || i % BUTTONS_PER_ROW == 0)
                {
                    x += width;
                }
                else
                {
                    x = 0;
                    y += height;
                }

                // set the font
                b.Font = fontLarge;
            }
        }

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

        internal void StyleListView(ListView listView)
        {
            listView.Location = new System.Drawing.Point(0, NavigationPanelHeight);
            listView.Size = form.ClientSize;
            listView.View = View.Details; // displays column headers
            listView.Font = fontDefault;
            listView.FullRowSelect = true;
            listView.HeaderStyle = ColumnHeaderStyle.None;
            for (int i = 0; i < listView.Columns.Count; ++i)
            {
                listView.Columns[i].Width = listView.ClientSize.Width;
            }
        }
    }
}
