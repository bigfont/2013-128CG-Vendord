using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Vendord.SmartDevice.App
{
    internal static class FormHelper
    {
        internal static Button CreateButtonWithEventHandler(string action, int tabIndex, EventHandler eventHandler)
        {
            Button b = new Button();
            b.Name = FormNavigation.BUTTON_PREFIX + action;
            b.Text = action;
            b.Tag = action;
            b.Click += eventHandler;
            return b;
        }
    }
}
