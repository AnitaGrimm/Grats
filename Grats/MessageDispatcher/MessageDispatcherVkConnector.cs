using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace Grats.MessageDispatcher
{
    /// <summary>
    /// Объединяет методы, используемые MessageDispatcher
    /// для работы с API ВКонтакте
    /// </summary>
    public class MessageDispatcherVkConnector
    {
        /// <summary>
        /// См. VkApi.Users.Get(...)
        /// </summary>
        public virtual User GetUser(long userId, ProfileFields fields)
        {
            return (App.Current as App).VKAPI.Users.Get(userId, fields);
        }
        /// <summary>
        /// См. VkApi.Messages.Send(...)
        /// </summary>
        public virtual long SendMessage(long userId, string message)
        {
            return (App.Current as App).VKAPI.Messages.Send(new MessagesSendParams
            {
                UserId = userId,
                Message = message,
            });
        }
    }
}
