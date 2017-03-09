using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.Model
{
    /// <summary>
    /// Вспомогательная сущность для реализации связи
    /// многие-ко-многим между Category и Contact
    /// </summary>
    public class CategoryContact
    {
        public long CategoryID { get; set; }
        public Category Category { get; set; }

        public long ContactID { get; set; }
        public Contact Contact { get; set; }

        public CategoryContact () { }

        public CategoryContact(Category category, Contact contact)
        {
            Category = category;
            Contact = contact;
        }
    }
}
