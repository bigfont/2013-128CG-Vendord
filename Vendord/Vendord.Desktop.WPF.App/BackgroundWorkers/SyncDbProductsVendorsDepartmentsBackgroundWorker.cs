namespace Vendord.Desktop.WPF.App.BackgroundWorkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using Vendord.Sync;
    using System.IO;
    using Microsoft.Synchronization;
    public class SyncDbProductsVendorsDepartmentsBackgroundWorker : BackgroundWorkerWrapper
    {
        public SyncDbProductsVendorsDepartmentsBackgroundWorker(ProgressChangedEventHandler progressChanged, RunWorkerCompletedEventHandler workerCompleted)
            : base(progressChanged, workerCompleted)
        { 
            
        }

        public void SyncStagedProgress(object sender, SyncStagedProgressEventArgs e)
        {
            uint percComplete = e.CompletedWork * 100 / e.TotalWork;
            string message = e.ReportingProvider.ToString() + " " + e.Stage.ToString();
            this.bWorker.ReportProgress(Convert.ToInt16(percComplete), message);
        }

        protected override void DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                DbSync sync = new DbSync();

                SyncResult syncResult = sync.SyncDesktopAndDeviceDatabases(
                    ScopeName.SyncProductsVendorsAndDepts,
                    new EventHandler<SyncStagedProgressEventArgs>(SyncStagedProgress));

                e.Result = syncResult.ToString();
            }
            catch (Exception ex)
            {
                e.Result = ex.Message;
            }
        }
    }
}
