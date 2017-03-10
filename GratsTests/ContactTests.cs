using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using VkNet.Model;
using Grats.Model;
using Microsoft.EntityFrameworkCore;

namespace GratsTests
{
    [Collection("Model tests")]
    public class ContactTests
    {   
        [Fact]
        public void CanCreateFromVKUser()
        {
            var vkuser = new User()
            {
                FirstName = "Foo",
                LastName = "Bar",
                BirthDate = "25.8.1990",
                BirthdayVisibility = VkNet.Enums.BirthdayVisibility.Full,
                Photo100 = new Uri("https://pp.userapi.com/c837429/v837429341/13f59/WXwQuiIamSw.jpg")
            };

            var contact = new Grats.Model.Contact(vkuser);
            Assert.Equal(contact.Birthday.Value.Day, 25);
            Assert.Equal(contact.Birthday.Value.Month, 8);

            vkuser.BirthdayVisibility = VkNet.Enums.BirthdayVisibility.OnlyDayAndMonth;
            vkuser.BirthDate = "25.8";
            contact = new Grats.Model.Contact(vkuser);
            Assert.Equal(contact.Birthday.Value.Day, 25);
            Assert.Equal(contact.Birthday.Value.Month, 8);

            vkuser.BirthdayVisibility = VkNet.Enums.BirthdayVisibility.OnlyDayAndMonth;
            vkuser.BirthDate = "5.8";
            contact = new Grats.Model.Contact(vkuser);
            Assert.Equal(contact.Birthday.Value.Day, 5);
            Assert.Equal(contact.Birthday.Value.Month, 8);

            vkuser.BirthdayVisibility = VkNet.Enums.BirthdayVisibility.Invisible;
            contact = new Grats.Model.Contact(vkuser);
            Assert.False(contact.Birthday.HasValue);
        }

        [Fact]
        public void CanSaveContactWithEmptyBirthday()
        {
            var db = (App.Current as App).dbContext;
            try
            {
                var contact = new Grats.Model.Contact()
                {
                    ScreenName = "Maru",
                };
                db.Contacts.Add(contact);
                db.SaveChanges();
            }
            finally
            {
                db.Database.ExecuteSqlCommand("delete from [contacts]");
            }

        }
    }
}
