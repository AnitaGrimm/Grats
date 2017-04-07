using Grats.Interfaces;
using Grats.MessageDispatcher;
using Grats.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Exception;

namespace GratsTests
{
    [Collection("Model tests")]
    public class MessageDispatcherTests : IDisposable
    {
        const long UserId = 1000;
        static readonly DateTime TestDate = new DateTime(2017, 4, 1, 12, 0, 0);

        class FakeVkConnector : MessageDispatcherVkConnector
        {
            public List<User> Users = new List<User>();
            public List<string> Messages = new List<string>();
            public VkApiException Exception = null;

            public override long GetCurrentUserId()
            {
                return UserId;
            }

            public override User GetUser(long userId, ProfileFields fields)
            {
                if (Exception != null)
                    throw Exception;
                var user = Users.FirstOrDefault(u => u.Id == userId);
                Assert.NotNull(user);
                return user;
            }

            public override long SendMessage(long userId, string message)
            {
                if (Exception != null)
                    throw Exception;
                Messages.Add(string.Format("{0}: {1}", userId, message));
                return 0;
            }

            public bool HasSent(params string[] messages)
            {
                return messages.OrderBy(s => s).SequenceEqual(Messages.OrderBy(s => s));
            }
        }

        GratsDBContext db;
        FakeVkConnector vk = new FakeVkConnector();
        MessageDispatcher dispatcher;

        Category
            goodCategoryA,
            goodCategoryB,
            badCategory,
            otherCategory;
        Grats.Model.Contact
            contact1,
            contact2;
        User
            user1,
            user2;
        
        public MessageDispatcherTests()
        {
            db = (App.Current as App).dbContext;
            dispatcher = new MessageDispatcher(db, vk);

            goodCategoryA = new GeneralCategory { Name = "A", Template = "A", OwnersVKID = UserId };
            goodCategoryB = new GeneralCategory { Name = "B", Template = "B", OwnersVKID = UserId };
            badCategory = new GeneralCategory { Name = "bad", Template = "^", OwnersVKID = UserId };
            otherCategory = new GeneralCategory { Name = "bad", Template = "^", OwnersVKID = UserId + 1 };

            contact1 = new Grats.Model.Contact { VKID = 1, ScreenName = "UserX" };
            contact2 = new Grats.Model.Contact { VKID = 2, ScreenName = "UserY" };
            
            db.Add(goodCategoryA);
            db.Add(goodCategoryB);
            db.Add(badCategory);

            db.Add(contact1);
            db.Add(contact2);
            
            db.SaveChanges();

            user1 = new User { Id = 1, ScreenName = "User1" };
            user2 = new User { Id = 2, ScreenName = "User2" };
            
            vk.Users.Add(user1);
            vk.Users.Add(user2);
        }

        public void Dispose()
        {
            db.Database.ExecuteSqlCommand("delete from [categories]");
            db.Database.ExecuteSqlCommand("delete from [contacts]");
            db.Database.ExecuteSqlCommand("delete from [categorycontacts]");
            db.Database.ExecuteSqlCommand("delete from [messagetasks]");
            db.Dispose();
        }

        [Fact]
        public void CanSendSingleMessage()
        {
            var task = new MessageTask
            {
                Category = goodCategoryA,
                Contact = contact1,
                DispatchDate = TestDate.AddHours(-1),
                Status = MessageTask.TaskStatus.New,
            };

            db.MessageTasks.Add(task);
            db.SaveChanges();

            int counter = 0;

            dispatcher.OnTaskHandled += (s, e) => { ++counter; };
            dispatcher.Dispatch(TestDate);

            db.Entry(task).Reload();

            Assert.Equal(MessageTask.TaskStatus.Done, task.Status);
            Assert.True(task.LastTryDate > task.DispatchDate);

            Assert.True(vk.HasSent("1: A"));

            Assert.Equal(1, counter);
        }

        [Fact]
        public void ShouldOnlySendDueMessages()
        {
            var taskX = new MessageTask
            {
                Category = goodCategoryA,
                Contact = contact1,
                DispatchDate = TestDate.AddHours(-1),
                Status = MessageTask.TaskStatus.New,
            };

            var taskY = new MessageTask
            {
                Category = goodCategoryA,
                Contact = contact2,
                DispatchDate = TestDate.AddHours(-1),
                Status = MessageTask.TaskStatus.Retry,
            };

            var taskZ = new MessageTask
            {
                Category = goodCategoryB,
                Contact = contact1,
                DispatchDate = TestDate.AddHours(10), // не должна быть отправлена
                Status = MessageTask.TaskStatus.New,
            };

            db.Add(taskX);
            db.Add(taskY);
            db.Add(taskZ);
            db.SaveChanges();

            int counter = 0;
            dispatcher.OnTaskHandled += (s, e) => { ++counter; };

            dispatcher.Dispatch(TestDate);

            db.Entry(taskX).Reload();
            db.Entry(taskY).Reload();
            db.Entry(taskZ).Reload();

            Assert.Equal(MessageTask.TaskStatus.Done, taskX.Status);
            Assert.Equal(MessageTask.TaskStatus.Done, taskY.Status);
            Assert.Equal(MessageTask.TaskStatus.New, taskZ.Status);

            Assert.True(vk.HasSent("1: A", "2: A"));

            Assert.Equal(2, counter);
        }

        [Fact]
        public void ShouldMarkFailed()
        {
            var task = new MessageTask
            {
                Category = badCategory,
                Contact = contact1,
                DispatchDate = TestDate.AddHours(-1),
                Status = MessageTask.TaskStatus.New,
            };

            db.MessageTasks.Add(task);
            db.SaveChanges();

            int counter = 0;

            dispatcher.OnTaskHandled += (s, e) =>
            {
                ++counter;
                Assert.NotNull(e.Exception);
            };
            dispatcher.Dispatch(TestDate);

            db.Entry(task).Reload();

            Assert.Equal(MessageTask.TaskStatus.Pending, task.Status);
            Assert.True(task.LastTryDate > task.DispatchDate);

            Assert.True(vk.HasSent());

            Assert.Equal(1, counter);
        }

        [Fact]
        public void ShouldPropagateVkError()
        {
            var task = new MessageTask
            {
                Category = goodCategoryA,
                Contact = contact1,
                DispatchDate = TestDate.AddHours(-1),
                Status = MessageTask.TaskStatus.New,
            };

            db.MessageTasks.Add(task);
            db.SaveChanges();

            vk.Exception = new VkApiException();

            int counter = 0;
            dispatcher.OnTaskHandled += (s, e) =>
            {
                ++counter;
                Assert.NotNull(e.Exception);
                Assert.True(e.Exception.InnerException is VkApiException);
            };

            dispatcher.Dispatch(TestDate);

            db.Entry(task).Reload();

            Assert.Equal(MessageTask.TaskStatus.Pending, task.Status);
            Assert.True(task.LastTryDate > task.DispatchDate);

            Assert.True(vk.HasSent());

            Assert.Equal(1, counter);
        }

        [Fact]
        public void ShouldRefreshTasks()
        {
            var dispatchDate = TestDate.AddHours(-1);

            var task = new MessageTask
            {
                Category = goodCategoryA,
                Contact = contact1,
                DispatchDate = dispatchDate,
                Status = MessageTask.TaskStatus.New,
            };

            db.MessageTasks.Add(task);
            db.SaveChanges();

            dispatcher.Dispatch(TestDate);

            db.Entry(task).Reload();

            Assert.Equal(MessageTask.TaskStatus.Done, task.Status);

            Assert.Equal(2, db.MessageTasks.Count());

            var newTask = db.MessageTasks
                .OrderByDescending(t => t.DispatchDate).First();

            Assert.Equal(goodCategoryA.ID, newTask.CategoryID);
            Assert.Equal(contact1.ID, newTask.ContactID);
            Assert.Equal(MessageTask.TaskStatus.New, newTask.Status);
            Assert.Equal(dispatchDate.AddYears(1), newTask.DispatchDate);
        }

        [Fact]
        public void ShouldIgnoreOthersMessages()
        {
            var task = new MessageTask
            {
                Category = otherCategory,
                Contact = contact1,
                DispatchDate = TestDate.AddHours(-1),
                Status = MessageTask.TaskStatus.New,
            };

            db.MessageTasks.Add(task);
            db.SaveChanges();

            dispatcher.Dispatch(TestDate);

            db.Entry(task).Reload();

            Assert.Equal(MessageTask.TaskStatus.New, task.Status);
            Assert.Equal(1, db.MessageTasks.Count());
            Assert.True(vk.HasSent());
        }

        [Fact]
        public void ShouldFailOldMessages()
        {
            var task = new MessageTask
            {
                Category = goodCategoryA,
                Contact = contact1,
                DispatchDate = TestDate.AddDays(-1.5), // пропущенная задача
                Status = MessageTask.TaskStatus.New,
            };

            db.MessageTasks.Add(task);
            db.SaveChanges();

            dispatcher.Dispatch(TestDate);

            db.Entry(task).Reload();

            Assert.Equal(MessageTask.TaskStatus.Pending, task.Status);
            Assert.True(task.LastTryDate > task.DispatchDate);

            Assert.True(vk.HasSent());
        }
    }
}
