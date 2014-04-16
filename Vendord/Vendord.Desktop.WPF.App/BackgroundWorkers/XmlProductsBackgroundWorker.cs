namespace Vendord.Desktop.WPF.App.BackgroundWorkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using Vendord.Sync;
    using System.IO;

    public class XmlProductsBackgroundWorker : BackgroundWorkerWrapper
    {
        public XmlProductsBackgroundWorker(ProgressChangedEventHandler ProgressChanged)
            : base(ProgressChanged)
        {
        }

        protected override void DoWork(object sender, DoWorkEventArgs e)
        {
            XmlSync sync = new XmlSync();
            BackgroundWorker worker = sender as BackgroundWorker;
#if ImportTestData
            string filePath = @"C:\Users\Shaun\SkyDrive\Documents\Work\BigFont\Clients\2013-124CG\DataToImport\my-products-small.xml";
#else
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Vendord\DataToImport\products.xml");
#endif
            SyncResult result = sync.PullProductsFromItRetailXmlBackup(
                worker,
                filePath);
        }
    }
}
