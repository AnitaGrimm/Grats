using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Exception;
using Windows.ApplicationModel.Background;

namespace Grats.Background
{
    class BackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            MessageDispatcher.MessageDispatcher messageDispatcher = new
                MessageDispatcher.MessageDispatcher(new Model.GratsDBContext(),null);

            try
            {
                messageDispatcher.Dispatch();
            }
            catch (VkApiException e)
            {
                //Обработка исключений вк.
            }
            finally
            {
                _deferral.Complete();
            }
        }

    }
}
