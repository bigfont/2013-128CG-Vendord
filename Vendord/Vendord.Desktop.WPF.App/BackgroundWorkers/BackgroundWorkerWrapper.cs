namespace Vendord.Desktop.WPF.App.BackgroundWorkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    public abstract class BackgroundWorkerWrapper
    {
        public BackgroundWorker bWorker;

        public BackgroundWorkerWrapper(
            ProgressChangedEventHandler ProgressChanged,
            RunWorkerCompletedEventHandler RunWorkerCompleted)
        {
            bWorker = new BackgroundWorker();
            bWorker.WorkerReportsProgress = true;
            bWorker.WorkerSupportsCancellation = true;
            bWorker.ProgressChanged += ProgressChanged;
            bWorker.RunWorkerCompleted += RunWorkerCompleted;
            bWorker.DoWork += new DoWorkEventHandler(DoWork);
        }

        public void Run()
        {
            bWorker.RunWorkerAsync();
        }

        protected abstract void DoWork(object sender, DoWorkEventArgs e);
    }
}
