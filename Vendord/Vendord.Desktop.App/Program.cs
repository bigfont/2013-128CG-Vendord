using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Vendord.SmartDevice.Shared;

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
            IOHelpers.LogSubroutine("Main");
            
            SetupGlobalErrorHandling();
            CreateApplicationDatabase();
            
            Application.Run(new MainForm());
        }

        static void CreateApplicationDatabase()
        {            
            VendordDatabase db = new VendordDatabase();
        }

        static void SetupGlobalErrorHandling()
        {
            ErrorHandler.Setup();
        }
    }
}
