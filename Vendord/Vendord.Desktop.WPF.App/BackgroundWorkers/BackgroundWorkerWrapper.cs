namespace Vendord.Desktop.WPF.App.BackgroundWorkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    public abstract class BackgroundWorkerWrapper
    {
        public static BackgroundWorker bWorker;

        public BackgroundWorkerWrapper(ProgressChangedEventHandler ProgressChanged)
        {
            bWorker = new BackgroundWorker();
            bWorker.WorkerReportsProgress = true;
            bWorker.ProgressChanged += ProgressChanged;
            bWorker.DoWork += new DoWorkEventHandler(DoWork);
        }

        public void Run()
        {
            bWorker.RunWorkerAsync();
        }

        protected abstract void DoWork(object sender, DoWorkEventArgs e);
    }
}
