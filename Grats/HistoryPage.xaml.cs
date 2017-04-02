using Grats.Model;
using Grats.ViewModels;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Grats
{
    public class InvertObservableCollection<T>: ObservableCollection<T>
    {
        public new void Add(T item)
        {
            this.Insert(0, item);
        }
    }
    public class HumanizeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTime sourceTime = DateTime.Parse((String)value);
            var d =  sourceTime.Humanize();
            return d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (String)value;
        }
    }
    public sealed partial class HistoryPage : Page
    {
        public InvertObservableCollection<MessageTaskViewModel> Messages = new InvertObservableCollection<MessageTaskViewModel>();
        public HistoryPage()
        {
            this.InitializeComponent();
            UpdateMessageTasks();
        }

        public void UpdateMessageTasks()
        {
            var db = (App.Current as App).dbContext;
            var categories = Enumerable.Union<Category>(
                db.BirthdayCategories.Include(c => c.Tasks),
                db.GeneralCategories.Include(c => c.Tasks));
            var messageTaskViewModels = categories?.SelectMany(category => category.Tasks)?.Where(task=>task.Status== MessageTask.TaskStatus.Done || task.Status== MessageTask.TaskStatus.Pending)?.Select(task=>new MessageTaskViewModel(task));
            foreach (var viewModel in messageTaskViewModels)
                Messages.Add(viewModel);
        }
    }
}
