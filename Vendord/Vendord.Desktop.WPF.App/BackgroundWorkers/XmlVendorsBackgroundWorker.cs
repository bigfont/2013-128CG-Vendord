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
        public XmlVendorsBackgroundWorker(ProgressChangedEventHandler progressChanged, RunWorkerCompletedEventHandler workerCompleted)
            : base(progressChanged, workerCompleted)
        {
        }

        protected override void DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                XmlSync sync = new XmlSync();
                BackgroundWorker worker = sender as BackgroundWorker;
#if ImportTestData
            string filePath = @"C:\Users\Shaun\SkyDrive\Documents\Work\BigFont\Clients\2013-124CG\DataToImport\my-vendors-small.xml";
#else
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Vendord\DataToImport\vendors.xml");
#endif

                SyncResult syncResult =
                    sync.PullVendorsFromItRetailXmlBackup(
                        worker,
                        filePath);

                e.Result = syncResult.ToString();
            }
            catch (Exception ex)
            {
                e.Result = ex.Message;
            }
        }
    }
}
