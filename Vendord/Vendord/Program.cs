using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Vendord.SmartDeviceApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            DataUtilities.SeedDB();
            Application.Run(new MainForm());
        }
    }
}