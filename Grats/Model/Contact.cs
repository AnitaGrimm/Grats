using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using VkNet.Model;
using VkNet.Enums;

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
        public DateTime? Birthday { get; set; }
        public string PhotoUri { get; set; }

        public Category Category { get; set; }
        public long CategoryID { get; set; }

        public List<MessageTask> Tasks { get; set; }

        public Contact() { }
        public Contact(User user)
        {
            this.VKID = user.Id;
            this.ScreenName = user.LastName + " " + user.FirstName;
            if (user.BirthdayVisibility.HasValue)
            {
                switch (user.BirthdayVisibility.Value)
                {
                    case BirthdayVisibility.Full:
                        this.Birthday = DateTime.ParseExact(user.BirthDate, "d.M.yyyy", CultureInfo.InvariantCulture);
                        break;
                    case BirthdayVisibility.OnlyDayAndMonth:
                        this.Birthday = DateTime.ParseExact(user.BirthDate, "d.M", CultureInfo.InvariantCulture);
                        break;
                    default:
                        this.Birthday = null;
                        break;
                } 
            }
            this.PhotoUri = user.Photo100.AbsoluteUri;
        }
    }
}
