using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;
using Windows.UI.Xaml.Media.Imaging;

namespace Grats.ViewModels
{
    public class VKCurrentUserViewModel
    {
        public User User { get; set; }
        public String ScreenName { get; set; }
        public BitmapImage Photo { get; set; }

        public VKCurrentUserViewModel() { }

        public VKCurrentUserViewModel(User user)
        {
            this.User = user;
            this.ScreenName = user.LastName + " " + user.FirstName;
            this.Photo = new BitmapImage(user.Photo200);
        }
    }
}
