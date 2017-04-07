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

        public void Generate(GratsDBContext db)
        {
            var contacts = (from categoryContact in db.CategoryContacts
                            where categoryContact.CategoryID == ID
                            select categoryContact.Contact).ToList();

            var now = DateTime.Now;

            var dispatchDate = new DateTime(
                Date.Year, Date.Month, Date.Day,
                Time.Hours, Time.Minutes, Time.Seconds);
            dispatchDate = dispatchDate.AddYears(now.Year - dispatchDate.Year);
            if (dispatchDate < now)
                dispatchDate = dispatchDate.AddYears(1);

            foreach (var contact in contacts)
            {
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
            this.Time = category.Time;
        }
    }
}
