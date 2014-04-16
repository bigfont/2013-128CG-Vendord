using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.SmartDevice.Linked;
using System.IO;

namespace Vendord.Printer
{
    public static class TxtPrinter
    {
        public static void PrintOrderForOneVendor(PrintableOrder pOrder)
        {
            string filePath = CreateTimestampedFilePath("order");
            using (FileStream fs = File.Create(filePath))
            {
                // print header                
                AppendText(fs, PrintableOrder.CreateOrderHeader(pOrder));
                AppendLine(fs);
                AppendLine(fs);

                // print colums headers
                string columnHeaders = PrintableOrderProduct.CreateColumnHeaders();
                AppendText(fs, columnHeaders);
                AppendLine(fs);

                // print rows
                foreach (var pop in pOrder.PrintableOrderProducts)
                {
                    string rowValues = PrintableOrderProduct.CreateRow(pop);
                    AppendText(fs, rowValues);
                    AppendLine(fs);
                }
                
            }
            System.Diagnostics.Process.Start("notepad.exe", filePath);
        }

        #region Generic Helpers

        private static string CreateTimestampedFilePath(string prefix)
        {
            string friendlyDateTime = DateTime.Now.ToString("yyyy-MMMM-dd-HH-mm-ss");
            string fileName = string.Format("{0}-{1}.txt", prefix, friendlyDateTime);

            string filePath =
                Path.Combine(
                    Constants.ApplicationDataStoreFullPath,
                    fileName);

            return filePath;
        }

        private static void AppendText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        private static void AppendLine(FileStream fs)
        {
            AppendText(fs, "\r\n");
        }

        #endregion
    }
}
