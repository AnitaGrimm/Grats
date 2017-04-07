using Grats.MessageTemplates;
using Grats.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using VkNet.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Grats
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MessageEditorPage : Page, INotifyPropertyChanged
    {
        public CategoryDetailViewModel ViewModel;
        
        static User DummyUser = new User()
        {
            FirstName = "Иван",
            LastName = "Иванов",
            Sex = VkNet.Enums.Sex.Male
        };

        public MessageEditorPage()
        {
            this.InitializeComponent();
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

        public event PropertyChangedEventHandler PropertyChanged;

        public string TemplatePlaceholderText;
        public string TemplatePreviewText;

        private bool preview = false;
        public bool Preview
        {
            get { return preview; }
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

        private void OpenTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(TemplatesMasterPage), ViewModel);
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            Preview = !Preview;
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
                !string.IsNullOrEmpty(TemplateName.Text))
            {
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

        private void OpenTemplateReferenceButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(TemplateReferencePage));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel = e.Parameter as CategoryDetailViewModel;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
