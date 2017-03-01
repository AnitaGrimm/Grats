using System;
using Grats.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Xunit;

namespace GratsTests
{
    [Collection("Model tests")]
    public class CategoryTest
    {
        [Fact]
        public void CanCreateGeneralСategory()
        {
            var db = (App.Current as App).dbContext;
            try
            {
                var category = new GeneralCategory()
                {
                    Name = "Simple",
                    Date = new DateTime(2016, 05, 14)
                };

                category.Contacts = new List<Contact>
            {
                new Contact()
                {
                    ScreenName = "Foobaar"
                }
            };

                db.Categories.Add(category);
                db.SaveChanges();

                var contact = db.Contacts.ToList().First();
                Assert.True(contact.Category is GeneralCategory);
                Assert.False(contact.Category is BirthdayCategory);
            }
            finally
            {
                db.Database.ExecuteSqlCommand("delete from [categories]");
                db.Database.ExecuteSqlCommand("delete from [contacts]");
            }
        }
    }
}
