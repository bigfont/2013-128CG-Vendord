using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Vendord
{
    public class ResponsiveForm : Form
    {
        private int BUTTONS_PER_ROW = 2;
        private int BUTTONS_PER_COLUMN = 2;

        internal void SetFormSizeAndLocation()
        {
            this.Location = new Point(0, 0);
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
        }

        internal void SetButtonSizesAndLocations(Button[] buttons)
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
            }
        }
    }
}
