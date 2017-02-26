using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Grats.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ModelTests
{
    [TestClass]
    public class CategoryTests
    {
        
        [TestMethod]
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
            Assert.IsTrue(contact.Category is GeneralCategory);
            Assert.IsFalse(contact.Category is BirthdayCategory);
        }
    }
}
