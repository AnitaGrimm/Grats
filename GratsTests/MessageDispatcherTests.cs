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
        class FakeVkConnector : MessageDispatcherVkConnector
        {
            public List<User> Users = new List<User>();
            public List<string> Messages = new List<string>();
            public VkApiException Exception = null;

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
        IMessageDispatcher dispatcher;

        Category
            goodCategoryA,
            goodCategoryB,
            badCategory;
        Grats.Model.Contact
            goodContactA1, goodContactA2,
            goodContactB1, goodContactB2,
            badContact1, badContact2;
        User
            user1, user2;
        
        public MessageDispatcherTests()
        {
            db = (App.Current as App).dbContext;
            dispatcher = new MessageDispatcher(db, vk);

            goodCategoryA = new GeneralCategory { Name = "A", Template = "A" };
            goodContactA1 = new Grats.Model.Contact { Category = goodCategoryA, VKID = 1, ScreenName = "User1" };
            goodContactA2 = new Grats.Model.Contact { Category = goodCategoryA, VKID = 2, ScreenName = "User2" };

            goodCategoryB = new GeneralCategory { Name = "B", Template = "B" };
            goodContactB1 = new Grats.Model.Contact { Category = goodCategoryB, VKID = 1, ScreenName = "User1" };
            goodContactB2 = new Grats.Model.Contact { Category = goodCategoryB, VKID = 2, ScreenName = "User2" };

            badCategory = new GeneralCategory { Name = "bad", Template = "^" };
            badContact1 = new Grats.Model.Contact { Category = badCategory, VKID = 1, ScreenName = "User1" };
            badContact2 = new Grats.Model.Contact { Category = badCategory, VKID = 2, ScreenName = "User2" };

            db.Add(goodCategoryA);
            db.Add(goodCategoryB);
            db.Add(badCategory);

            db.Add(goodContactA1);
            db.Add(goodContactA2);
            db.Add(goodContactB1);
            db.Add(goodContactB2);
            db.Add(badContact1);
            db.Add(badContact2);

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
            db.Database.ExecuteSqlCommand("delete from [messagetasks]");
            db.Dispose();
        }

        [Fact]
        public void CanSendSingleMessage()
        {
            var task = new MessageTask
            {
                Category = goodCategoryA,
                Contact = goodContactA1,
                DispatchDate = DateTime.Today.AddDays(-1),
                Status = MessageTask.TaskStatus.New,
            };

            db.MessageTasks.Add(task);
            db.SaveChanges();

            int counter = 0;

            dispatcher.OnTaskHandled += (s, e) => { ++counter; };
            dispatcher.Dispatch();

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
                Contact = goodContactA1,
                DispatchDate = DateTime.Today.AddDays(-1),
                Status = MessageTask.TaskStatus.New,
            };

            var taskY = new MessageTask
            {
                Category = goodCategoryA,
                Contact = goodContactA2,
                DispatchDate = DateTime.Today.AddDays(-1),
                Status = MessageTask.TaskStatus.Retry,
            };

            var taskZ = new MessageTask
            {
                Category = goodCategoryB,
                Contact = goodContactB1,
                DispatchDate = DateTime.Today.AddDays(10), // не должна быть отправлена
                Status = MessageTask.TaskStatus.New,
            };

            db.Add(taskX);
            db.Add(taskY);
            db.Add(taskZ);
            db.SaveChanges();

            int counter = 0;
            dispatcher.OnTaskHandled += (s, e) => { ++counter; };

            dispatcher.Dispatch();

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
                Contact = badContact1,
                DispatchDate = DateTime.Today.AddDays(-1),
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
            dispatcher.Dispatch();

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
                Contact = goodContactA1,
                DispatchDate = DateTime.Today.AddDays(-1),
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

            dispatcher.Dispatch();

            db.Entry(task).Reload();

            Assert.Equal(MessageTask.TaskStatus.Pending, task.Status);
            Assert.True(task.LastTryDate > task.DispatchDate);

            Assert.True(vk.HasSent());

            Assert.Equal(1, counter);
        }
    }
}
