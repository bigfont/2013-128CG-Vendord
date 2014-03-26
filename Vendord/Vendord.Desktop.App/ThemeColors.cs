using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Vendord.Desktop.App
{
    public static class ThemeColors
    {
        internal static Color AllowDrop
        {
            get
            {
                return Color.Yellow;
            }
        }

        internal static Color DragLeave
        {
            get
            {
                return Color.Yellow;
            }
        }

        internal static Color DragEnter
        {
            get
            {
                return Color.YellowGreen;
            }
        }

        internal static Color Enabled
        {
            get
            {
                return Color.GhostWhite;
            }
        }

        internal static Color Disabled
        {
            get
            {
                return Color.DarkGray;
            }
        }
    }
}
