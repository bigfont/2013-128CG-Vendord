namespace Vendord.Desktop.WPF.App.BackgroundWorkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using Vendord.Sync;
    using System.IO;

    public class XmlVendorsBackgroundWorker : BackgroundWorkerWrapper
    {
        public XmlVendorsBackgroundWorker(ProgressChangedEventHandler ProgressChanged)
            : base(ProgressChanged)
        {
        }

        protected override void DoWork(object sender, DoWorkEventArgs e)
        {
            XmlSync sync = new XmlSync();
            BackgroundWorker worker = sender as BackgroundWorker;
#if ImportTestData
            string filePath = @"C:\Users\Shaun\SkyDrive\Documents\Work\BigFont\Clients\2013-124CG\DataToImport\my-vendors-small.xml";
#else
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Vendord\DataToImport\vendors.xml");
#endif
            try
            {
                SyncResult result =
                    sync.PullVendorsFromItRetailXmlBackup(
                        worker,
                        filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}
