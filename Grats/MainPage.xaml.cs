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
using Grats.Model;
using Microsoft.EntityFrameworkCore;
using Windows.UI.Xaml.Media.Animation;
using static Grats.EditorPage;
using System.ComponentModel;
using Windows.UI.Xaml;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using VkNet.Exception;
using System.Net.Http;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Grats
{
    /// <summary>
    /// Главная страница приложения, содержит список друзей и поздравлений
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public VKCurrentUserViewModel Current { get; set; }
        public ObservableCollection<CategoryMasterViewModel> Categories = new ObservableCollection<CategoryMasterViewModel>();
        ObservableCollection<FriendsGroupByKey> friendsByKeyDefault;
        List<VKUserViewModel> selectedUsers;


        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public MainPage()
        {
            this.InitializeComponent();
            UpdateUI();
            MainFrame.Navigated += MainFrame_Navigated;
        }

        private void MainFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode == Windows.UI.Xaml.Navigation.NavigationMode.Back)
                UpdateCategories();
        }

        /// <summary>
        /// Обновление интерфейса
        /// </summary>
        private void UpdateUI()
        {
            // TODO: Подумать насчет переноса этого в параметры окна
            UpdateCurrentUser();
            UpdateFriends();
            UpdateCategories();
        }

        private void UpdateCategories()
        {
            var db = (App.Current as App).dbContext;
            var VKAPI = (App.Current as App).VKAPI;
            var categories = Enumerable.Union<Category>(
                db.BirthdayCategories.Where(c => c.OwnersVKID == VKAPI.UserId.Value),
                db.GeneralCategories.Where(c => c.OwnersVKID == VKAPI.UserId.Value));
            var categoriesViewModels = from category in categories
                                      select new CategoryMasterViewModel(category);
            Categories.Clear();
            foreach (var viewModel in categoriesViewModels)
                Categories.Add(viewModel);
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
            List<User> friends = null;
            try
            {
                friends = await FetchFriends();
            }
            // TODO: Перехват сетевых исключений
            catch (AccessTokenInvalidException)
            {
                (App.Current as App).SignOut();
                return;
            }
            catch (HttpRequestException e)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Не удалось получить список друзей",
                    Content = e.Message,
                    PrimaryButtonText = "Повторить",
                    SecondaryButtonText = "Выход"
                };
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    UpdateFriends();
                    return;
                }
                else
                {
                    (App.Current as App).SignOut();
                    return;
                }
            }
            // Показываем сводную страницу
            UpdateSummaryPage(friends);
            var friendsVM = from friend in friends
                            select new VKUserViewModel(friend);
            // Группируем по первой букве фамилии
            friendsByKeyDefault = GroupFriendsByKey(friendsVM);
            FriendsGroupedByKey.Source = friendsByKeyDefault;
        }
        public ObservableCollection<FriendsGroupByKey> GroupFriendsByKey(IEnumerable<VKUserViewModel> friendsVM)
        {
            if (friendsVM == null || friendsVM.Count() == 0)
                return new ObservableCollection<FriendsGroupByKey>();
            var groups = from item in friendsVM
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
            return collection;
        }
        #endregion

        private void UpdateSummaryPage(List<User> friends)
        {
            MainFrame.Navigate(typeof(SummaryPage), friends);
        }

        private void LogoutMenuItem_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var app = App.Current as App;
            app.SignOut();
        }

        private void CreateCategoryButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var selectedFriends = from viewModel in FriendsListView.SelectedItems
                                  select (viewModel as VKUserViewModel).User;
            if (selectedFriends.Count() != 0)
                CreateCategory(selectedFriends);
            FriendsListView.SelectedItems.Clear();
            IsSelecting = false;
        }

        private void CreateCategory(IEnumerable<User> friends)
        {
            var app = App.Current as App;
            var category = new Model.Category()
            {
                OwnersVKID = app.VKAPI.UserId.Value,
                Color = Category.DefaultColor,
            };
            category.CategoryContacts =
                (from friend in friends
                 select new Model.CategoryContact(category, new Model.Contact(friend))).ToList();
            ShowCategoryEditorPage(category);
        }

        private void ShowCategoryEditorPage(Model.Category category)
        {
            while (MainFrame.CanGoBack)
                MainFrame.GoBack();
            MainFrame.Navigate(
                typeof(EditorPage), 
                new NewCategoryParameter()
                {
                    Category = category
                },
                new DrillInNavigationTransitionInfo());
        }

        private void ShowCategoryEditorPage(long id, Type categoryType)
        {
            while (MainFrame.CanGoBack)
                MainFrame.GoBack();
            MainFrame.Navigate(
                typeof(EditorPage),
                new EditCategoryParameter()
                {
                    ID = id,
                    CategoryType = categoryType
                }, 
                new DrillInNavigationTransitionInfo());
        }

        private void CategoriesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var category = (e.ClickedItem as CategoryMasterViewModel).Category;
            ShowCategoryEditorPage(category.ID, category.GetType());
        }

        #region Выбор пользователей

        public bool isSelecting = false;
        public bool IsSelecting
        {
            get { return isSelecting; }
            set
            {
                isSelecting = value;
                OnPropertyChanged();
                OnPropertyChanged("SelectionButtonsVisibility");
                OnPropertyChanged("FriendsSelectionMode");
                OnPropertyChanged("SelectButtonVisibility");
            }
        }
        public Visibility SelectionButtonsVisibility => 
            IsSelecting ? Visibility.Visible : Visibility.Collapsed;
        public ListViewSelectionMode FriendsSelectionMode =>
            IsSelecting ? ListViewSelectionMode.Multiple : ListViewSelectionMode.None;
        public Visibility SelectButtonVisibility =>
            IsSelecting ? Visibility.Collapsed : Visibility.Visible;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelecting)
                IsSelecting = false;
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            FriendsListView.SelectAll();
        }
        
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSelecting)
                IsSelecting = true;
        }

        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ColorizeTitleBar();
        }

        private void ColorizeTitleBar()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = (this.Background as SolidColorBrush).Color;
        }

        private void FriendsSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            var textbox = ((TextBox)sender);
            if (textbox.Text.Replace(" ", "") == "")
            {
                if (FriendsGroupedByKey.Source != friendsByKeyDefault)
                {
                    FriendsGroupedByKey.Source = friendsByKeyDefault;
                    foreach (var item in selectedUsers)
                    {
                        var indexRange = new Windows.UI.Xaml.Data.ItemIndexRange(FriendsListView.Items.IndexOf(item), 1);
                        FriendsListView.SelectRange(indexRange);
                    }
                }
                textbox.Text = "Поиск";
            }
        }

        private void FriendsSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = ((TextBox)sender);
            if (textbox.Text=="Поиск")
                textbox.Text="";
            
        }

        private void FriendsSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = ((TextBox)sender);
            if (textbox.FocusState == FocusState.Unfocused)
                return;
            if (textbox.Text.Replace(" ", "") == "")
            {
                if (FriendsGroupedByKey.Source != friendsByKeyDefault)
                {
                    FriendsGroupedByKey.Source = friendsByKeyDefault;
                    foreach (var item in selectedUsers)
                    {
                        var indexRange = new Windows.UI.Xaml.Data.ItemIndexRange(FriendsListView.Items.IndexOf(item), 1);
                        FriendsListView.SelectRange(indexRange);
                    }
                }

            }
            else
            {
                var searchResult = new ObservableCollection<FriendsGroupByKey>();
                var validFriends = friendsByKeyDefault?.SelectMany(group => group.ToArray())?.Where(friend => IsUserSearchResult(textbox.Text, friend.User) || selectedUsers.IndexOf(friend)>=0);
                searchResult = GroupFriendsByKey(validFriends);
                FriendsGroupedByKey.Source = searchResult;
                foreach (var item in selectedUsers)
                {
                    try
                    {
                        var indexRange = new Windows.UI.Xaml.Data.ItemIndexRange(FriendsListView.Items.IndexOf(item), 1);
                        FriendsListView.SelectRange(indexRange);
                    }
                    catch { }
                }
            }
        }
        public bool IsUserSearchResult(string searchCall, User user)
        {
            var screenName = user.ScreenName;
            var firstName = user.FirstName;
            var lastName = user.LastName;
            return IsSearchResult(searchCall, screenName) || IsSearchResult(searchCall, lastName) || IsSearchResult(searchCall, firstName);
        }
        public bool IsSearchResult(string searchCall, string text)
        {
            if (text!=null && text.ToLower().IndexOf(searchCall.ToLower()) >= 0)
                return true;
            else
                return false;
        }

        private void FriendsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = ((ListView)sender);
            if (listView.SelectedItems == null || listView.SelectedItems.Count() == 0)
                return;
            selectedUsers = listView.SelectedItems.Select(x => (VKUserViewModel)x)?.ToList();
        }
    }
}
