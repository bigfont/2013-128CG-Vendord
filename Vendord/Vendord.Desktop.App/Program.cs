[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.Desktop.App
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    using Vendord.SmartDevice.Shared;

    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            IOHelpers.LogSubroutine("Main");
            
            SetupGlobalErrorHandling();
            CreateApplicationDatabase();
            
            Application.Run(new MainForm());
        }

        public static void CreateApplicationDatabase()
        {            
            VendordDatabase db = new VendordDatabase();
        }

        public static void SetupGlobalErrorHandling()
        {
            ErrorHandler.Setup();
        }
    }
}
