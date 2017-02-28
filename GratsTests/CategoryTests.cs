using System;
using Grats.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Xunit;

namespace GratsTests
{
    public class CategoryTests
    {

        [Fact]
        public void CanCreateGeneralСategory()
        {
            var db = new GratsDBContext();
            db.Database.Migrate();

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
    }
}
