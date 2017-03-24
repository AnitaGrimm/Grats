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
            Colors.Azure,
            Colors.Firebrick,
            Colors.SkyBlue,
            Colors.LawnGreen,
            Colors.Ivory
        };

        static User DummyUser = new User()
        {
            FirstName = "Иван",
            LastName = "Иванов",
            Sex = VkNet.Enums.Sex.Male
        };

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private GratsDBContext DBContext { get; set; }
        public CategoryDetailViewModel ViewModel { get; set; }
        public ObservableCollection<SolidColorBrush> Brushes = new ObservableCollection<SolidColorBrush>();
        public string TemplatePlaceholderText;
        public string TemplatePreviewText;

        private bool preview = false;
        public bool Preview
        {
            get { return preview;  }
            set
            {
                preview = value;
                if (value)
                {
                    try
                    {
                        TemplatePreviewText = new MessageTemplate(ViewModel.MessageText)
                            .Substitute(DummyUser);
                    }
                    catch (MessageTemplateSyntaxException e)
                    {
                        TemplatePreviewText = e.Message;
                    }
                    catch (Exception e) { }
                }
                OnPropertyChanged("TemplatePreviewText");
                OnPropertyChanged("TemplateVisibility");
                OnPropertyChanged("PreviewVisibility");
            }
        }
        public Visibility TemplateVisibility
        {
            get
            {
                return Preview ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public Visibility PreviewVisibility
        {
            get
            {
                return !Preview ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public DateTime MaxDate { get; private set; }
        public DateTime MinDate { get; private set; }

        public EditorPage()
        {
            MinDate = DateTime.Now;
            MaxDate = DateTime.Now.AddYears(1);
            this.InitializeComponent();
            UpdateUI();
            DBContext = (App.Current as App).dbContext;
            NavigationCacheMode = NavigationCacheMode.Enabled;

        }

        private void UpdateUI()
        {
            var brushes = from color in AvailableColors
                          select new SolidColorBrush(color);
            foreach (var b in brushes)
                Brushes.Add(b);

            // TODO: Выделить в ресурс
            TemplatePlaceholderText = "Синтаксис шаблона: \r\n\r\n" +
                "\t^<поле_значения>\r\n" +
                "\t^<поле_выбора>{ \"<случай>\" : \"<значение>\", \"<случай>\" : \"<значение>\", ... ,\"<значение_по-умолчанию>\" }\r\n\r\n" +
                "Доступные поля:\r\n\r\n";
            foreach (var field in MessageTemplate.AvailableFields)
            {
                TemplatePlaceholderText += field;
                if (MessageTemplate.AvailableFieldValues.Keys.Contains(field))
                {
                    TemplatePlaceholderText += "(" +
                        String.Join(",", MessageTemplate.AvailableFieldValues[field].ToArray()) +
                        ")";
                }
                TemplatePlaceholderText += "\r\n";
            }
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
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.Frame.GoBack();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Избавиться от костылей
            // TOOD: Добавить валидацию
            // TODO: Выделить в отдельный класс, покрыть тестами
            ViewModel.Date = DatePicker.Date;
            if (ViewModel.Validate())
            {
                try
                {
                    ViewModel.Save(DBContext);
                    this.NavigationCacheMode = NavigationCacheMode.Disabled;
                    this.Frame.GoBack();
                }
                catch (InvalidOperationException exception)
                {

                }
            }
        }
        
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            (sender as AppBarButton).Focus(FocusState.Keyboard);
            Preview = !Preview;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.Focus(FocusState.Keyboard);
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(TemplatesMasterPage), ViewModel);
        }

        #region SaveTemplateButton

        private void SaveTemplateAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ValidateMessageText())
                SaveTemplateFlyout.ShowAt(SaveTemplateAppBarButton);
        }

        private void SaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            var db = (App.Current as App).dbContext;
            if (ViewModel.ValidateMessageText() &&
                !string.IsNullOrEmpty(TemplateName.Text)){
                db.Templates.Add(new Model.Template()
                {
                    Name = TemplateName.Text,
                    Text = ViewModel.MessageText
                });
                db.SaveChanges();
                SaveTemplateFlyout.Hide();
            }
        }
        
        private void CancelSaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            SaveTemplateFlyout.Hide();
        }

        #endregion

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
            if (ViewModel.Validate())
            {
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
}
