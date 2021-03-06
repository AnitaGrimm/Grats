﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Grats.Model;

namespace Grats.Migrations
{
    [DbContext(typeof(GratsDBContext))]
    [Migration("20170407113424_CategoryTimeMigration")]
    partial class CategoryTimeMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("Grats.Model.Category", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Color");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<string>("Name");

                    b.Property<long>("OwnersVKID");

                    b.Property<string>("Template");

                    b.Property<TimeSpan>("Time");

                    b.HasKey("ID");

                    b.ToTable("Categories");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Category");
                });

            modelBuilder.Entity("Grats.Model.CategoryContact", b =>
                {
                    b.Property<long>("CategoryID");

                    b.Property<long>("ContactID");

                    b.HasKey("CategoryID", "ContactID");

                    b.HasIndex("ContactID");

                    b.ToTable("CategoryContacts");
                });

            modelBuilder.Entity("Grats.Model.Contact", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("Birthday");

                    b.Property<string>("PhotoUri");

                    b.Property<string>("ScreenName");

                    b.Property<long>("VKID");

                    b.HasKey("ID");

                    b.HasAlternateKey("VKID");

                    b.ToTable("Contacts");
                });

            modelBuilder.Entity("Grats.Model.MessageTask", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CategoryID");

                    b.Property<long?>("ContactID");

                    b.Property<DateTime>("DispatchDate");

                    b.Property<DateTime>("LastTryDate");

                    b.Property<int>("Status");

                    b.Property<string>("StatusMessage");

                    b.HasKey("ID");

                    b.HasIndex("CategoryID");

                    b.HasIndex("ContactID");

                    b.ToTable("MessageTasks");
                });

            modelBuilder.Entity("Grats.Model.Template", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsEmbedded");

                    b.Property<string>("Name");

                    b.Property<string>("Text");

                    b.HasKey("ID");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("Grats.Model.BirthdayCategory", b =>
                {
                    b.HasBaseType("Grats.Model.Category");


                    b.ToTable("BirthdayCategory");

                    b.HasDiscriminator().HasValue("BirthdayCategory");
                });

            modelBuilder.Entity("Grats.Model.GeneralCategory", b =>
                {
                    b.HasBaseType("Grats.Model.Category");

                    b.Property<DateTime>("Date");

                    b.ToTable("GeneralCategory");

                    b.HasDiscriminator().HasValue("GeneralCategory");
                });

            modelBuilder.Entity("Grats.Model.CategoryContact", b =>
                {
                    b.HasOne("Grats.Model.Category", "Category")
                        .WithMany("CategoryContacts")
                        .HasForeignKey("CategoryID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Grats.Model.Contact", "Contact")
                        .WithMany("CategoryContacts")
                        .HasForeignKey("ContactID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Grats.Model.MessageTask", b =>
                {
                    b.HasOne("Grats.Model.Category", "Category")
                        .WithMany("Tasks")
                        .HasForeignKey("CategoryID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Grats.Model.Contact", "Contact")
                        .WithMany("Tasks")
                        .HasForeignKey("ContactID");
                });
        }
    }
}
