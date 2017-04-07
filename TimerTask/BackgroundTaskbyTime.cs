using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace TimerTask
{
    public sealed class BackgroundTaskbyTime:IBackgroundTask
    {
        BackgroundTaskDeferral defferral=null;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            defferral = taskInstance.GetDeferral();
            defferral.Complete();
        }
    }
}
