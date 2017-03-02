using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grats.Interfaces;
using Grats.Model;
using System.Diagnostics;
using Grats.MessageTemplates;
using VkNet.Model.RequestParams;
using VkNet.Exception;

namespace Grats.MessageDispatcher
{
    public class MessageDispatcher : Interfaces.IMessageDispatcher
    {
        /// <summary>
        /// Создает объект MessageDispatcher
        /// </summary>
        /// <param name="db">Контекст, используемый для работы с БД</param>
        public MessageDispatcher(GratsDBContext db)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }
            DB = db;
        }

        public void Dispatch()
        {
            var task = FindWaitingTask();
            if (task == null)
            {
                // делать пока нечего
                return;
            }

            string statusString;
            var messageSent = HandleTask(task, out statusString);

            UpdateTask(task, messageSent, statusString);

            OnTaskHandled?.Invoke(this, new MessageDispatcherEventArgs(task));
        }

        public event EventHandler<MessageDispatcherEventArgs> OnTaskHandled;
        
        private GratsDBContext DB;

        private MessageTask FindWaitingTask()
        {
            var now = DateTime.Now;
            var tasksToDo =
                from t in DB.MessageTasks
                where t.Status == MessageTask.TaskStatus.New || t.Status == MessageTask.TaskStatus.Retry
                where t.DispatchDate < now
                orderby t.DispatchDate // ???
                select t;
            return tasksToDo.FirstOrDefault();
        }


        private bool HandleTask(MessageTask task, out string statusString)
        {
            var app = App.Current as App;

            var category = task.Category;
            var contact = new Contact(); // TODO: fix

            try
            {
                var template = new MessageTemplate(category.Template);
                var requiredFields = template.Fields;
                var user = app.VKAPI.Users.Get(contact.VKID, requiredFields);
                var messageText = template.Substitute(user);
                app.VKAPI.Messages.Send(new MessagesSendParams
                {
                    UserId = user.Id,
                    Message = messageText,
                });
                statusString = string.Format(
                    "Сообщение успешно отправлено пользователю {0}",
                    contact.ScreenName);
                return true;
            }
            catch (MessageTemplateSyntaxException ex)
            {
                statusString = string.Format(
                    "Синтаксическая ошибка в шаблоне: {0}",
                    ex.Message);
                return false;
            }
            catch (VkApiException ex)
            {
                statusString = string.Format(
                    "Ошибка связи с Вконтакте: {0}",
                    ex.Message);
                return false;
            }
        }

        private void UpdateTask(MessageTask task, bool messageSent, string statusString)
        {
            throw new NotImplementedException();
        }
    }
}
