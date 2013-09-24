namespace Vendord.SmartDevice.Shared
{
    using System.IO;
    using System;
    public static class IOHelpers
    {
        public static void CreateItemIfNotExists(string fullPath)
        {
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);                
            }
        }        

        public static void LogException(Exception e)
        {           
            CreateItemIfNotExists(Constants.ApplicationDataStoreFullPath);

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
            CreateItemIfNotExists(Constants.ApplicationDataStoreFullPath);

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
