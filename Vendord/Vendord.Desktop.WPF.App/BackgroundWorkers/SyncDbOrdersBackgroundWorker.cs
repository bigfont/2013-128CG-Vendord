namespace Vendord.Desktop.WPF.App.BackgroundWorkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using Vendord.Sync;
    using System.IO;

    public class SyncDbOrdersBackgroundWorker : BackgroundWorkerWrapper
    {
        public SyncDbOrdersBackgroundWorker(ProgressChangedEventHandler ProgressChanged)
            : base(ProgressChanged)
        {
        }

        protected override void DoWork(object sender, DoWorkEventArgs e)
        {
            DbSync sync = new DbSync();
            try
            {
                SyncResult result = sync.SyncDesktopAndDeviceDatabases(ScopeName.SyncOrders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}
