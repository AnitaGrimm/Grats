using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.Model
{
    /// <summary>
    /// Поздравление в заданную дату
    /// </summary>
    public class GeneralCategory: Category, ITaskGenerator
    {
        public DateTime Date { get; set; }

        public void Generate()
        {
            throw new NotImplementedException();
        }

        public void Regenerate()
        {
            throw new NotImplementedException();
        }

        public GeneralCategory() { }

        public GeneralCategory(Category category, DateTime date)
        {
            this.Name = category.Name;
            this.Color = category.Color;
            //this.Contacts = (from contact in category.Contacts
            //                 select new Contact(contact)).ToList();
            this.CategoryContacts = 
                (from categoryContact in category.CategoryContacts
                 select new CategoryContact(this, categoryContact.Contact)).ToList();
            this.OwnersVKID = category.OwnersVKID;
            this.Template = category.Template;
            this.Date = date;
        }
    }
}
