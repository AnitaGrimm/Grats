using Grats.Model;
using Grats.ViewModels;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using static Grats.EditorPage;
using static Grats.Model.MessageTask;

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
            var d =  sourceTime.Humanize(false);
            return d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (String)value;
        }
    }
    public sealed partial class HistoryPage : Page, INotifyPropertyChanged
    {
        public static DependencyProperty MainFrameProp = DependencyProperty.Register("MainFrame", typeof(Frame), typeof(HistoryPage), new PropertyMetadata(null));
        public Frame MainFrame
        {
            get { return (Frame)GetValue(MainFrameProp); }
            set { SetValue(MainFrameProp, value); }
        }

        public InvertObservableCollection<MessageTaskViewModel> Messages = new InvertObservableCollection<MessageTaskViewModel>();
        public HistoryPage()
        {
            this.InitializeComponent();
            UpdateMessageTasks();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Filter

        bool doneOnly;
        public bool DoneOnly
        {
            get { return doneOnly; }
            set
            {
                doneOnly = value;
                OnPropertyChanged("DoneOnlyButtonBackground");
            }
        }

        bool pendingOnly;
        public bool PendingOnly
        {
            get { return pendingOnly; }
            set
            {
                pendingOnly = value;
                OnPropertyChanged("PendingOnlyButtonBackground");
            }
        }

        bool errorOnly;
        public bool ErrorOnly
        {
            get { return errorOnly; }
            set
            {
                errorOnly = value;
                OnPropertyChanged("ErrorOnlyButtonBackground");
            }
        }

        public Color DoneOnlyButtonBackground => DoneOnly ? Colors.Red : Colors.Transparent;
        public Color PendingOnlyButtonBackground => PendingOnly ? Colors.Red : Colors.Transparent;
        public Color ErrorOnlyButtonBackground => ErrorOnly ? Colors.Red : Colors.Transparent;

        private void DoneOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            DoneOnly = !DoneOnly;
            UpdateMessageTasks();
        }

        private void PendingOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            PendingOnly = !PendingOnly;
            UpdateMessageTasks();
        }

        private void ErrorOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorOnly = !ErrorOnly;
            UpdateMessageTasks();
        }

        public HashSet<TaskStatus> WhiteList
        {
            get
            {
                var result = new HashSet<TaskStatus>();
                if (DoneOnly)
                    result.Add(TaskStatus.Done);
                if (PendingOnly)
                    result.Add(TaskStatus.Pending);
                return result;
            }
        }
        
        public HashSet<TaskStatus> BlackList
        {
            get
            {
                return new HashSet<TaskStatus>
                {
                    TaskStatus.New,
                    TaskStatus.Retry
                };
            }
        }

        #endregion

        public void UpdateMessageTasks()
        {
            Messages.Clear();
            var db = (App.Current as App).dbContext;
            var categories = Enumerable.Union<Category>(
                db.BirthdayCategories.Include(c => c.Tasks),
                db.GeneralCategories.Include(c => c.Tasks));
            var whiteList = WhiteList;
            var blackList = BlackList;
            var messageTaskViewModels = categories?.SelectMany(category => category.Tasks)?
                .Where(task=> whiteList.Count == 0 ? !blackList.Contains(task.Status) : whiteList.Contains(task.Status) )?
                .Select(task=>new MessageTaskViewModel(task));
            foreach (var viewModel in messageTaskViewModels)
            {
                viewModel.RetryTask += ViewModel_RetryTask;
                Messages.Add(viewModel);
            }
        }

        private void ViewModel_RetryTask(object sender, EventArgs e)
        {
            UpdateMessageTasks();
            TriggerBackgroundTask();
        }

        private async void TriggerBackgroundTask()
        {
            try
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == "SendGrats")
                    {
                        BackgroundTaskRegistration bt = task.Value as BackgroundTaskRegistration;
                        ApplicationTrigger appTr = bt.Trigger as ApplicationTrigger;
                        await appTr.RequestAsync();
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainFrame == null)
                return;
            var Item = e.AddedItems?.FirstOrDefault() as MessageTaskViewModel;
            MainFrame.Navigate(
                typeof(EditorPage),
                    new EditCategoryParameter()
                    {
                        ID = Item.Task.CategoryID,
                        CategoryType = Item.Task.Category.GetType()
                    },
                    new DrillInNavigationTransitionInfo());
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
