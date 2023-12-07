﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OrganixMessenger.ServerData;

#nullable disable

namespace OrganixMessenger.ServerData.Migrations
{
    [DbContext(typeof(ApplicationDBContext))]
    [Migration("20231207174529_for-bot-message-tweaks")]
    partial class forbotmessagetweaks
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("OrganixMessenger.ServerModels.BotCommandModel.BotCommand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Guid>("BotId")
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(24)
                        .HasColumnType("character varying(24)");

                    b.Property<string>("Trigger")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("BotId");

                    b.ToTable("Commands");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.FileModel.UploadedFile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("MessageId")
                        .HasColumnType("integer");

                    b.Property<string>("StoredFileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MessageId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.MessageModel.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Guid?>("CustomProfilePictureId")
                        .HasColumnType("uuid");

                    b.Property<string>("CustomUsername")
                        .HasColumnType("text");

                    b.Property<bool>("Edited")
                        .HasColumnType("boolean");

                    b.Property<int?>("MessageReplyId")
                        .HasColumnType("integer");

                    b.Property<bool>("Removed")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("SendTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("SenderId")
                        .HasColumnType("uuid");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CustomProfilePictureId");

                    b.HasIndex("MessageReplyId");

                    b.HasIndex("SenderId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.MessengerEntityModels.MessengerEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("LastOnline")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("ProfilePictureId")
                        .HasColumnType("uuid");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ProfilePictureId");

                    b.ToTable("MessengerEntities");

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.RefreshTokenModel.RefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("IssuedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("JWTId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationBotModel.ApplicationBot", b =>
                {
                    b.HasBaseType("OrganixMessenger.ServerModels.MessengerEntityModels.MessengerEntity");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uuid");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasIndex("OwnerId");

                    b.ToTable("Bots");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel.ApplicationUser", b =>
                {
                    b.HasBaseType("OrganixMessenger.ServerModels.MessengerEntityModels.MessengerEntity");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordResetToken")
                        .HasColumnType("text");

                    b.Property<DateTime?>("PasswordResetTokenExpires")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<string>("VereficationToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.BotCommandModel.BotCommand", b =>
                {
                    b.HasOne("OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationBotModel.ApplicationBot", "Bot")
                        .WithMany("Commands")
                        .HasForeignKey("BotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bot");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.FileModel.UploadedFile", b =>
                {
                    b.HasOne("OrganixMessenger.ServerModels.MessageModel.Message", null)
                        .WithMany("Files")
                        .HasForeignKey("MessageId");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.MessageModel.Message", b =>
                {
                    b.HasOne("OrganixMessenger.ServerModels.FileModel.UploadedFile", "CustomProfilePicture")
                        .WithMany()
                        .HasForeignKey("CustomProfilePictureId");

                    b.HasOne("OrganixMessenger.ServerModels.MessageModel.Message", "MessageReply")
                        .WithMany()
                        .HasForeignKey("MessageReplyId");

                    b.HasOne("OrganixMessenger.ServerModels.MessengerEntityModels.MessengerEntity", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CustomProfilePicture");

                    b.Navigation("MessageReply");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.MessengerEntityModels.MessengerEntity", b =>
                {
                    b.HasOne("OrganixMessenger.ServerModels.FileModel.UploadedFile", "ProfilePicture")
                        .WithMany()
                        .HasForeignKey("ProfilePictureId");

                    b.Navigation("ProfilePicture");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.RefreshTokenModel.RefreshToken", b =>
                {
                    b.HasOne("OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationBotModel.ApplicationBot", b =>
                {
                    b.HasOne("OrganixMessenger.ServerModels.MessengerEntityModels.MessengerEntity", null)
                        .WithOne()
                        .HasForeignKey("OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationBotModel.ApplicationBot", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel.ApplicationUser", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel.ApplicationUser", b =>
                {
                    b.HasOne("OrganixMessenger.ServerModels.MessengerEntityModels.MessengerEntity", null)
                        .WithOne()
                        .HasForeignKey("OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel.ApplicationUser", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.MessageModel.Message", b =>
                {
                    b.Navigation("Files");
                });

            modelBuilder.Entity("OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationBotModel.ApplicationBot", b =>
                {
                    b.Navigation("Commands");
                });
#pragma warning restore 612, 618
        }
    }
}
