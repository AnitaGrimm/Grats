
using Windows.UI;
using Grats.Model;
using Grats.ViewModels;
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
using VkNet.Utils;
using VkNet.Model;
using System.Threading.Tasks;
using VkNet.Model.RequestParams;
using VkNet.Enums.Filters;
using Grats.Extensions;
using Microsoft.EntityFrameworkCore;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Grats
{
    public sealed partial class EventsCalendar : Page
    {
        public List<EventCalendarView> CalendarEvents = new List<EventCalendarView>();
        public EventsCalendar()
        {
            this.InitializeComponent();
            UpdateEvents();
        }
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
        public async void UpdateEvents()
        {
            var db = (App.Current as App).dbContext;
            var categories = Enumerable.Union<Category>(
                db.BirthdayCategories.Include(c => c.Tasks),
                db.GeneralCategories.Include(c => c.Tasks));
            //добавление событий
            foreach (var category in categories)
            {
                var color = ColorExtensions.FromHex(category.Color);
                CalendarEvents.AddRange(category?.Tasks?.Select(task => new EventCalendarView { EventColor = color, EventDate = task.DispatchDate }));
            }
            //добавление дней рождений
            var friends = await FetchFriends();
            CalendarEvents.AddRange(db?.Contacts?.Select(contact => new EventCalendarView { EventColor = Colors.LightSkyBlue, EventDate = contact.Birthday.Value }));
            foreach (var friend in friends)
            {
                try {
                    CalendarEvents.Add(new EventCalendarView { EventColor = Colors.LightSkyBlue, EventDate = DateTime.Parse(friend.BirthDate) }); }
                catch { }
            }
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
                if (args.Item.Date < DateTimeOffset.Now)
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
                if (args.Item.Date > DateTimeOffset.Now)
                {
                    // Get bookings for the date being rendered.

                    List<Color> densityColors = new List<Color>();
                    // Set a density bar color for each of the days bookings.
                    // It's assumed that there can't be more than 10 bookings in a day. Otherwise,
                    // further processing is needed to fit within the max of 10 density bars.
                    foreach (var calendarEvent in CalendarEvents)
                    {   
                        if (calendarEvent.EventDate.Day == args.Item.Date.Day && calendarEvent.EventDate.Month == args.Item.Date.Month)
                            densityColors.Add(calendarEvent.EventColor);
                    }
                    if (densityColors.Count>0)
                        args.Item.SetDensityColors(FillColors(densityColors));
                }
            }
        }
        public List<Color> FillColors(List<Color> densityColors)
        {
            List<Color> colors = new List<Color>();
            foreach (var densityColor in densityColors)
                for (int i = 0; i < 10.0/densityColors.Count; i++)
                    colors.Add(densityColor);
            return colors;
        }
    }
}
