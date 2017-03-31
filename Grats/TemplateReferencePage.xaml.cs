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
    public sealed partial class TemplateReferencePage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<ReferenceViewModel> Items = new ObservableCollection<ReferenceViewModel>
        {
            new ReferenceViewModel(
                "Имя", 
                @"Вставляет имя получателя.

Пример использования:

Добрый день, ^имя!"),
            new ReferenceViewModel(
                "Фамилия",
                @"Вставляет фамилию получателя.

Пример использования:

Добрый день, ^фамилия!"),
            new ReferenceViewModel(
                "Пол",
                @"Вариативный модификатор. Вставляет кусок строки, соответствующий значению пола получателя. 

Возможные значения: ""м"",""ж"",""н""

Пример использования:

Добрый день, сотруд^пол{м: ник, н: ница, ник}!")
        };

        public string detailTitle = @"Шаблонизатор ""Grats""";
        public string DetailTitle
        {
            get { return detailTitle; }
            set
            {
                detailTitle = value;
                OnPropertyChanged();
            }
        }

        public string detailDescription;
        public string DetailDescription
        {
            get { return detailDescription; }
            set
            {
                this.detailDescription = value;
                OnPropertyChanged();
            }
        }


        public TemplateReferencePage()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ModifiersListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = e.ClickedItem as ReferenceViewModel;
            if (selectedItem != null)
            {
                this.DetailTitle = selectedItem.Title;
                this.DetailDescription = selectedItem.Description;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
