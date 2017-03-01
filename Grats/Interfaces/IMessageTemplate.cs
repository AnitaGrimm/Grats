using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace Grats.Interfaces
{
    /// <summary>
    /// Шаблонизированное сообщение
    /// </summary>
    interface IMessageTemplate
    {
        /// <summary>
        /// Поля, используемые в шаблоне
        /// </summary>
        ProfileFields Fields { get; }
        /// <summary>
        /// Метод, возвращающий вычисленный текст на основе пользователя
        /// </summary>
        /// <param name="user">Пользователь VK</param>
        /// <returns></returns>
        string Substitute(User user);
    }
}
