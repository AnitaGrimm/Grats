using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.Model
{
    /// <summary>
    /// Задание на отправку сообщения
    /// </summary>
    public class MessageTask
    {
        /// <summary>
        /// Статус задания
        /// </summary>
        public enum TaskStatus
        {
            New,        // Новое
            Retry,      // Требует повторной отправки
            Pending,    // Требует подверждения
            Done        // Отправлено
        }

        public long ID { get; set; }
        public DateTime DispatchDate { get; set; }
        public TaskStatus Status { get; set; }
        public string StatusMessage { get; set; }
        public DateTime LastTryDate { get; set; }

        public Category Category { get; set; }
        public long CategoryID { get; set; }

        public Contact Contact { get; set; }
        public long? ContactID { get; set; }
    }
}
