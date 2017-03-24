using Grats.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Grats.ViewModels
{
    public class ContactViewModel
    {
        public Contact Contact { get; set; }
        public BitmapImage Photo { get; private set; }
        public double Opacity { get; set; }
        public string ScreenName
        {
            get
            {
                return this.Contact.ScreenName;
            }
        }

        public ContactViewModel() { }

        public ContactViewModel(Contact contact, bool IsBirthday = false)
        {
            this.Contact = contact;
            this.Photo = new BitmapImage(new Uri(contact.PhotoUri));
            this.Opacity = IsBirthday && Contact.Birthday == null ? 0.25 : 1;
        }
    }
}
