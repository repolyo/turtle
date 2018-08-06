using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace TLib
{
    public enum ResultType
    {
        INITIALIZE,
        OPEN,
        SAVE,
        PROGRESS,
        DELETE,
        DELETEALL,
        PROGRESS_BAR,
        PROGRESS2,
        NEW_TREE_NODE,
        NEW_CLASS_NODE,
        REFRESH_TREE_NODE,
        FOUND_FACTORY,
        NEW_DOCUMENT,
        NEW_RECORD,
    };

    public class ResultObject
    {
        private ResultType state;
        public ResultType State
        {
            get { return state; }
            set { state = value; }
        }

        private object param;
        public object Param
        {
            get { return param; }
            set { param = value; }
        }

    }

    public class BackgroundWorkerEx : System.ComponentModel.BackgroundWorker
    {
        private int progress = 0;
        //
        // Summary:
        //     Raises the System.ComponentModel.BackgroundWorker.ProgressChanged event.
        //
        // Parameters:
        //   percentProgress:
        //     The percentage, from 0 to 100, of the background operation that is complete.
        //
        //   userState:
        //     The state object passed to System.ComponentModel.BackgroundWorker.RunWorkerAsync(System.Object).
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The System.ComponentModel.BackgroundWorker.WorkerReportsProgress property
        //     is set to false.
        public void ReportResult(int percentProgress, ResultType type, object userState)
        {
            if (this.WorkerReportsProgress && this.IsBusy)
            {
                ResultObject result = new ResultObject();
                result.State = type;
                result.Param = userState;

                this.progress = percentProgress;
                base.ReportProgress(percentProgress, result);
            }
        }

        public void ReportResult(int percentProgress, ResultType type)
        {
            this.ReportResult(percentProgress, type, null);
        }

        public void ReportResult(object source, ResultType type)
        {
            this.ReportResult(1, type, null);
        }

        public void ReportResult(DataRow dr, ResultType type)
        {
            int percentProgress = 0;
            foreach (object o in dr.ItemArray)
            {
                percentProgress += (null != o) ? o.ToString().Length : 0;
            }

            this.ReportResult(percentProgress, type);
        }

        public void ReportResult(ResultType type, object userState = null)
        {
            this.ReportResult(this.progress + 1, type, userState);
        }
    }
}
