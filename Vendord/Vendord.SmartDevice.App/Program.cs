namespace Vendord.SmartDevice.App
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using Vendord.SmartDevice.Shared;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
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