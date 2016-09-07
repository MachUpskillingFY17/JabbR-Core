using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JabbR_Core.Data.Models
{
    public partial class JabbrContext : DbContext
    {
        public JabbrContext(DbContextOptions<JabbrContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attachments>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_dbo.Attachments");

                entity.HasIndex(e => e.OwnerKey)
                    .HasName("IX_OwnerKey");

                entity.HasIndex(e => e.RoomKey)
                    .HasName("IX_RoomKey");

                entity.Property(e => e.Size).HasDefaultValueSql("0");

                entity.HasOne(d => d.OwnerKeyNavigation)
                    .WithMany(p => p.Attachments)
                    .HasForeignKey(d => d.OwnerKey)
                    .HasConstraintName("FK_dbo.Attachments_dbo.ChatUsers_OwnerKey");

                entity.HasOne(d => d.RoomKeyNavigation)
                    .WithMany(p => p.Attachments)
                    .HasForeignKey(d => d.RoomKey)
                    .HasConstraintName("FK_dbo.Attachments_dbo.ChatRooms_RoomKey");
            });

            modelBuilder.Entity<ChatClients>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_ChatClients");

                entity.Property(e => e.LastActivity).HasDefaultValueSql("'0001-01-01T00:00:00.000+00:00'");

                entity.Property(e => e.LastClientActivity).HasDefaultValueSql("'0001-01-01T00:00:00.000+00:00'");

                entity.Property(e => e.UserKey).HasColumnName("User_Key");

                entity.HasOne(d => d.UserKeyNavigation)
                    .WithMany(p => p.ChatClients)
                    .HasForeignKey(d => d.UserKey)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatMessages>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_ChatMessages");

                entity.Property(e => e.HtmlEncoded).HasDefaultValueSql("1");

                entity.Property(e => e.MessageType).HasDefaultValueSql("0");

                entity.Property(e => e.RoomKey).HasColumnName("Room_Key");

                entity.Property(e => e.UserKey).HasColumnName("User_Key");

                entity.HasOne(d => d.RoomKeyNavigation)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.RoomKey);

                entity.HasOne(d => d.UserKeyNavigation)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.UserKey);
            });

            modelBuilder.Entity<ChatRoomChatUser1>(entity =>
            {
                entity.HasKey(e => new { e.ChatRoomKey, e.ChatUserKey })
                    .HasName("PK_ChatRoomChatUser1");

                entity.Property(e => e.ChatRoomKey).HasColumnName("ChatRoom_Key");

                entity.Property(e => e.ChatUserKey).HasColumnName("ChatUser_Key");

                entity.HasOne(d => d.ChatRoomKeyNavigation)
                    .WithMany(p => p.AllowedUsers)
                    .HasForeignKey(d => d.ChatRoomKey)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ChatUserKeyNavigation)
                    .WithMany(p => p.AllowedRooms)
                    .HasForeignKey(d => d.ChatUserKey)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatRoomChatUsers>(entity =>
            {
                entity.HasKey(e => new { e.ChatRoomKey, e.ChatUserKey })
                    .HasName("PK_ChatRoomChatUsers");

                entity.Property(e => e.ChatRoomKey).HasColumnName("ChatRoom_Key");

                entity.Property(e => e.ChatUserKey).HasColumnName("ChatUser_Key");

                entity.HasOne(d => d.ChatRoomKeyNavigation)
                    .WithMany(p => p.Owners)
                    .HasForeignKey(d => d.ChatRoomKey)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ChatUserKeyNavigation)
                    .WithMany(p => p.OwnedRooms)
                    .HasForeignKey(d => d.ChatUserKey)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatRooms>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_ChatRooms");

                entity.HasIndex(e => e.Name)
                    .HasName("IX_Name")
                    .IsUnique();

                entity.Property(e => e.Closed).HasDefaultValueSql("0");

                entity.Property(e => e.CreatorKey).HasColumnName("Creator_Key");

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
                    .HasForeignKey(d => d.CreatorKey);
            });

            modelBuilder.Entity<ChatUserChatRooms>(entity =>
            {
                entity.HasKey(e => new { e.ChatUserKey, e.ChatRoomKey })
                    .HasName("PK_ChatUserChatRooms");

                entity.Property(e => e.ChatUserKey).HasColumnName("ChatUser_Key");

                entity.Property(e => e.ChatRoomKey).HasColumnName("ChatRoom_Key");

                entity.HasOne(d => d.ChatRoomKeyNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.ChatRoomKey)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ChatUserKeyNavigation)
                    .WithMany(p => p.Rooms)
                    .HasForeignKey(d => d.ChatUserKey)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatUserIdentities>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_dbo.ChatUserIdentities");

                entity.HasIndex(e => e.UserKey)
                    .HasName("IX_UserKey");

                entity.HasOne(d => d.UserKeyNavigation)
                    .WithMany(p => p.ChatUserIdentities)
                    .HasForeignKey(d => d.UserKey)
                    .HasConstraintName("FK_dbo.ChatUserIdentities_dbo.ChatUsers_UserKey");
            });

            modelBuilder.Entity<ChatUsers>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_ChatUsers");

                entity.HasIndex(e => e.Id)
                    .HasName("IX_Id")
                    .IsUnique();

                entity.Property(e => e.AfkNote).HasMaxLength(200);

                entity.Property(e => e.Flag).HasMaxLength(255);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasMaxLength(200);

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

            modelBuilder.Entity<Notifications>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_dbo.Notifications");

                entity.HasIndex(e => e.MessageKey)
                    .HasName("IX_MessageKey");

                entity.HasIndex(e => e.RoomKey)
                    .HasName("IX_RoomKey");

                entity.HasIndex(e => e.UserKey)
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
                    .HasForeignKey(d => d.UserKey)
                    .HasConstraintName("FK_dbo.Notifications_dbo.ChatUsers_UserKey");
            });

            modelBuilder.Entity<Settings>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_dbo.Settings");
            });
        }

        public DbSet<Attachments> Attachments { get; set; }
        public DbSet<ChatClients> ChatClients { get; set; }
        public DbSet<ChatMessages> ChatMessages { get; set; }
        public DbSet<ChatRoomChatUser1> ChatRoomChatUser1 { get; set; }
        public DbSet<ChatRoomChatUsers> ChatRoomChatUsers { get; set; }
        public DbSet<ChatRooms> ChatRooms { get; set; }
        public DbSet<ChatUserChatRooms> ChatUserChatRooms { get; set; }
        public DbSet<ChatUserIdentities> ChatUserIdentities { get; set; }
        public DbSet<ChatUsers> ChatUsers { get; set; }
        public DbSet<MigrationHistory> MigrationHistory { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Settings> Settings { get; set; }
    }
}