using Grats.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using Windows.UI.Xaml.Controls;
using System;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Grats
{
    /// <summary>
    /// Главная страница приложения, содержит список друзей и поздравлений
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public VKCurrentUserViewModel Current { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            UpdateUI();
        }

        /// <summary>
        /// Обновление интерфейса
        /// </summary>
        private void UpdateUI()
        {
            // TODO: Подумать насчет переноса этого в параметры окна
            UpdateCurrentUser();
            // TODO: Добавить перехват исключений
            UpdateFriends();
        }

        private void UpdateCurrentUser()
        {
            var app = App.Current as App;
            var usersAPI = app.VKAPI.Users;
            Current = new VKCurrentUserViewModel(usersAPI.Get(
                app.VKAPI.UserId.Value,
                ProfileFields.FirstName | ProfileFields.LastName | ProfileFields.Photo200
                ));
        }

        #region Получение списка друзей

        /// <summary>
        /// Получения списка друзей с VKAPI
        /// </summary>
        /// <returns>Список друзей</returns>
        public async Task<List<User>> FetchFriends()
        {
            var app = App.Current as App;
            var result = await Task.Run<VkCollection<User>>(() =>
            {
                return app.VKAPI.Friends.Get(new FriendsGetParams()
                {
                    UserId = app.VKAPI.UserId,
                    Fields = ProfileFields.ScreenName |
                    ProfileFields.Photo100 |
                    ProfileFields.FirstName |
                    ProfileFields.LastName |
                    ProfileFields.Sex |
                    ProfileFields.BirthDate
                });
            });
            return result.ToList();
        }

        /// <summary>
        /// Обновление списка друзей
        /// </summary>
        private async void UpdateFriends()
        {
            var app = App.Current as App;
            // Тянем друзей с VK
            var friends = from friend in await FetchFriends()
                          select new VKUserViewModel(friend);
            //Группируем по первой букве фамилии
            var groups = from item in friends
                         group item by item.ScreenName[0] into g
                         orderby g.Key
                         select new { Key = g.Key, Friends = g };

            var collection = new ObservableCollection<FriendsGroupByKey>();
            foreach (var g in groups)
            {
                var group = new FriendsGroupByKey();
                group.Key = g.Key;
                group.AddRange(g.Friends);
                collection.Add(group);
            }
            // Обновляем источник данных
            FriendsGroupedByKey.Source = collection;
        }

        #endregion

        private void LogoutMenuItem_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var app = App.Current as App;
            app.SignOut();
        }
    }
}
