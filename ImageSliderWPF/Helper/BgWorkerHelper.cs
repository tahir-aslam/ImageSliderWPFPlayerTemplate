using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSliderWPF.Helper
{
   public class BgWorkerHelper
    {
        private BackgroundWorker bgWorker = new BackgroundWorker();

        public void RunMethodAsync<TResult>(Func<TResult> func, Action<TResult> completedAction)
        {
            RunMethodAsync<TResult>(func, completedAction, null);
        }

        public void RunMethodAsync<TResult>(Func<TResult> func, Action<TResult> completedAction, Action isBusyAction)
        {
            if (!bgWorker.IsBusy)
            {
                bgWorker.WorkerReportsProgress = true;

                ProgressChangedEventHandler progressChangedEventHandler = null;

                progressChangedEventHandler = new ProgressChangedEventHandler((s, e) =>
                {
                    // We are finished, run completed action
                    completedAction((TResult)e.UserState);

                    bgWorker.ProgressChanged -= progressChangedEventHandler;

                    // Finished
                    bgWorker.Dispose();
                });

                DoWorkEventHandler doWorkEventHandler = null;

                doWorkEventHandler = new DoWorkEventHandler((s, e) =>
                {
                    TResult result = func();

                    bgWorker.ReportProgress(100, result);

                    bgWorker.DoWork -= doWorkEventHandler;
                });

                bgWorker.DoWork += doWorkEventHandler;
                bgWorker.ProgressChanged += progressChangedEventHandler;

                bgWorker.RunWorkerAsync();

            }
            else
            {
                if (isBusyAction != null)
                {
                    isBusyAction();
                }
            }
        }
    }
}
