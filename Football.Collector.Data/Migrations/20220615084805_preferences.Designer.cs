﻿// <auto-generated />
using System;
using Football.Collector.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Football.Collector.Data.Migrations
{
    [DbContext(typeof(FootballCollectorDbContext))]
    [Migration("20220615084805_preferences")]
    partial class preferences
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Football.Collector.Data.Models.ServiceUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)")
                        .HasDefaultValueSql("newid()");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Username");

                    b.ToTable("ServiceUsers", (string)null);
                });

            modelBuilder.Entity("Football.Collector.Data.Models.TelegramChatUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("bit");

                    b.Property<string>("TelegramChatId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TelegramUserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("TelegramUserId");

                    b.ToTable("TelegramChatUsers", (string)null);
                });

            modelBuilder.Entity("Football.Collector.Data.Models.TelegramGame", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)")
                        .HasDefaultValueSql("newid()");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ChatId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Cost")
                        .HasColumnType("float");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("DurationInMins")
                        .HasColumnType("int");

                    b.Property<bool>("HasChangingRoom")
                        .HasColumnType("bit");

                    b.Property<bool>("HasParking")
                        .HasColumnType("bit");

                    b.Property<bool>("HasShower")
                        .HasColumnType("bit");

                    b.Property<string>("MessageId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("TelegramGames", (string)null);
                });

            modelBuilder.Entity("Football.Collector.Data.Models.TelegramGamePlayer", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)")
                        .HasDefaultValueSql("newid()");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("TelegramGameId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("TelegramUserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("TelegramUserId");

                    b.HasIndex("TelegramGameId", "TelegramUserId");

                    b.ToTable("TelegramGamePlayers", (string)null);
                });

            modelBuilder.Entity("Football.Collector.Data.Models.TelegramUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)")
                        .HasDefaultValueSql("newid()");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TelegramId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("TelegramId");

                    b.ToTable("TelegramUsers", (string)null);
                });

            modelBuilder.Entity("Football.Collector.Data.Models.TelegramChatUser", b =>
                {
                    b.HasOne("Football.Collector.Data.Models.TelegramUser", "TelegramUser")
                        .WithMany()
                        .HasForeignKey("TelegramUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TelegramUser");
                });

            modelBuilder.Entity("Football.Collector.Data.Models.TelegramGamePlayer", b =>
                {
                    b.HasOne("Football.Collector.Data.Models.TelegramGame", "TelegramGame")
                        .WithMany("TelegramGamePlayers")
                        .HasForeignKey("TelegramGameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Football.Collector.Data.Models.TelegramUser", "TelegramUser")
                        .WithMany()
                        .HasForeignKey("TelegramUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TelegramGame");

                    b.Navigation("TelegramUser");
                });

            modelBuilder.Entity("Football.Collector.Data.Models.TelegramGame", b =>
                {
                    b.Navigation("TelegramGamePlayers");
                });
#pragma warning restore 612, 618
        }
    }
}
