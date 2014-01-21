[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.SmartDevice.Linked
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class ErrorHandler
    {
        internal static void Setup()
        {
            IOHelpers.LogSubroutine("SetupGlobalErrorHandling");

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        internal static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e;
            e = (Exception)args.ExceptionObject;
            IOHelpers.LogException(e);
        }
    }
}
