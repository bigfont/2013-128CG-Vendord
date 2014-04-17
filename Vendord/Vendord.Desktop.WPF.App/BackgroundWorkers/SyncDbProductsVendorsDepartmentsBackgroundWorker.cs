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
            this.bWorker.ReportProgress(0, "Hello World");
        }

        protected override void DoWork(object sender, DoWorkEventArgs e)
        {
            DbSync sync = new DbSync();
            try
            {
                SyncResult result = sync.SyncDesktopAndDeviceDatabases(
                    ScopeName.SyncProductsVendorsAndDepts,
                    new EventHandler<SyncStagedProgressEventArgs>(SyncStagedProgress));

                this.bWorker.ReportProgress(0, result.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}
