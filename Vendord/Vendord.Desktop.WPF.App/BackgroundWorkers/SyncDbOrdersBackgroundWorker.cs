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

    public class SyncDbOrdersBackgroundWorker : BackgroundWorkerWrapper
    {
        public SyncDbOrdersBackgroundWorker(ProgressChangedEventHandler ProgressChanged, RunWorkerCompletedEventHandler WorkerCompleted)
            : base(ProgressChanged, WorkerCompleted)
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
            DbSync sync = new DbSync();
            try
            {
                SyncResult syncResult = sync.SyncDesktopAndDeviceDatabases(
                    ScopeName.SyncOrders, 
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
