using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Vendord.SmartDevice.Shared
{
    internal static class ErrorHandler
    {
        internal static void Setup()
        {
            IOHelpers.LogSubroutine("SetupGlobalErrorHandling");

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(currentDomain_UnhandledException);
        }

        internal static void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e;
            e = (Exception)args.ExceptionObject;
            IOHelpers.LogException(e);
        }
    }
}
