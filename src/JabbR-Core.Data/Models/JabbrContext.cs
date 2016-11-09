using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace JabbR_Core.Data.Models
{
    public partial class JabbrContext : IdentityDbContext<ChatUser>
    {
        public JabbrContext(DbContextOptions<JabbrContext> options) : base(options)
        {
            Console.WriteLine($"Created DbContext hash:{GetHashCode()}");
        }

        public override void Dispose()
        {
            Console.WriteLine("Disposed JabbrContext " + GetHashCode());
            base.Dispose();
            Console.WriteLine("Completed base.Dispose on JabbrContext " + GetHashCode());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_dbo.Attachments");

                entity.HasIndex(e => e.OwnerId)
                    .HasName("IX_OwnerKey");

                entity.HasIndex(e => e.RoomKey)
                    .HasName("IX_RoomKey");

                entity.Property(e => e.Size).HasDefaultValueSql("0");

                entity.HasOne(d => d.OwnerKeyNavigation)
                    .WithMany(p => p.Attachments)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("FK_dbo.Attachments_dbo.ChatUsers_OwnerKey");

                entity.HasOne(d => d.RoomKeyNavigation)
                    .WithMany(p => p.Attachments)
                    .HasForeignKey(d => d.RoomKey)
                    .HasConstraintName("FK_dbo.Attachments_dbo.ChatRooms_RoomKey");
            });

            modelBuilder.Entity<ChatClient>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_ChatClients");

                entity.Property(e => e.LastActivity).HasDefaultValueSql("'0001-01-01T00:00:00.000+00:00'");

                entity.Property(e => e.LastClientActivity).HasDefaultValueSql("'0001-01-01T00:00:00.000+00:00'");

                entity.Property(e => e.UserId).HasColumnName("User_Key");

                entity.HasOne(d => d.UserKeyNavigation)
                    .WithMany(p => p.ConnectedClients)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_ChatMessages");

                entity.Property(e => e.HtmlEncoded).HasDefaultValueSql("1");

                entity.Property(e => e.MessageType).HasDefaultValueSql("0");

                entity.Property(e => e.RoomKey).HasColumnName("Room_Key");

                entity.Property(e => e.UserId).HasColumnName("User_Key");

                entity.HasOne(d => d.RoomKeyNavigation)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.RoomKey);

                entity.HasOne(d => d.UserKeyNavigation)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<ChatPrivateRoomUsers>(entity =>
            {
                entity.HasKey(e => new { e.ChatRoomKey, e.ChatUserId })
                    .HasName("PK_ChatPrivateRoomUsers");

                entity.Property(e => e.ChatRoomKey).HasColumnName("ChatRoom_Key");

                entity.Property(e => e.ChatUserId).HasColumnName("ChatUser_Key");

                entity.HasOne(d => d.ChatRoomKeyNavigation)
                    .WithMany(p => p.AllowedUsers)
                    .HasForeignKey(d => d.ChatRoomKey)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ChatUserKeyNavigation)
                    .WithMany(p => p.AllowedRooms)
                    .HasForeignKey(d => d.ChatUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatRoomOwners>(entity =>
            {
                entity.HasKey(e => new { e.ChatRoomKey, e.ChatUserId })
                    .HasName("PK_ChatRoomOwners");

                entity.Property(e => e.ChatRoomKey).HasColumnName("ChatRoom_Key");

                entity.Property(e => e.ChatUserId).HasColumnName("ChatUser_Key");

                entity.HasOne(d => d.ChatRoomKeyNavigation)
                    .WithMany(p => p.Owners)
                    .HasForeignKey(d => d.ChatRoomKey)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ChatUserKeyNavigation)
                    .WithMany(p => p.OwnedRooms)
                    .HasForeignKey(d => d.ChatUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_ChatRooms");

                entity.HasIndex(e => e.Name)
                    .HasName("IX_Name")
                    .IsUnique();

                entity.Property(e => e.Closed).HasDefaultValueSql("0");

                entity.Property(e => e.CreatorId);

                entity.Property(e => e.InviteCode).HasColumnType("nchar(6)");

                entity.Property(e => e.LastNudged).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Private).HasDefaultValueSql("0");

                entity.Property(e => e.Topic).HasMaxLength(80);

                entity.Property(e => e.Welcome).HasMaxLength(200);

                entity.HasOne(d => d.CreatorKeyNavigation)
                    .WithMany(p => p.ChatRooms)
                    .HasForeignKey(d => d.CreatorId);
            });

            modelBuilder.Entity<ChatRoomUsers>(entity =>
            {
                entity.HasKey(e => new { e.ChatUserId, e.ChatRoomKey })
                    .HasName("PK_ChatRoomUsers");

                entity.Property(e => e.ChatUserId).HasColumnName("ChatUser_Key");

                entity.Property(e => e.ChatRoomKey).HasColumnName("ChatRoom_Key");

                entity.HasOne(d => d.ChatRoomKeyNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.ChatRoomKey)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ChatUserKeyNavigation)
                    .WithMany(p => p.Rooms)
                    .HasForeignKey(d => d.ChatUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatUserIdentity>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_dbo.ChatUserIdentities");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserKey");

                entity.HasOne(d => d.UserKeyNavigation)
                    .WithMany(p => p.ChatUserIdentities)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.ChatUserIdentities_dbo.ChatUsers_UserKey");
            });

            modelBuilder.Entity<ChatUser>(entity =>
            {
                //entity.HasKey(e => e.Key)
                //    .HasName("PK_ChatUsers");

                //entity.HasIndex(e => e.Id)
                //    .HasName("IX_Id")
                //    .IsUnique();

                entity.HasKey(e => e.Id)
                    .HasName("PK_Id");

                entity.Property(e => e.AfkNote).HasMaxLength(200);

                entity.Property(e => e.Flag).HasMaxLength(255);

                //entity.Property(e => e.Id)
                //    .IsRequired()
                //    .HasMaxLength(200);

                entity.Property(e => e.IsAdmin).HasDefaultValueSql("0");

                entity.Property(e => e.IsAfk).HasDefaultValueSql("0");

                entity.Property(e => e.IsBanned).HasDefaultValueSql("0");

                entity.Property(e => e.LastActivity).HasColumnType("datetime");

                entity.Property(e => e.LastNudged).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(200);

                entity.Property(e => e.Status).HasDefaultValueSql("0");
            });

            modelBuilder.Entity<MigrationHistory>(entity =>
            {
                entity.HasKey(e => new { e.MigrationId, e.ContextKey })
                    .HasName("PK_dbo.__MigrationHistory");

                entity.ToTable("__MigrationHistory");

                entity.Property(e => e.MigrationId).HasMaxLength(150);

                entity.Property(e => e.ContextKey).HasMaxLength(300);

                entity.Property(e => e.Model).IsRequired();

                entity.Property(e => e.ProductVersion)
                    .IsRequired()
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_dbo.Notifications");

                entity.HasIndex(e => e.MessageKey)
                    .HasName("IX_MessageKey");

                entity.HasIndex(e => e.RoomKey)
                    .HasName("IX_RoomKey");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserKey");

                entity.HasOne(d => d.MessageKeyNavigation)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.MessageKey)
                    .HasConstraintName("FK_dbo.Notifications_dbo.ChatMessages_MessageKey");

                entity.HasOne(d => d.RoomKeyNavigation)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.RoomKey)
                    .HasConstraintName("FK_dbo.Notifications_dbo.ChatRooms_RoomKey");

                entity.HasOne(d => d.UserKeyNavigation)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.Notifications_dbo.ChatUsers_UserKey");
            });

            modelBuilder.Entity<Settings>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_dbo.Settings");
            });
        }

        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<ChatClient> ChatClients { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatPrivateRoomUsers> ChatPrivateRoomUsers { get; set; }
        public DbSet<ChatRoomOwners> ChatRoomOwners { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatRoomUsers> ChatRoomUsers { get; set; }
        public DbSet<ChatUserIdentity> ChatUserIdentities { get; set; }
        public DbSet<ChatUser> AspNetUsers { get; set; }
        public DbSet<MigrationHistory> MigrationHistory { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Settings> Settings { get; set; }
    }
}