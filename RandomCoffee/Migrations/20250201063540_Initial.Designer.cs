﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RandomCoffee;

#nullable disable

namespace RandomCoffee.Migrations
{
    [DbContext(typeof(CoffeeContext))]
    [Migration("20250201063540_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.12");

            modelBuilder.Entity("RandomCoffee.Coffee", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("AnnouncedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("GroupId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AnnouncedAt");

                    b.HasIndex("GroupId");

                    b.ToTable("Coffees");
                });

            modelBuilder.Entity("RandomCoffee.Group", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("RandomCoffee.OutBoxMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ChatId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("ParseMode")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("ProcessedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("ReplyToMessageId")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(4096)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("ProcessedAt");

                    b.ToTable("OutBoxMessages");
                });

            modelBuilder.Entity("RandomCoffee.Participant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("CoffeeId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ScheduledAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CoffeeId");

                    b.HasIndex("ScheduledAt");

                    b.HasIndex("UserName");

                    b.ToTable("Participant");
                });

            modelBuilder.Entity("RandomCoffee.Coffee", b =>
                {
                    b.HasOne("RandomCoffee.Group", "Group")
                        .WithMany("Coffees")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("RandomCoffee.Participant", b =>
                {
                    b.HasOne("RandomCoffee.Coffee", "Coffee")
                        .WithMany("CoffeeParticipants")
                        .HasForeignKey("CoffeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coffee");
                });

            modelBuilder.Entity("RandomCoffee.Coffee", b =>
                {
                    b.Navigation("CoffeeParticipants");
                });

            modelBuilder.Entity("RandomCoffee.Group", b =>
                {
                    b.Navigation("Coffees");
                });
#pragma warning restore 612, 618
        }
    }
}
