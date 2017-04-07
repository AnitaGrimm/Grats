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
        public void Generate(GratsDBContext db)
        {
            var contacts = (from categoryContact in db.CategoryContacts
                            where categoryContact.CategoryID == ID &&
                                  categoryContact.Contact.Birthday != null
                            select categoryContact.Contact).ToList();

            var now = DateTime.Now;

            foreach (var contact in contacts)
            {
                var birthday = contact.Birthday.Value;
                var dispatchDate = new DateTime(
                    birthday.Year, birthday.Month, birthday.Day,
                    Time.Hours, Time.Minutes, Time.Seconds);
                dispatchDate = dispatchDate.AddYears(now.Year - dispatchDate.Year);
                if (dispatchDate.Date < now.Date)
                    dispatchDate = dispatchDate.AddYears(1);

                db.MessageTasks.Add(new MessageTask()
                {
                    CategoryID = ID,
                    Contact = contact,
                    DispatchDate = dispatchDate,
                    Status = MessageTask.TaskStatus.New,
                });
            }

            db.SaveChanges();
        }

        public void Regenerate(GratsDBContext db)
        {
            var pendingTasks = from task in db.MessageTasks
                               where task.CategoryID == ID &&
                                     (task.Status == MessageTask.TaskStatus.New ||
                                      task.Status == MessageTask.TaskStatus.Retry)
                               select task;
            db.MessageTasks.RemoveRange(pendingTasks);

            Generate(db);
        }

        public BirthdayCategory() { }

        public BirthdayCategory(Category category)
        {
            this.Name = category.Name;
            this.Color = category.Color;
            //this.Contacts = (from contact in category.Contacts
            //                 where contact.Birthday.HasValue
            //                 select new Contact(contact)).ToList();
            this.CategoryContacts =
                (from categoryContact in category.CategoryContacts
                 where categoryContact.Contact.Birthday.HasValue
                 select new CategoryContact(this, categoryContact.Contact)).ToList();
            this.OwnersVKID = category.OwnersVKID;
            this.Template = category.Template;
            this.Time = category.Time;
        }
    }
}
