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
            CreateDatabase();
            
            Application.Run(new MainForm());
        }

        static void CreateDatabase()
        {
            Database db = new Database(Constants.DATABASE_NAME);
        }

        static void SetupGlobalErrorHandling()
        {
            IOHelpers.LogSubroutine("SetupGlobalErrorHandling");

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(currentDomain_UnhandledException);
        }

        static void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e;
            e = (Exception)args.ExceptionObject;
            IOHelpers.LogException(e);
        }
    }
}
