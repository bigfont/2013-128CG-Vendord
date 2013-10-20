[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.SmartDevice.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;    
    using System.Windows.Forms;
    using Vendord.SmartDevice.Shared;

    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        public static void Main()
        {
            IOHelpers.LogSubroutine("Main");

            SetupGlobalErrorHandling();
            CreateApplicationDatabase();

            Application.Run(new MainForm());
        }

        private static void CreateApplicationDatabase()
        {
            Database db = new Database();
        }

        private static void SetupGlobalErrorHandling()
        {
            ErrorHandler.Setup();
        }
    }
}