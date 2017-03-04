using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.Model
{
    /// <summary>
    /// Поздравление с днем рождения
    /// </summary>
    public class BirthdayCategory : Category, ITaskGenerator
    {
        public void Generate()
        {
            throw new NotImplementedException();
        }

        public void Regenerate()
        {
            throw new NotImplementedException();
        }

        public BirthdayCategory() { }

        public BirthdayCategory(Category category)
        {
            this.Name = category.Name;
            this.Color = category.Color;
            this.Contacts = (from contact in category.Contacts
                             where contact.Birthday.HasValue
                             select new Contact(contact)).ToList();
            this.OwnersVKID = category.OwnersVKID;
            this.Template = category.Template;
        }
    }
}
