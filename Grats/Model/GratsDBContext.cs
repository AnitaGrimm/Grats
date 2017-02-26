using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Grats.Model
{
    /// <summary>
    /// БД контекст приложения
    /// </summary>
    public class GratsDBContext: DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<GeneralCategory> GeneralCategories { get; set; }
        public DbSet<BirthdayCategory> BirthdayCategories { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<MessageTask> MessageTasks { get; set; }
        public DbSet<Template> Templates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=GratsDatabase.db");
        }

    }
}
