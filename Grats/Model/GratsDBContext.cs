using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageTask>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Tasks)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MessageTask>()
                .HasOne(p => p.Contact)
                .WithMany(c => c.Tasks)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Contact>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Contacts)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
