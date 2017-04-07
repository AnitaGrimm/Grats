using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.Model
{
    /// <summary>
    /// Интерфейс создания задач. Используется в потомках класса Category
    /// </summary>
    public interface ITaskGenerator
    {
        void Generate(GratsDBContext db);
        void Regenerate(GratsDBContext db);
    }
    /// <summary>
    /// Класс поздравления, содержащий контактов, которые участвуют в рассылке,
    /// Шаблон и прочие настройки
    /// </summary>
    public class Category
    {
        // TODO: Выбрать дефолтный цвет
        public static string DefaultColor = "#00000000";
        public static TimeSpan DefaultTime = new TimeSpan(12, 0, 0);

        public Category()
        {
            Color = DefaultColor;
            Time = DefaultTime;
        }

        public long ID { get; set; }
        public long OwnersVKID { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Template { get; set; }
        public TimeSpan Time { get; set; }
        //public List<Contact> Contacts { get; set; }
        public List<CategoryContact> CategoryContacts { get; set; }
        public List<MessageTask> Tasks { get; set; }
    }
}
