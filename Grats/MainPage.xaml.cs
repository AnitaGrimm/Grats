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

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Grats
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            UpdateUI();
        }

        private void UpdateUI()
        {
            UpdateFriends();
        }

        #region Получение списка друзей

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

        private async void UpdateFriends()
        {
            var app = App.Current as App;
            var friends = from friend in await FetchFriends()
                          select new VKUserViewModel(friend);
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
            FriendsGroupedByKey.Source = collection;
        }

        #endregion
    }
}
