using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Vendord.SmartDevice.DAL;

namespace Vendord.Desktop.App
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Database db = new Database(Constants.DATABASE_NAME);
            Application.Run(new MainForm());
        }
    }
}
