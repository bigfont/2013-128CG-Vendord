namespace Vendord.Desktop.App
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    using System.Drawing;

    public class MainFormStyles : Form
    {
        private const int NAVIGATION_PANELS_PER_ROW = 5;
        private const int BUTTONS_PER_ROW = 2;
        private const int BUTTONS_PER_COLUMN = 2;
        private const int emSizeLarge = 20;
        private const int emSizeMedium = 16;
        private const int emSizeSmall = 12;
        private Font fontSmall;
        private Font fontDefault;
        private Font fontLarge;
        public int NavigationPanelHeight
        {
            get
            {
                return this.ClientSize.Height / NAVIGATION_PANELS_PER_ROW;
            }
        }

        private void SetFonts()
        {
            FontFamily family;
            FontStyle style;

            family = FontFamily.GenericSansSerif;
            style = FontStyle.Regular;

            this.fontLarge = new Font(family, emSizeLarge, style);
            this.fontDefault = new Font(family, emSizeMedium, style);
            this.fontSmall = new Font(family, emSizeSmall, style);
        }

        private void StyleForm()
        {
            this.WindowState = FormWindowState.Maximized;
            this.Font = fontDefault;
        }

        public MainFormStyles()
        {
            SetFonts();
            StyleForm();
        }

        protected void StyleNavigationPanel(Panel panel)
        {
            Size panelSize;
            Size buttonSize;

            panelSize = new Size(this.ClientSize.Width, NavigationPanelHeight);
            buttonSize = new Size(this.ClientSize.Width / BUTTONS_PER_ROW, NavigationPanelHeight);

            panel.Size = panelSize;

            foreach (Button b in panel.Controls)
            {
                b.Size = buttonSize;
                b.Font = fontDefault;
            }
        }

        protected void StyleHomeViewButtons(Button[] buttons)
        {
            int width;
            int height;
            int x;
            int y;
            System.Drawing.Size size;
            Button b;

            height = this.ClientSize.Height / BUTTONS_PER_COLUMN;
            width = this.ClientSize.Width / BUTTONS_PER_ROW;
            size = new System.Drawing.Size(width, height);

            x = 0;
            y = 0;

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

        protected void StyleDataGridView(DataGridView view)
        {            
            // Change the Border and Gridline Styles
            view.GridColor = Color.Blue;
            view.BorderStyle = BorderStyle.Fixed3D;
            view.CellBorderStyle = DataGridViewCellBorderStyle.Raised;
            view.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Sunken;
            view.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            // Cell Styles
            view.DefaultCellStyle.BackColor = Color.AliceBlue;
            
            // Heights
            view.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            // Widths
            view.Width = this.ClientSize.Width;
            view.Height = this.ClientSize.Height;
            for (int i = 0; i < view.Columns.Count; ++i)
            {
                view.Columns[i].Width = view.ClientSize.Width / view.Columns.Count;
            }
        }

        protected void StyleListView(ListView listView)
        {
            listView.Location = new System.Drawing.Point(0, NavigationPanelHeight);
            listView.Size = this.ClientSize;
            listView.View = View.Details; // displays column headers
            listView.Font = fontDefault;
            listView.FullRowSelect = true;
            listView.HeaderStyle = ColumnHeaderStyle.None;
            for (int i = 0; i < listView.Columns.Count; ++i)
            {
                listView.Columns[i].Width = listView.ClientSize.Width;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
