using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Grats.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ModelTests
{
    [TestClass]
    public class CategoryTests
    {
        
        [TestMethod]
        public void CanCreateGeneralTategory()
        {
            var db = new GratsDBContext();
            db.Database.Migrate();

            var category = new GeneralCategory()
            {
                Name = "Simple",
                Date = new DateTime(2016, 05, 14)
            };

            category.Contacts.Add(new Contact()
            {
                ScreenName = "FooBar"
            });

            db.SaveChanges();

            var contact = db.Contacts.ToList().First();
            Assert.IsTrue(contact.Category is GeneralCategory);
            Assert.IsFalse(contact.Category is BirthdayCategory);
        }
    }
}
