using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JabbR_Core.Data.Models;

namespace JabbRCore.Data.Migrations
{
    [DbContext(typeof(JabbrContext))]
    [Migration("20161014002717_IntegrateIdentity")]
    partial class IntegrateIdentity
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-alpha1-22397")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("JabbR_Core.Data.Models.Attachment", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContentType");

                    b.Property<string>("FileName");

                    b.Property<string>("Id");

                    b.Property<string>("OwnerId");

                    b.Property<int>("RoomKey");

                    b.Property<long>("Size")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<string>("Url");

                    b.Property<DateTimeOffset>("When");

                    b.HasKey("Key")
                        .HasName("PK_dbo.Attachments");

                    b.HasIndex("OwnerId")
                        .HasName("IX_OwnerKey");

                    b.HasIndex("RoomKey")
                        .HasName("IX_RoomKey");

                    b.ToTable("Attachments");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatClient", b =>
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

                    b.Property<string>("UserId")
                        .HasColumnName("User_Key");

                    b.HasKey("Key")
                        .HasName("PK_ChatClients");

                    b.HasIndex("UserId");

                    b.ToTable("ChatClients");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatMessage", b =>
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

                    b.Property<string>("UserId")
                        .HasColumnName("User_Key");

                    b.Property<DateTimeOffset>("When");

                    b.HasKey("Key")
                        .HasName("PK_ChatMessages");

                    b.HasIndex("RoomKey");

                    b.HasIndex("UserId");

                    b.ToTable("ChatMessages");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatPrivateRoomUsers", b =>
                {
                    b.Property<int>("ChatRoomKey")
                        .HasColumnName("ChatRoom_Key");

                    b.Property<string>("ChatUserId")
                        .HasColumnName("ChatUser_Key");

                    b.HasKey("ChatRoomKey", "ChatUserId")
                        .HasName("PK_ChatPrivateRoomUsers");

                    b.HasIndex("ChatUserId");

                    b.ToTable("ChatPrivateRoomUsers");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatRoom", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Closed")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<string>("CreatorId");

                    b.Property<string>("InviteCode")
                        .HasColumnType("nchar(6)");

                    b.Property<DateTime?>("LastNudged")
                        .HasColumnType("datetime");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<bool>("Private")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<string>("Topic")
                        .HasMaxLength(80);

                    b.Property<string>("Welcome")
                        .HasMaxLength(200);

                    b.HasKey("Key")
                        .HasName("PK_ChatRooms");

                    b.HasIndex("CreatorId");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasName("IX_Name");

                    b.ToTable("ChatRooms");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatRoomOwners", b =>
                {
                    b.Property<int>("ChatRoomKey")
                        .HasColumnName("ChatRoom_Key");

                    b.Property<string>("ChatUserId")
                        .HasColumnName("ChatUser_Key");

                    b.HasKey("ChatRoomKey", "ChatUserId")
                        .HasName("PK_ChatRoomOwners");

                    b.HasIndex("ChatUserId");

                    b.ToTable("ChatRoomOwners");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatRoomUsers", b =>
                {
                    b.Property<string>("ChatUserId")
                        .HasColumnName("ChatUser_Key");

                    b.Property<int>("ChatRoomKey")
                        .HasColumnName("ChatRoom_Key");

                    b.HasKey("ChatUserId", "ChatRoomKey")
                        .HasName("PK_ChatRoomUsers");

                    b.HasIndex("ChatRoomKey");

                    b.ToTable("ChatRoomUsers");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("AfkNote")
                        .HasMaxLength(200);

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("Flag")
                        .HasMaxLength(255);

                    b.Property<string>("Hash");

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

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("Name");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("Note")
                        .HasMaxLength(200);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("RawPreferences");

                    b.Property<string>("RequestPasswordResetId");

                    b.Property<DateTimeOffset?>("RequestPasswordResetValidThrough");

                    b.Property<string>("SecurityStamp");

                    b.Property<int>("Status")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id")
                        .HasName("PK_Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatUserIdentity", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<string>("Identity");

                    b.Property<string>("ProviderName");

                    b.Property<string>("UserId");

                    b.HasKey("Key")
                        .HasName("PK_dbo.ChatUserIdentities");

                    b.HasIndex("UserId")
                        .HasName("IX_UserKey");

                    b.ToTable("ChatUserIdentities");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.MigrationHistory", b =>
                {
                    b.Property<string>("MigrationId")
                        .HasMaxLength(150);

                    b.Property<string>("ContextKey")
                        .HasMaxLength(300);

                    b.Property<byte[]>("Model")
                        .IsRequired();

                    b.Property<string>("ProductVersion")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.HasKey("MigrationId", "ContextKey")
                        .HasName("PK_dbo.__MigrationHistory");

                    b.ToTable("__MigrationHistory");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.Notification", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("MessageKey");

                    b.Property<bool>("Read");

                    b.Property<int>("RoomKey");

                    b.Property<string>("UserId");

                    b.HasKey("Key")
                        .HasName("PK_dbo.Notifications");

                    b.HasIndex("MessageKey")
                        .HasName("IX_MessageKey");

                    b.HasIndex("RoomKey")
                        .HasName("IX_RoomKey");

                    b.HasIndex("UserId")
                        .HasName("IX_UserKey");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.Settings", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("RawSettings");

                    b.HasKey("Key")
                        .HasName("PK_dbo.Settings");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.Attachment", b =>
                {
                    b.HasOne("JabbR_Core.Data.Models.ChatUser", "OwnerKeyNavigation")
                        .WithMany("Attachments")
                        .HasForeignKey("OwnerId")
                        .HasConstraintName("FK_dbo.Attachments_dbo.ChatUsers_OwnerKey");

                    b.HasOne("JabbR_Core.Data.Models.ChatRoom", "RoomKeyNavigation")
                        .WithMany("Attachments")
                        .HasForeignKey("RoomKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatClient", b =>
                {
                    b.HasOne("JabbR_Core.Data.Models.ChatUser", "UserKeyNavigation")
                        .WithMany("ConnectedClients")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatMessage", b =>
                {
                    b.HasOne("JabbR_Core.Data.Models.ChatRoom", "RoomKeyNavigation")
                        .WithMany("ChatMessages")
                        .HasForeignKey("RoomKey");

                    b.HasOne("JabbR_Core.Data.Models.ChatUser", "UserKeyNavigation")
                        .WithMany("ChatMessages")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatPrivateRoomUsers", b =>
                {
                    b.HasOne("JabbR_Core.Data.Models.ChatRoom", "ChatRoomKeyNavigation")
                        .WithMany("AllowedUsers")
                        .HasForeignKey("ChatRoomKey");

                    b.HasOne("JabbR_Core.Data.Models.ChatUser", "ChatUserKeyNavigation")
                        .WithMany("AllowedRooms")
                        .HasForeignKey("ChatUserId");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatRoom", b =>
                {
                    b.HasOne("JabbR_Core.Data.Models.ChatUser", "CreatorKeyNavigation")
                        .WithMany("ChatRooms")
                        .HasForeignKey("CreatorId");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatRoomOwners", b =>
                {
                    b.HasOne("JabbR_Core.Data.Models.ChatRoom", "ChatRoomKeyNavigation")
                        .WithMany("Owners")
                        .HasForeignKey("ChatRoomKey");

                    b.HasOne("JabbR_Core.Data.Models.ChatUser", "ChatUserKeyNavigation")
                        .WithMany("OwnedRooms")
                        .HasForeignKey("ChatUserId");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatRoomUsers", b =>
                {
                    b.HasOne("JabbR_Core.Data.Models.ChatRoom", "ChatRoomKeyNavigation")
                        .WithMany("Users")
                        .HasForeignKey("ChatRoomKey");

                    b.HasOne("JabbR_Core.Data.Models.ChatUser", "ChatUserKeyNavigation")
                        .WithMany("Rooms")
                        .HasForeignKey("ChatUserId");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.ChatUserIdentity", b =>
                {
                    b.HasOne("JabbR_Core.Data.Models.ChatUser", "UserKeyNavigation")
                        .WithMany("ChatUserIdentities")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_dbo.ChatUserIdentities_dbo.ChatUsers_UserKey");
                });

            modelBuilder.Entity("JabbR_Core.Data.Models.Notification", b =>
                {
                    b.HasOne("JabbR_Core.Data.Models.ChatMessage", "MessageKeyNavigation")
                        .WithMany("Notifications")
                        .HasForeignKey("MessageKey")
                        .HasConstraintName("FK_dbo.Notifications_dbo.ChatMessages_MessageKey")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("JabbR_Core.Data.Models.ChatRoom", "RoomKeyNavigation")
                        .WithMany("Notifications")
                        .HasForeignKey("RoomKey")
                        .HasConstraintName("FK_dbo.Notifications_dbo.ChatRooms_RoomKey")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("JabbR_Core.Data.Models.ChatUser", "UserKeyNavigation")
                        .WithMany("Notifications")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_dbo.Notifications_dbo.ChatUsers_UserKey");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("JabbR_Core.Data.Models.ChatUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("JabbR_Core.Data.Models.ChatUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("JabbR_Core.Data.Models.ChatUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
