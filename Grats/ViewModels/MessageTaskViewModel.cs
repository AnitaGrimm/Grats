using Grats.Extensions;
using Grats.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Grats.ViewModels
{
    public class MessageTaskViewModel
    {
        public MessageTaskViewModel() { }

        public MessageTask Task;

        public string Name => Task.Category.Name;

        public Color Color => ColorExtensions.FromHex(Task.Category.Color);

        public string StatusMessage => Task.StatusMessage;

        public bool CanBeResended => Task.Status == MessageTask.TaskStatus.Pending;

        public string LastTryDate => Task.LastTryDate.ToString();

        public Symbol StatusSymbol { get; private set; }

        public event EventHandler RetryTask;
        
        public void RetryClick(object sender, RoutedEventArgs e)
        {
            var db = (App.Current as App).dbContext;
            Task.Status = MessageTask.TaskStatus.Retry;
            db.MessageTasks.Update(Task);
            db.SaveChanges();
            RetryTask(this, new EventArgs());
        }

        public MessageTaskViewModel( MessageTask CurrentTask)
        {
            this.Task = CurrentTask;
            switch (CurrentTask.Status)
            {
                case MessageTask.TaskStatus.Done:
                    StatusSymbol = Symbol.Accept;
                    break;
                case MessageTask.TaskStatus.Pending:
                    StatusSymbol = Symbol.Remove;
                    break;
                default:
                    break;
            }
        }
    }
}
