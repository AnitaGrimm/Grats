using Grats.Extensions;
using Grats.Model;
using Grats.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace Grats
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EditorPage : Page
    {
        public Category Category;
        public ObservableCollection<ContactViewModel> Contacts = new ObservableCollection<ContactViewModel>();
        public string CategoryName => Category.Name;
        public DateTime Date { get; set; }
        public bool IsBithday { get; set; }
        public bool IsGeneral => !IsBithday;
        public string MessageText => Category.Template;
        public Color Color;

        public EditorPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateUI(e.Parameter as Category);
        }

        private void UpdateUI(Category category)
        {
            this.Category = category;
            if (category is GeneralCategory)
                Date = (category as GeneralCategory).Date;
            else if (category is BirthdayCategory)
                IsBithday = true;
            this.Color = ColorExtensions.FromHex(category.Color);
            var contactViewModels = from contact in category.Contacts
                                    select new ContactViewModel(contact);
            foreach (var vm in contactViewModels)
                Contacts.Add(vm);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
