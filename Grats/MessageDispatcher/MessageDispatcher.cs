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
using Microsoft.EntityFrameworkCore;
using VkNet;

namespace Grats.MessageDispatcher
{
    public class MessageDispatcher : IMessageDispatcher
    {
        /// <summary>
        /// Создает объект MessageDispatcher
        /// </summary>
        /// <param name="db">Контекст, используемый для работы с БД</param>
        /// <param name="vk">Объект, используемый для связи с ВК</param>
        public MessageDispatcher(GratsDBContext db, MessageDispatcherVkConnector vk = null)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }
            if (vk == null)
            {
                vk = new MessageDispatcherVkConnector();
            }
            DB = db;
            VK = vk;
        }

        public void Dispatch()
        {
            var tasks = FindWaitingTasks();

            foreach (var task in tasks)
            {
                HandleTask(task);
            }
        }

        public event EventHandler<MessageDispatcherEventArgs> OnTaskHandled;
        
        private GratsDBContext DB;
        private MessageDispatcherVkConnector VK;

        private List<MessageTask> FindWaitingTasks()
        {
            var today = DateTime.Now.Date;
            var tasksToDo =
                from t in DB.MessageTasks
                where t.Status == MessageTask.TaskStatus.New || t.Status == MessageTask.TaskStatus.Retry
                where t.DispatchDate <= today
                orderby t.DispatchDate
                select t;
            return tasksToDo
                .Include(t => t.Category)
                .Include(t => t.Contact)
                .ToList();
        }


        private void HandleTask(MessageTask task)
        {
            var category = task.Category;
            var contact = task.Contact;

            try
            {
                IMessageTemplate template = new MessageTemplate(category.Template);

                var requiredFields = template.Fields;
                var user = VK.GetUser(contact.VKID, requiredFields);

                var messageText = template.Substitute(user);
                VK.SendMessage(contact.VKID, messageText);

                UpdateTask(task, true, "Сообщение успешно отправлено");
            }
            catch (MessageTemplateSyntaxException ex)
            {
                UpdateTask(task, false, "Синтаксическая ошибка в шаблоне", ex);
            }
            catch (VkApiException ex)
            {
                UpdateTask(task, false, "Ошибка соединения с ВКонтакте", ex);
            }
            catch (Exception ex)
            {
                UpdateTask(task, false, "Неизвестная ошибка", ex);
            }
        }

        private void UpdateTask(MessageTask task, bool messageSent, string statusString, Exception exception = null)
        {
            task.Status = messageSent ? MessageTask.TaskStatus.Done : MessageTask.TaskStatus.Pending;
            task.StatusMessage = statusString;
            task.LastTryDate = DateTime.Now;

            DB.SaveChanges();

            MessageDispatcherException mde = null;
            if (exception != null)
            {
                mde = new MessageDispatcherException(statusString, exception);
            }

            OnTaskHandled?.Invoke(this, new MessageDispatcherEventArgs(task, mde));
        }
    }
}
