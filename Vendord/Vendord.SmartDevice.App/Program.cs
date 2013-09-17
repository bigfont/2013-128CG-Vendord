using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Vendord.SmartDevice.DAL;

namespace Vendord.SmartDevice.App
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {            
            Database db = new Database(Constants.DATABASE_NAME);
            Application.Run(new MainForm());
        }
    }
}