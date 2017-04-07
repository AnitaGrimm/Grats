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
using Grats.ViewModels;
using Grats.Model;
using Windows.UI;
using static Grats.EditorPage;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace Grats
{
    /// <summary>
    /// Сводная страница. Появляется первой на главной странице.
    /// Содержит календарь и историю поздравлений.
    /// </summary>
    public sealed partial class SummaryPage : Page
    {
        public List<EventCalendarView> CalendarEvents = new List<EventCalendarView>();
        Point pointerPosition = new Point();
        public SummaryPage()
        {
            this.InitializeComponent();
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var friends = e.Parameter as List<User>;
            UpdateCalendar(friends);
            UpdateHistory();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

        }

        #region Обновление секций

        private void UpdateHistory()
        {
            // TODO: Реализовать
        }

        private void UpdateCalendar(List<User> friends)
        {
            UpdateEvents(friends);
        }
        public async void UpdateEvents(List<User> friends)
        {
            var db = (App.Current as App).dbContext;
            var generalCategories = db.GeneralCategories
                        .Include(c => c.CategoryContacts)
                        .ThenInclude(cc => cc.Contact);
            var birthdayCategories = db.BirthdayCategories
                        .Include(c => c.CategoryContacts)
                        .ThenInclude(cc => cc.Contact);
            //добавление генеральных событий
            CalendarEvents.Clear();
            foreach (var category in generalCategories)
            {
                var color = Extensions.ColorExtensions.FromHex(category.Color);
                EventCalendarView val;
                try
                {
                    val = new EventCalendarView { EventColor = color, EventDate = category.Date, Contacts = category, EventName = category.Name ?? "безымянное событие" };
                }
                catch
                {
                    val = null;
                }
                if (val!=null)
                    CalendarEvents.Add(val);
            }
            //добавление дней рождений, для которых нет поздравлений
            foreach (var friend in friends)
            {
                BirthdayCategory cat = new BirthdayCategory();
                cat.Color = "#00FFFFF0"; 
                cat.Name = "День рождения пользователя " + friend.FirstName + " " + friend.LastName + "(поздравление не установлено)";
                cat.CategoryContacts = new List<CategoryContact>() { new CategoryContact { Category = cat, Contact = new Model.Contact(friend) } };
                EventCalendarView val1=null, val2=null;
                try
                {
                    var date = DateTime.Parse(friend.BirthDate);
                    var color = Color.FromArgb(255, 255, 255, 240);
                    if (!IsInBithdayCategories(birthdayCategories, friend))
                    {
                        val1 = new EventCalendarView { EventColor = color, EventDate = new DateTime(DateTime.Now.Year, date.Month, date.Day), Contacts = cat, EventName = cat.Name };
                    }
                    var nextyearbd = new DateTime(DateTime.Now.Year + 1, date.Month, date.Day);
                    if (nextyearbd<=DateTime.Now.AddYears(1))
                        val2 = new EventCalendarView { EventColor = color, EventDate = nextyearbd, Contacts = cat, EventName = cat.Name };
                }
                catch
                {
                    val1= val2 = null;
                }
                if(val1!=null)
                    CalendarEvents.Add(val1);
                if (val2 != null)
                    CalendarEvents.Add(val2);
            }
            //добавление дней рождений, для которых есть поздравление
            foreach(var category in birthdayCategories)
            {
                var color = Extensions.ColorExtensions.FromHex(category.Color);
                if(category.CategoryContacts!=null)
                    foreach (var cont in category.CategoryContacts)
                    {
                        EventCalendarView val;
                        try
                        {
                            val = new EventCalendarView { EventColor = color, EventDate = cont.Contact.Birthday.Value, Contacts = category, EventName = "День рождения пользователя " + cont.Contact.ScreenName + " (" + (category.Name ?? "безымянное событие") + ")" };
                        }
                        catch
                        {
                            val = null;
                        }
                        if (val != null)
                            CalendarEvents.Add(val);
                    }
            }
        }

        private bool IsInBithdayCategories(IIncludableQueryable<BirthdayCategory, Model.Contact> birthdayCategories, User friend)
        {
            if (birthdayCategories != null)
                foreach (var cat in birthdayCategories)
                    if (cat.CategoryContacts!=null)
                        foreach(var cont in cat.CategoryContacts)
                            if (cont.Contact.VKID == friend.Id)
                                return true;
            return false;
        }
        private void CalendarView_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            // Render basic day items.
            if (args.Phase == 0)
            {
                // Register callback for next phase.
                args.RegisterUpdateCallback(CalendarView_CalendarViewDayItemChanging);
            }
            // Устанавливаем даты, которые затемняем.
            else if (args.Phase == 1)
            {
                // Затемняем прошедшие даты
                if (args.Item.Date < new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, DateTimeOffset.Now.Day, 0, 0, 0, DateTimeOffset.Now.Offset))
                {
                    args.Item.IsBlackout = true;
                }
                // Register callback for next phase.
                args.RegisterUpdateCallback(CalendarView_CalendarViewDayItemChanging);
            }
            // Set density bars.
            else if (args.Phase == 2)
            {
                // Avoid unnecessary processing.
                // Не нужны даты, которые были до нынешней
                if (args.Item.Date >= new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, DateTimeOffset.Now.Day,0,0,0, DateTimeOffset.Now.Offset))
                {
                    // Get bookings for the date being rendered.

                    List<Color> densityColors = new List<Color>();
                    // Set a density bar color for each of the days bookings.
                    // It's assumed that there can't be more than 10 bookings in a day. Otherwise,
                    // further processing is needed to fit within the max of 10 density bars.
                    foreach (var calendarEvent in CalendarEvents)
                    {
                        if (calendarEvent.EventDate.Day == args.Item.Date.Day && calendarEvent.EventDate.Month == args.Item.Date.Month &&  calendarEvent.EventDate.Year == args.Item.Date.Year)
                            densityColors.Add(calendarEvent.EventColor);
                    }
                    if (densityColors.Count > 0)
                        args.Item.SetDensityColors(FillColors(densityColors));
                }
            }
        }
        public List<Color> FillColors(List<Color> densityColors)
        {
            List<Color> colors = new List<Color>();
            foreach (var densityColor in densityColors)
                for (int i = 0; i < 10.0 / densityColors.Count; i++)
                    colors.Add(densityColor);
            return colors;
        }
        private void calendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            var calendar = ((CalendarView)sender);
            var selectedDate = args.AddedDates?.FirstOrDefault();
            if (selectedDate == null || !selectedDate.HasValue || selectedDate.Value == DateTimeOffset.MinValue)
                return;
            var flyout = new MenuFlyout();
            MenuFlyoutItem flyoutItem;
            var eventsOfDay = CalendarEvents?.Where(x => x.EventDate.Day == selectedDate.Value.Day && x.EventDate.Month == selectedDate.Value.Month && x.EventDate.Year == selectedDate.Value.Year);
            if (eventsOfDay != null && eventsOfDay.Count() > 0)
            {
                calendar.ContextFlyout = flyout;
                foreach (var item in eventsOfDay)
                {
                    flyoutItem = new MenuFlyoutItem();
                    flyoutItem.DataContext = item;
                    flyoutItem.Text = item.EventName??" ";
                    flyoutItem.Click += MenuFlyoutItem_Click;
                    flyoutItem.Foreground = new SolidColorBrush(item.EventColor);
                    flyout.Items.Add(flyoutItem);
                }
            }
            calendar.ContextFlyout = flyout;
            flyoutItem = new MenuFlyoutItem();
            flyoutItem.DataContext = new EventCalendarView { EventDate = selectedDate.Value.DateTime, Contacts = new GeneralCategory { Date = selectedDate.Value.DateTime , CategoryContacts = new List<CategoryContact>(), Color="#FFFFFFFF" }, EventColor = Colors.LightGray };
            flyoutItem.Text = "Добавить событие";
            flyoutItem.Click += MenuFlyoutItem_Click;
            flyoutItem.Foreground = new SolidColorBrush(Colors.LightGray);
            flyout.Items.Add(flyoutItem);
            flyout.ShowAt(this, pointerPosition);
        }
        #endregion


        private void calendar_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            pointerPosition = e.GetCurrentPoint(this).Position;
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var eventDesc = (EventCalendarView)((MenuFlyoutItem)sender).DataContext;
            var MainFrame = (Frame)this.Parent;
            var Cat = eventDesc.Contacts;
            var db = (App.Current as App).dbContext;
            var generalCategories = db.GeneralCategories
                        .Include(c => c.CategoryContacts)
                        .ThenInclude(cc => cc.Contact);
            var birthdayCategories = db.BirthdayCategories
                        .Include(c => c.CategoryContacts)
                        .ThenInclude(cc => cc.Contact);
            bool IsEdit = false;
            if (Cat is GeneralCategory )
                IsEdit = generalCategories.ToList().IndexOf(Cat as GeneralCategory) != -1;
            if (Cat is BirthdayCategory)
                IsEdit = birthdayCategories.ToList().IndexOf(Cat as BirthdayCategory) != -1;
            if (IsEdit)
                MainFrame.Navigate(
                    typeof(EditorPage),
                    new EditCategoryParameter()
                    {
                        ID = Cat.ID
                    },
                    new DrillInNavigationTransitionInfo());
            else
            {
                Cat.Name = "";
                MainFrame.Navigate(
                    typeof(EditorPage),
                    new NewCategoryParameter()
                    {
                        Category = Cat
                    },
                    new DrillInNavigationTransitionInfo());
            }
        }
        

        private void CalendarView_Loaded(object sender, RoutedEventArgs e)
        {
            var calendar = (CalendarView)sender;
            calendar.MinDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            calendar.MaxDate = new DateTime(DateTime.Now.Year + 1, DateTime.Now.Month, DateTime.Now.Day);
        }
    }
}
