using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.MessageTemplates
{
    /// <summary>
    /// Сообщает о неверном синтаксисе шаблона
    /// </summary>
    public class MessageTemplateSyntaxException : Exception
    {
        public MessageTemplateSyntaxException() { }
        public MessageTemplateSyntaxException(string message) : base(message) { }
        public MessageTemplateSyntaxException(string message, Exception inner) : base(message, inner) { }
    }
}
