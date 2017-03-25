using Grats.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.ViewModels
{
    public class MessageTaskViewModel
    {
        public string IDidentificator = "#";
        public string DispatchDateDefinition = "Дата отправки: ";
        public string StatusDefinition = "Статус: ";
        public string StatusMessageDefinition;
        public string LastTryDateDefinition = "Последняя попытка: ";
        public MessageTaskViewModel() { }
        public MessageTaskViewModel( MessageTask CurrentTask)
        {
            ID = IDidentificator + CurrentTask.ID;
            DispatchDate = DispatchDateDefinition + CurrentTask.DispatchDate;
            Status = StatusDefinition + CurrentTask.Status;
            StatusMessage = StatusMessageDefinition + CurrentTask.StatusMessage;
            LastTryDate = LastTryDateDefinition + CurrentTask.LastTryDate;
            Category = CurrentTask.Category;
            CategoryID = CurrentTask.CategoryID;
        }
        public string ID { get; set; }
        public string DispatchDate { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public string LastTryDate { get; set; }
        public Category Category { get; set; }
        public long CategoryID { get; set; }
    }
}
