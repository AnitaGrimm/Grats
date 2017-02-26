using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;

namespace Grats.Model
{
    /// <summary>
    /// Контакт, участвующий в рассылке поздравлений
    /// </summary>
    public class Contact
    {
        public long ID { get; set; }
        public long VKID { get; set; }
        public String ScreenName { get; set; }
        public DateTime Birthday { get; set; }

        public Category Category { get; set; }
        public long CategoryID { get; set; }

        public Contact() { }
        public Contact(User user)
        {
            throw new NotImplementedException();
        }
    }
}
