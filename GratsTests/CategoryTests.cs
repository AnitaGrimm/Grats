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

        [Fact]
        public void CanDeleteCascade()
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

                category.Tasks = new List<MessageTask>
                {
                    new MessageTask()
                    {
                        DispatchDate = DateTime.Now,
                        Contact = category.Contacts.First()
                    }
                };

                db.Categories.Add(category);
                db.SaveChanges();

                Assert.NotEmpty(db.Contacts);
                Assert.NotEmpty(db.MessageTasks);

                db.Categories.Remove(db.Categories.First());
                db.SaveChanges();

                Assert.Empty(db.Contacts);
                Assert.Empty(db.MessageTasks);
            }
            finally
            {
                db.Database.ExecuteSqlCommand("delete from [categories]");
                db.Database.ExecuteSqlCommand("delete from [contacts]");
            }
        }

        [Fact]
        public void CanDreateGeneralCategoryFromBirthday()
        {
            var db = (App.Current as App).dbContext;
            try
            {
                var category = new BirthdayCategory()
                {
                    Name = "Simple"
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

                category = db.BirthdayCategories
                    .Include("Contacts")
                    .First();
                var generalCategory = new GeneralCategory(category, DateTime.Now);
                db.BirthdayCategories.Remove(category);
                db.GeneralCategories.Add(generalCategory);
                db.SaveChanges();

                generalCategory = db.GeneralCategories
                    .Include("Contacts")
                    .First();
                Assert.NotEmpty(generalCategory.Contacts);
                Assert.Equal(db.Contacts.Count(), 1);
            }
            finally
            {
                db.Database.ExecuteSqlCommand("delete from [categories]");
                db.Database.ExecuteSqlCommand("delete from [contacts]");
            }
        }
    }
}
