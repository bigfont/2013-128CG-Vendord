[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;    

    public class IOHelpers
    {
        public static string AddSuffixToFilePath(string filePath, string suffix)
        {
            string result;
            result = filePath.Insert(filePath.LastIndexOf('.'), suffix);
            return result;            
        }

        public static void CreateDirectoryIfNotExists(string fullPath)
        {
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public static void LogException(Exception e)
        {
            CreateDirectoryIfNotExists(Constants.ApplicationDataStoreFullPath);

            // write to the error log
            using (StreamWriter w = File.AppendText(Constants.ErrorLogFullPath))
            {
                w.Write("\r\nLogEntry: ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                w.WriteLine("Message : {0}", e.Message);
                w.WriteLine("StackTrace : {0}", e.StackTrace);
                w.WriteLine("--------------------------------------");

                // update the underlying file
                w.Flush();

                // close the writer and underlying file
                w.Close();
            }
        }

        public static void LogSubroutine(string message)
        {
            CreateDirectoryIfNotExists(Constants.ApplicationDataStoreFullPath);

            using (StreamWriter w = File.AppendText(Constants.ApplicationLogFullPath))
            {
                w.Write("\r\nLogEntry: ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                w.WriteLine("Message : {0}", message);
                w.WriteLine("--------------------------------------");

                // update the underlying file
                w.Flush();

                // close the writer and underlying file
                w.Close();
            }
        }
    }
}
