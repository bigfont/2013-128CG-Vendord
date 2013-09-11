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
        private const int BUTTONS_PER_ROW = 2;
        private const int BUTTONS_PER_COLUMN = 2;
        private const int emSizeLarge = 20;
        private const int emSizeMedium = 16;
        private const int emSizeSmall = 12;
        protected Font smallFont;
        protected Font mediumFont;
        protected Font largeFont;

        public ResponsiveForm()
        {
            SetFonts();
        }

        private void SetFonts()
        {
            FontFamily family;
            FontStyle style;                        

            family = FontFamily.GenericSansSerif;
            style = FontStyle.Regular;

            this.largeFont = new Font(family, emSizeLarge, style);
            this.mediumFont = new Font(family, emSizeMedium, style);
            this.smallFont = new Font(family, emSizeSmall, style);
        }

        protected Button CreateButtonWithEventHandler(string text, int tabIndex, EventHandler eventHandler)
        {
            Button b = new Button();
            b.Name = "btn" + text;
            b.Text = text;
            b.Click += eventHandler;            
            return b;
        }

        protected void SetFormSizeAndLocation()
        {
            this.Location = new Point(0, 0);
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
        }

        protected void SetButtonSizesAndLocations(Button[] buttons)
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
                b.Font = largeFont;
            }
        }
    }
}
