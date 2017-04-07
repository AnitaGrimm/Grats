using Grats.Extensions;
using Grats.Model;
using Grats.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Grats.MessageTemplates;
using VkNet.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.Devices.Input;
using Microsoft.EntityFrameworkCore;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Background;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace Grats
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EditorPage : Page, INotifyPropertyChanged
    {
        public interface IEditorPageParameter { }

        public class NewCategoryParameter: IEditorPageParameter
        {
            public Category Category { get; set; }
        }

        public class EditCategoryParameter : IEditorPageParameter
        {
            public long ID { get; set; }
            public Type CategoryType { get; set; }
        }

        // TODO: Подобрать цвета
        static List<Color> AvailableColors = new List<Color>
        {

            Color.FromArgb(255,238,64,53),
            Color.FromArgb(255,243,119,54),
            Color.FromArgb(255,253,244,152),
            Color.FromArgb(255,123,192,67),
            Color.FromArgb(255,3,146,207)
        };

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private GratsDBContext DBContext { get; set; }
        public ObservableCollection<string> Colors = new ObservableCollection<string>(
            AvailableColors.Select(color => color.ToString()));

        CategoryDetailViewModel ViewModelField;
        public CategoryDetailViewModel ViewModel
        {
            get { return ViewModelField; }
            set { ViewModelField = value; OnPropertyChanged(); }
        }
       
        public DateTime MaxDate { get; private set; }
        public DateTime MinDate { get; private set; }
        
        public EditorPage()
        {
            MinDate = DateTime.Now;
            MaxDate = DateTime.Now.AddYears(1);
            this.InitializeComponent();
            DBContext = (App.Current as App).dbContext;
            NavigationCacheMode = NavigationCacheMode.Enabled;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
                return;
            if (!(e.Parameter is IEditorPageParameter))
                throw new NotSupportedException("Parameter is not supported");
            if (e.Parameter is NewCategoryParameter)
            {
                var parameter = (e.Parameter as NewCategoryParameter);
                DeleteButton.Visibility = Visibility.Collapsed;
                if(parameter.Category is GeneralCategory)
                    DatePicker.Date = (parameter.Category as GeneralCategory).Date;
                ViewModel = new CategoryDetailViewModel(parameter.Category);
            }
            else
            {
                var parameter = (e.Parameter as EditCategoryParameter);
                DeleteButton.Visibility = Visibility.Visible;
                if (parameter.CategoryType == typeof(GeneralCategory))
                {
                    var category = DBContext.GeneralCategories
                        .Include(c => c.CategoryContacts)
                        .ThenInclude(cc => cc.Contact)
                        .Single(s => s.ID == parameter.ID);
                    DatePicker.Date = category.Date;
                    ViewModel = new CategoryDetailViewModel(category);
                }
                else
                {
                    var category = DBContext.BirthdayCategories
                        .Include(c => c.CategoryContacts)
                        .ThenInclude(cc => cc.Contact)
                        .Single(s => s.ID == parameter.ID);
                    ViewModel = new CategoryDetailViewModel(category);
                }
            }
            MessageFrame.Navigate(typeof(MessageEditorPage), ViewModel);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.Frame.GoBack();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Избавиться от костылей
            // TOOD: Добавить валидацию
            // TODO: Выделить в отдельный класс, покрыть тестами
            ViewModel.Date = DatePicker.Date;
            if (ViewModel.Validate())
            {
                ViewModel.Save(DBContext);
                try
                {
                    foreach(var task in BackgroundTaskRegistration.AllTasks)
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
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
                this.Frame.GoBack();
            }
        }
        
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ContactsListView_DragEnter(object sender, DragEventArgs e)
        {
            object data;
            if (e.DataView.Properties.TryGetValue("friends", out data)
                && data is IEnumerable<User>)
            {
                e.DragUIOverride.IsGlyphVisible = false;
                e.AcceptedOperation = e.AllowedOperations;
            }
            else if (e.DataView.Properties.TryGetValue("friendId", out data)
                && data is long)
            {
                e.DragUIOverride.IsGlyphVisible = false;
                e.AcceptedOperation = e.AllowedOperations;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        private void ContactsListView_Drop(object sender, DragEventArgs e)
        {
            object data;
            if (e.DataView.Properties.TryGetValue("friends", out data)
                && data is IEnumerable<User>)
            {
                var friends = (IEnumerable<User>)data;
                ViewModel.AddContacts(friends.Select(friend => new Model.Contact(friend)));
            }
        }

        private void ContactsListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items == null || e.Items.Count == 0)
            {
                e.Cancel = true;
                return;
            }
            var contact = (ContactViewModel)e.Items.First();
            e.Data.RequestedOperation = DataPackageOperation.Copy;
            e.Data.SetText(contact.ScreenName);
            e.Data.Properties.Add("friendId", contact.Contact.VKID);
        }

        private void ContactsListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs e)
        {
            if (e.DropResult == DataPackageOperation.None)
            {
                ViewModel.RemoveContacts(from ContactViewModel contact in e.Items select contact.Contact);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Date = DatePicker.Date;
            try
            {
                ViewModel.Delete(DBContext);
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
                this.Frame.GoBack();
            }
            catch (InvalidOperationException exception)
            {

            }
        }
    }
}
