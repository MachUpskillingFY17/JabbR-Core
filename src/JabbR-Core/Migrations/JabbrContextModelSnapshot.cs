using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JabbR_Core.Models;

namespace JabbRCore.Migrations
{
    [DbContext(typeof(JabbrContext))]
    partial class JabbrContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-alpha1-22028")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("JabbR_Core.Models.Attachments", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContentType");

                    b.Property<string>("FileName");

                    b.Property<string>("Id");

                    b.Property<int>("OwnerKey");

                    b.Property<int>("RoomKey");

                    b.Property<long>("Size")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<string>("Url");

                    b.Property<DateTimeOffset>("When");

                    b.HasKey("Key")
                        .HasName("PK_dbo.Attachments");

                    b.HasIndex("OwnerKey")
                        .HasName("IX_OwnerKey");

                    b.HasIndex("RoomKey")
                        .HasName("IX_RoomKey");

                    b.ToTable("Attachments");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatClients", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Id");

                    b.Property<DateTimeOffset>("LastActivity")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("'0001-01-01T00:00:00.000+00:00'");

                    b.Property<DateTimeOffset>("LastClientActivity")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("'0001-01-01T00:00:00.000+00:00'");

                    b.Property<string>("Name");

                    b.Property<string>("UserAgent");

                    b.Property<int>("UserKey")
                        .HasColumnName("User_Key");

                    b.HasKey("Key")
                        .HasName("PK_ChatClients");

                    b.HasIndex("UserKey");

                    b.ToTable("ChatClients");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatMessages", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<string>("HtmlContent");

                    b.Property<bool>("HtmlEncoded")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("1");

                    b.Property<string>("Id");

                    b.Property<string>("ImageUrl");

                    b.Property<int>("MessageType")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<int?>("RoomKey")
                        .HasColumnName("Room_Key");

                    b.Property<string>("Source");

                    b.Property<int?>("UserKey")
                        .HasColumnName("User_Key");

                    b.Property<DateTimeOffset>("When");

                    b.HasKey("Key")
                        .HasName("PK_ChatMessages");

                    b.HasIndex("RoomKey");

                    b.HasIndex("UserKey");

                    b.ToTable("ChatMessages");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatRoomChatUser1", b =>
                {
                    b.Property<int>("ChatRoomKey")
                        .HasColumnName("ChatRoom_Key");

                    b.Property<int>("ChatUserKey")
                        .HasColumnName("ChatUser_Key");

                    b.HasKey("ChatRoomKey", "ChatUserKey")
                        .HasName("PK_ChatRoomChatUser1");

                    b.HasIndex("ChatUserKey");

                    b.ToTable("ChatRoomChatUser1");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatRoomChatUsers", b =>
                {
                    b.Property<int>("ChatRoomKey")
                        .HasColumnName("ChatRoom_Key");

                    b.Property<int>("ChatUserKey")
                        .HasColumnName("ChatUser_Key");

                    b.HasKey("ChatRoomKey", "ChatUserKey")
                        .HasName("PK_ChatRoomChatUsers");

                    b.HasIndex("ChatUserKey");

                    b.ToTable("ChatRoomChatUsers");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatRooms", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Closed")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<int?>("CreatorKey")
                        .HasColumnName("Creator_Key");

                    b.Property<string>("InviteCode")
                        .HasColumnType("nchar(6)");

                    b.Property<DateTime?>("LastNudged")
                        .HasColumnType("datetime");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.Property<bool>("Private")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<string>("Topic")
                        .HasAnnotation("MaxLength", 80);

                    b.Property<string>("Welcome")
                        .HasAnnotation("MaxLength", 200);

                    b.HasKey("Key")
                        .HasName("PK_ChatRooms");

                    b.HasIndex("CreatorKey");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasName("IX_Name");

                    b.ToTable("ChatRooms");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatUserChatRooms", b =>
                {
                    b.Property<int>("ChatUserKey")
                        .HasColumnName("ChatUser_Key");

                    b.Property<int>("ChatRoomKey")
                        .HasColumnName("ChatRoom_Key");

                    b.HasKey("ChatUserKey", "ChatRoomKey")
                        .HasName("PK_ChatUserChatRooms");

                    b.HasIndex("ChatRoomKey");

                    b.ToTable("ChatUserChatRooms");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatUserIdentities", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<string>("Identity");

                    b.Property<string>("ProviderName");

                    b.Property<int>("UserKey");

                    b.HasKey("Key")
                        .HasName("PK_dbo.ChatUserIdentities");

                    b.HasIndex("UserKey")
                        .HasName("IX_UserKey");

                    b.ToTable("ChatUserIdentities");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatUsers", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AfkNote")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<string>("Email");

                    b.Property<string>("Flag")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("Hash");

                    b.Property<string>("HashedPassword");

                    b.Property<string>("Id")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.Property<string>("Identity");

                    b.Property<bool>("IsAdmin")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<bool>("IsAfk")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<bool>("IsBanned")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("LastNudged")
                        .HasColumnType("datetime");

                    b.Property<string>("Name");

                    b.Property<string>("Note")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<string>("RawPreferences");

                    b.Property<string>("RequestPasswordResetId");

                    b.Property<DateTimeOffset?>("RequestPasswordResetValidThrough");

                    b.Property<string>("Salt");

                    b.Property<int>("Status")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.HasKey("Key")
                        .HasName("PK_ChatUsers");

                    b.HasIndex("Id")
                        .IsUnique()
                        .HasName("IX_Id");

                    b.ToTable("ChatUsers");
                });

            modelBuilder.Entity("JabbR_Core.Models.MigrationHistory", b =>
                {
                    b.Property<string>("MigrationId")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<string>("ContextKey")
                        .HasAnnotation("MaxLength", 300);

                    b.Property<byte[]>("Model")
                        .IsRequired();

                    b.Property<string>("ProductVersion")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 32);

                    b.HasKey("MigrationId", "ContextKey")
                        .HasName("PK_dbo.__MigrationHistory");

                    b.ToTable("__MigrationHistory");
                });

            modelBuilder.Entity("JabbR_Core.Models.Notifications", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("MessageKey");

                    b.Property<bool>("Read");

                    b.Property<int>("RoomKey");

                    b.Property<int>("UserKey");

                    b.HasKey("Key")
                        .HasName("PK_dbo.Notifications");

                    b.HasIndex("MessageKey")
                        .HasName("IX_MessageKey");

                    b.HasIndex("RoomKey")
                        .HasName("IX_RoomKey");

                    b.HasIndex("UserKey")
                        .HasName("IX_UserKey");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("JabbR_Core.Models.Settings", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("RawSettings");

                    b.HasKey("Key")
                        .HasName("PK_dbo.Settings");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("JabbR_Core.Models.Attachments", b =>
                {
                    b.HasOne("JabbR_Core.Models.ChatUsers", "OwnerKeyNavigation")
                        .WithMany("Attachments")
                        .HasForeignKey("OwnerKey")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("JabbR_Core.Models.ChatRooms", "RoomKeyNavigation")
                        .WithMany("Attachments")
                        .HasForeignKey("RoomKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatClients", b =>
                {
                    b.HasOne("JabbR_Core.Models.ChatUsers", "UserKeyNavigation")
                        .WithMany("ChatClients")
                        .HasForeignKey("UserKey");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatMessages", b =>
                {
                    b.HasOne("JabbR_Core.Models.ChatRooms", "RoomKeyNavigation")
                        .WithMany("ChatMessages")
                        .HasForeignKey("RoomKey");

                    b.HasOne("JabbR_Core.Models.ChatUsers", "UserKeyNavigation")
                        .WithMany("ChatMessages")
                        .HasForeignKey("UserKey");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatRoomChatUser1", b =>
                {
                    b.HasOne("JabbR_Core.Models.ChatRooms", "ChatRoomKeyNavigation")
                        .WithMany("AllowedUsers")
                        .HasForeignKey("ChatRoomKey");

                    b.HasOne("JabbR_Core.Models.ChatUsers", "ChatUserKeyNavigation")
                        .WithMany("AllowedRooms")
                        .HasForeignKey("ChatUserKey");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatRoomChatUsers", b =>
                {
                    b.HasOne("JabbR_Core.Models.ChatRooms", "ChatRoomKeyNavigation")
                        .WithMany("Owners")
                        .HasForeignKey("ChatRoomKey");

                    b.HasOne("JabbR_Core.Models.ChatUsers", "ChatUserKeyNavigation")
                        .WithMany("OwnedRooms")
                        .HasForeignKey("ChatUserKey");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatRooms", b =>
                {
                    b.HasOne("JabbR_Core.Models.ChatUsers", "CreatorKeyNavigation")
                        .WithMany("ChatRooms")
                        .HasForeignKey("CreatorKey");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatUserChatRooms", b =>
                {
                    b.HasOne("JabbR_Core.Models.ChatRooms", "ChatRoomKeyNavigation")
                        .WithMany("Users")
                        .HasForeignKey("ChatRoomKey");

                    b.HasOne("JabbR_Core.Models.ChatUsers", "ChatUserKeyNavigation")
                        .WithMany("Rooms")
                        .HasForeignKey("ChatUserKey");
                });

            modelBuilder.Entity("JabbR_Core.Models.ChatUserIdentities", b =>
                {
                    b.HasOne("JabbR_Core.Models.ChatUsers", "UserKeyNavigation")
                        .WithMany("ChatUserIdentities")
                        .HasForeignKey("UserKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("JabbR_Core.Models.Notifications", b =>
                {
                    b.HasOne("JabbR_Core.Models.ChatMessages", "MessageKeyNavigation")
                        .WithMany("Notifications")
                        .HasForeignKey("MessageKey")
                        .HasConstraintName("FK_dbo.Notifications_dbo.ChatMessages_MessageKey")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("JabbR_Core.Models.ChatRooms", "RoomKeyNavigation")
                        .WithMany("Notifications")
                        .HasForeignKey("RoomKey")
                        .HasConstraintName("FK_dbo.Notifications_dbo.ChatRooms_RoomKey")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("JabbR_Core.Models.ChatUsers", "UserKeyNavigation")
                        .WithMany("Notifications")
                        .HasForeignKey("UserKey")
                        .HasConstraintName("FK_dbo.Notifications_dbo.ChatUsers_UserKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
