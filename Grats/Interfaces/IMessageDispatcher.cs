using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grats.Model;

namespace Grats.Interfaces
{
    /// <summary>
    /// Исключение, сигнализирующее об ошибке в IMessageDispatcher
    /// </summary>
    public class MessageDispatcherException : Exception
    {
        public MessageDispatcherException() { }
        public MessageDispatcherException(string message) : base(message) { }
        public MessageDispatcherException(string message, Exception inner) : base(message, inner) { }
    }
    /// <summary>
    /// Аргументы события, генерируемого IMessageDispatcher
    /// </summary>
    public class MessageDispatcherEventArgs: EventArgs
    {
        public MessageTask Task { get; set; }
        public MessageDispatcherException Exception { get; set; }
  
        public MessageDispatcherEventArgs(MessageTask task, MessageDispatcherException exception = null)
        {
            this.Task = task;
            Exception = exception;
        }
    }
    /// <summary>
    /// Класс, совершающий отправку сообщений пользователям.
    /// Вызывается в бэкграунде
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>
        /// Метод, запускающий отправку
        /// Здесь происходит выборка из бд текущих заданий и их обработка
        /// </summary>
        void Dispatch();
        /// <summary>
        /// Событие, вызываемое после попытки обработать задачу
        /// </summary>
        event EventHandler<MessageDispatcherEventArgs> OnTaskHandled;
    }
}
