using Grats.Model;
using Grats.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Grats
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TemplatesMasterPage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<Template> Templates = new ObservableCollection<Model.Template>();
        public Template selectedTemplate = null;
        public Template SelectedTemplate
        {
            get { return selectedTemplate; }
            set
            {
                selectedTemplate = value;
                OnPropertyChanged("AcceptButtonVisibility");
                OnPropertyChanged("DeleteButtonVisibility");
                if (value == null)
                    TemplateDetailFrame.GoBack();
            }
        }
        public CategoryDetailViewModel ViewModel;

        public Visibility AcceptButtonVisibility =>
            SelectedTemplate == null ? Visibility.Collapsed : Visibility.Visible;
        public Visibility DeleteButtonVisibility =>
            SelectedTemplate != null && !SelectedTemplate.IsEmbedded ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public TemplatesMasterPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is CategoryDetailViewModel)
                ViewModel = e.Parameter as CategoryDetailViewModel;
            else
                throw new NotSupportedException("Parameter not supported");
            UpdateUI();
        }

        private void UpdateUI()
        {
            UpdateTemplateList();
            TemplateDetailFrame.Navigate(typeof(TemplatePlaceholderPage));
        }

        private void UpdateTemplateList()
        {
            var db = (App.Current as App).dbContext;
            foreach (var t in db.Templates)
                Templates.Add(t);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
        
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTemplate = (sender as ListView).SelectedItem as Template;
            if (SelectedTemplate != null)
            {
                while (TemplateDetailFrame.CanGoBack)
                    TemplateDetailFrame.GoBack();
                TemplateDetailFrame.Navigate(typeof(TemplatesDetailPage), SelectedTemplate);
            }

        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MessageText = SelectedTemplate.Text;
            this.Frame.GoBack();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var db = (App.Current as App).dbContext;
            db.Templates.Remove(SelectedTemplate);
            db.SaveChanges();
            Templates.Remove(SelectedTemplate);
            TemplatesListView.SelectedItem = null;
        
        }
    }
}
