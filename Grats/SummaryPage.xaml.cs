using System;
using System.Collections.Generic;
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
using VkNet.Model;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace Grats
{
    /// <summary>
    /// Сводная страница. Появляется первой на главной странице.
    /// Содержит календарь и историю поздравлений.
    /// </summary>
    public sealed partial class SummaryPage : Page
    {
        public SummaryPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var friends = e.Parameter as List<User>;
            // TODO: Раскомментировать
            UpdateCalendar(friends);
            // TODO: Раскомментировать
            UpdateHistory();
        }

        #region Обновление секций

        private void UpdateHistory()
        {
            // TODO: Реализовать
            throw new NotImplementedException();
        }

        private void UpdateCalendar(List<User> friends)
        {
            // TODO: Реализовать
            throw new NotImplementedException();
        }

        #endregion
    }
}
