using Microsoft.EntityFrameworkCore;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Jmgram_mk1.src.JMgram.Core.Storage
{
    public class JMgramDbContext : IdentityDbContext<AppIdentityUser>
    {
        public new DbSet<AppIdentityUser> Users { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Contact> Contacts { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MessageNotification> MessageNotifications { get; set; }
        public DbSet<ContactRequestNotification> ContactRequestNotifications { get; set; }
        public DbSet<SystemNotification> SystemNotifications { get; set; }
        public DbSet<ChatInviteNotification> ChatInviteNotifications { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public DbSet<ChatUser> ChatUsers { get; set; } = null!;
        public DbSet<ContactRequest> ContactRequests { get; set; } = null!;
        public JMgramDbContext(DbContextOptions<JMgramDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserProfile>()
             .HasKey(up => up.UserId);

            modelBuilder.Entity<AppIdentityUser>()
             .HasOne(u => u.UserProfile)
             .WithOne(p => p.User)
             .HasForeignKey<UserProfile>(p => p.UserId);

            modelBuilder.Entity<Contact>()
             .HasOne(c => c.User)
             .WithMany(u => u.Contacts)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contact>()
             .HasOne(c => c.ContactUser)
             .WithMany()
             .HasForeignKey(c => c.ContactUserId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd()
                .HasMaxLength(100);

            modelBuilder.Entity<ChatUser>()
                .HasKey(cu => new { cu.ChatId, cu.UserId });

            modelBuilder.Entity<ChatUser>()
                .Property(cu => cu.ChatId)
                .HasMaxLength(100);

            modelBuilder.Entity<ChatUser>()
                .Property(cu => cu.UserId)
                .HasMaxLength(450);

            modelBuilder.Entity<ChatUser>()
                .Property(cu => cu.JoinedAt)
                .HasColumnType("datetime2");

            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.Chat)
                .WithMany(c => c.ChatUsers)
                .HasForeignKey(cu => cu.ChatId);

            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.User)
                .WithMany()
                .HasForeignKey(cu => cu.UserId);

            modelBuilder.Entity<Message>()
             .HasOne(m => m.Chat)
             .WithMany()
             .HasForeignKey(m => m.ChatId);

            modelBuilder.Entity<Message>()
             .Property(m => m.Id)
             .ValueGeneratedOnAdd();

            modelBuilder.Entity<Message>()
             .HasOne(m => m.Sender)
             .WithMany()
             .HasForeignKey(m => m.SenderId);

            modelBuilder.Entity<Notification>()
             .HasDiscriminator<NotificationType>("NotificationType")
             .HasValue<MessageNotification>(NotificationType.Message)
             .HasValue<ContactRequestNotification>(NotificationType.ContactRequest)
             .HasValue<SystemNotification>(NotificationType.System)
             .HasValue<ChatInviteNotification>(NotificationType.ChatInvite);

            modelBuilder.Entity<ContactRequest>(entity =>
            {
                entity.HasKey(cr => cr.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(cr => cr.Status).HasConversion<string>();

                entity.HasOne<UserProfile>()
                      .WithMany()
                      .HasForeignKey(cr => cr.SenderUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<UserProfile>()
                      .WithMany()
                      .HasForeignKey(cr => cr.RecipientUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(cr => new { cr.SenderUserId, cr.RecipientUserId })
                      .IsUnique();
            });
        }
    }
}
