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

namespace GratsTests
{
    [Collection("Model tests")]
    public class MessageDispatcherTests
    {
        class FakeVkConnector : MessageDispatcherVkConnector
        {
            public List<User> Users = new List<User>();
            public string Log = "";

            public override User GetUser(long userId, ProfileFields fields)
            {
                var user = Users.FirstOrDefault(u => u.Id == userId);
                Assert.NotNull(user);
                return user;
            }

            public override long SendMessage(long userId, string message)
            {
                Log += string.Format("{0}: {1}\n", userId, message);
                return 0;
            }
        }
        
        IMessageDispatcher NewDispatcher(GratsDBContext db, FakeVkConnector vk)
        {
            return new MessageDispatcher(db, vk);
        }

        [Fact]
        public void CanSendSingleMessage()
        {
            var db = (App.Current as App).dbContext;
            try
            {
                var category = new Category
                {
                    Template = "привет",
                };
                var contact = new Grats.Model.Contact
                {
                    VKID = 42,
                    ScreenName = "User",
                    Category = category,
                };
                var user = new User
                {
                    Id = 42,
                };
                var task = new MessageTask
                {
                    Category = category,
                    Contact = contact,
                    DispatchDate = DateTime.Today.AddDays(-1),
                    Status = MessageTask.TaskStatus.New,
                };

                db.MessageTasks.Add(task);
                db.SaveChanges();

                var vk = new FakeVkConnector
                {
                    Users = { user },
                };

                var disp = NewDispatcher(db, vk);
                disp.Dispatch();

                db.Entry(task).Reload();

                Assert.Equal(MessageTask.TaskStatus.Done, task.Status);
                Assert.True(task.LastTryDate > task.DispatchDate);

                Assert.Equal("42: привет\n", vk.Log);
            }
            finally
            {
                db.Database.ExecuteSqlCommand("delete from [categories]");
                db.Database.ExecuteSqlCommand("delete from [contacts]");
                db.Database.ExecuteSqlCommand("delete from [messagetasks]");
            }
        }
    }
}
