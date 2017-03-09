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
        public string ScreenName
        {
            get
            {
                return this.Contact.ScreenName;
            }
        }

        public ContactViewModel() { }

        public ContactViewModel(Contact contact)
        {
            this.Contact = contact;
            this.Photo = new BitmapImage(new Uri(contact.PhotoUri));
        }
    }
}
