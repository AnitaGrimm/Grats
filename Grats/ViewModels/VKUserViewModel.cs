using System;
using VkNet.Model;
using Windows.UI.Xaml.Media.Imaging;

namespace Grats.ViewModels
{

    public class VKUserViewModel
    {
        public User User { get; set; }
        public String ScreenName { get; set; }
        public BitmapImage Photo { get; set; }
        
        public VKUserViewModel() { }

        public VKUserViewModel(User user)
        {
            this.User = user;
            this.ScreenName = user.LastName + " " + user.FirstName;
            this.Photo = new BitmapImage(user.Photo100);
        }
    }
}
