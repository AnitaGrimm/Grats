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
            var today = DateTime.Now;
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
                Grats.Notification.Toaster.PopToast("Отправлено", "Поздравление \"" + task.Category.Name + "\" успешно отправлено", null, null, contact.PhotoUri);
            }
            catch (MessageTemplateSyntaxException ex)
            {
                UpdateTask(task, false, "Синтаксическая ошибка в шаблоне", ex);
                Grats.Notification.Toaster.PopToast("Ошибка", "Синтаксическая ошибка в шаблоне ("+task.Category.Name+")");
            }
            catch (VkApiException ex)
            {
                UpdateTask(task, false, "Ошибка соединения с ВКонтакте", ex);
                Grats.Notification.Toaster.PopToast("Ошибка", "Ошибка соединения с ВКонтакте");
            }
            catch (Exception ex)
            {
                UpdateTask(task, false, "Неизвестная ошибка", ex);
                Grats.Notification.Toaster.PopToast("Ошибка", "Неизвестная ошибка(" + task.Category.Name + ")");
            }
        }

        private void UpdateTask(MessageTask task, bool messageSent, string statusString, Exception exception = null)
        {
            task.Status = messageSent ? MessageTask.TaskStatus.Done : MessageTask.TaskStatus.Pending;
            task.StatusMessage = statusString;
            task.LastTryDate = DateTime.Now;

            if (messageSent)
            {
                var newTask = new MessageTask
                {
                    Category = task.Category,
                    Contact = task.Contact,
                    DispatchDate = task.DispatchDate.AddYears(1),
                    Status = MessageTask.TaskStatus.New,
                };
                DB.MessageTasks.Add(newTask);
            }

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
