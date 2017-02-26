using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grats.Model;

namespace Grats.Interfaces
{
    /// <summary>
    /// Аргументы события, генерируемого IMessageDispatcher
    /// </summary>
    class MessageDispatcherEventArgs: EventArgs
    {
        public MessageTask Task { get; set; }
  
        public MessageDispatcherEventArgs(MessageTask task)
        {
            this.Task = task;
        }
    }
    /// <summary>
    /// Класс, совершающий отправку сообщений пользователям.
    /// Вызывается в бэкграунде
    /// </summary>
    interface IMessageDispatcher
    {
        /// <summary>
        /// Метод, запускающий отправку
        /// Здесь происходит выборка из бд текущих заданий и их обработка
        /// </summary>
        void dispatch();
        /// <summary>
        /// Событие, вызываемое после попытки обработать задачу
        /// </summary>
        event EventHandler<MessageDispatcherEventArgs> OnTaskHandled;
    }
}
