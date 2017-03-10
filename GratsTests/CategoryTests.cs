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

                category.CategoryContacts = new List<CategoryContact>
                {
                    new CategoryContact()
                    {
                        Category = category,
                        Contact = new Contact()
                        {
                            ScreenName = "Foobaar"
                        }
                    }
                };

                db.Categories.Add(category);
                db.SaveChanges();

                var contact = db.Contacts
                    .Include(c => c.CategoryContacts)
                    .ThenInclude(cc => cc.Category)
                    .ToList().First();
                Assert.True(contact.CategoryContacts.First().Category is GeneralCategory);
                Assert.False(contact.CategoryContacts.First().Category is BirthdayCategory);
            }
            finally
            {
                db.Database.ExecuteSqlCommand("delete from [categories]");
                db.Database.ExecuteSqlCommand("delete from [contacts]");
                db.Database.ExecuteSqlCommand("delete from [categorycontacts]");
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

                category.CategoryContacts = new List<CategoryContact>
                {
                    new CategoryContact()
                    {
                        Category = category,
                        Contact = new Contact()
                        {
                            ScreenName = "Foobaar"
                        }
                    }
                };

                category.Tasks = new List<MessageTask>
                {
                    new MessageTask()
                    {
                        DispatchDate = DateTime.Now,
                        Contact = category.CategoryContacts.First().Contact
                    }
                };

                db.Categories.Add(category);
                db.SaveChanges();

                Assert.NotEmpty(db.Contacts);
                Assert.NotEmpty(db.CategoryContacts);
                Assert.NotEmpty(db.MessageTasks);

                db.Categories.Remove(db.Categories.First());
                db.SaveChanges();

                Assert.NotEmpty(db.Contacts);
                Assert.Empty(db.CategoryContacts);
                Assert.Empty(db.MessageTasks);
            }
            finally
            {
                db.Database.ExecuteSqlCommand("delete from [categories]");
                db.Database.ExecuteSqlCommand("delete from [contacts]");
                db.Database.ExecuteSqlCommand("delete from [categorycontacts]");
            }
        }

        [Fact]
        public void CanCreateGeneralCategoryFromBirthday()
        {
            var db = (App.Current as App).dbContext;
            try
            {
                var category = new BirthdayCategory()
                {
                    Name = "Simple"
                };

                category.CategoryContacts = new List<CategoryContact>
                {
                    new CategoryContact()
                    {
                        Category = category,
                        Contact = new Contact()
                        {
                            ScreenName = "Foobaar"
                        }
                    }
                };

                db.Categories.Add(category);
                db.SaveChanges();

                category = db.BirthdayCategories
                    .Include(c => c.CategoryContacts)
                    .ThenInclude(cc => cc.Category)
                    .First();
                var generalCategory = new GeneralCategory(category, DateTime.Now);
                db.BirthdayCategories.Remove(category);
                db.GeneralCategories.Add(generalCategory);
                db.SaveChanges();

                generalCategory = db.GeneralCategories
                    .Include(c => c.CategoryContacts)
                    .ThenInclude(cc => cc.Category)
                    .First();
                Assert.NotEmpty(generalCategory.CategoryContacts);
                Assert.Equal(db.CategoryContacts.Count(), 1);
                Assert.Equal(db.Contacts.Count(), 1);
            }
            finally
            {
                db.Database.ExecuteSqlCommand("delete from [categories]");
                db.Database.ExecuteSqlCommand("delete from [contacts]");
                db.Database.ExecuteSqlCommand("delete from [categorycontacts]");
            }
        }

        [Fact]
        public void CanGenerateTasksFromCategory()
        {
            var db = (App.Current as App).dbContext;
            try
            {
                var category = new GeneralCategory()
                {
                    Name = "Simple",
                    Date = new DateTime(2016, 05, 14)
                };

                category.CategoryContacts = new List<CategoryContact>
                {
                    new CategoryContact()
                    {
                        Category = category,
                        Contact = new Contact()
                        {
                            VKID = 1,
                            ScreenName = "Foo"
                        }
                    },
                    new CategoryContact()
                    {
                        Category = category,
                        Contact = new Contact()
                        {
                            VKID = 2,
                            ScreenName = "Bar"
                        }
                    }
                };

                db.Categories.Add(category);
                db.SaveChanges();

                category.Generate(db);

                var tasks = db.MessageTasks
                    .Include(t => t.Category)
                    .Include(t => t.Contact)
                    .ToList();

                Assert.Equal(2, tasks.Count);
                Assert.All(tasks, t => Assert.Equal(category, t.Category));
                Assert.True(tasks.Any(t => t.Contact.ScreenName == "Foo"));
                Assert.True(tasks.Any(t => t.Contact.ScreenName == "Bar"));
                Assert.All(tasks, t =>
                {
                    Assert.True(t.DispatchDate >= DateTime.Today);
                    Assert.Equal(category.Date.Day, t.DispatchDate.Day);
                    Assert.Equal(category.Date.Month, t.DispatchDate.Month);
                    Assert.Equal(MessageTask.TaskStatus.New, t.Status);
                });
            }
            finally
            {
                db.Database.ExecuteSqlCommand("delete from [categories]");
                db.Database.ExecuteSqlCommand("delete from [contacts]");
                db.Database.ExecuteSqlCommand("delete from [categorycontacts]");
                db.Database.ExecuteSqlCommand("delete from [messagetasks]");
            }
        }
    }
}
