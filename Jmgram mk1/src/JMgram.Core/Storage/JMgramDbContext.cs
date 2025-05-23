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

            // 1. UserProfile: Установка первичного ключа (FK на AppIdentityUser)
            modelBuilder.Entity<UserProfile>()
             .HasKey(up => up.UserId);

            // 2. AppIdentityUser - UserProfile: Связь один к одному
            modelBuilder.Entity<AppIdentityUser>()
             .HasOne(u => u.UserProfile)
             .WithOne(p => p.User)
             .HasForeignKey<UserProfile>(p => p.UserId);

            // 3. AppIdentityUser - Contact: Связь один ко многим
            modelBuilder.Entity<Contact>()
             .HasOne(c => c.User)
             .WithMany(u => u.Contacts)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            // 4. Contact - AppIdentityUser (ContactUser): Связь один ко многим
            modelBuilder.Entity<Contact>()
             .HasOne(c => c.ContactUser)
             .WithMany() // У AppIdentityUser нет коллекции ContactUsers
             .HasForeignKey(c => c.ContactUserId)
             .OnDelete(DeleteBehavior.Restrict); // Запретить удаление контактного пользователя, если он используется в контактах

            // 5. Chat: Автоинкрементное Id чата (ВАЖНО: Указываем длину Id)
            modelBuilder.Entity<Chat>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd()
                .HasMaxLength(100); // Указываем длину для Id

            // 6. ChatUser: Составной ключ (ChatId, UserId) и конфигурация свойств
            modelBuilder.Entity<ChatUser>()
                .HasKey(cu => new { cu.ChatId, cu.UserId });

            modelBuilder.Entity<ChatUser>()
                .Property(cu => cu.ChatId)
                .HasMaxLength(100); // Должно соответствовать Chat.Id

            modelBuilder.Entity<ChatUser>()
                .Property(cu => cu.UserId)
                .HasMaxLength(450); // Должно соответствовать AppIdentityUser.Id

            modelBuilder.Entity<ChatUser>()
                .Property(cu => cu.JoinedAt)
                .HasColumnType("datetime2");

            // 7. ChatUser - Chat: Связь один ко многим
            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.Chat)
                .WithMany(c => c.ChatUsers)
                .HasForeignKey(cu => cu.ChatId);

            // 8. ChatUser - AppIdentityUser: Связь один ко многим
            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.User)
                .WithMany()
                .HasForeignKey(cu => cu.UserId);

            // 9. Message - Chat: Связь один ко многим
            modelBuilder.Entity<Message>()
             .HasOne(m => m.Chat)
             .WithMany() //  У чата нет коллекции Messages
             .HasForeignKey(m => m.ChatId);

            // 10. Message: Автоинкрементное Id сообщения
            modelBuilder.Entity<Message>()
             .Property(m => m.Id)
             .ValueGeneratedOnAdd();

            // 11. Message - AppIdentityUser (Sender): Связь один ко многим
            modelBuilder.Entity<Message>()
             .HasOne(m => m.Sender)
             .WithMany() // У пользователя нет коллекции SentMessages
             .HasForeignKey(m => m.SenderId);

            // 12. Notification: Дискриминатор для типов уведомлений
            modelBuilder.Entity<Notification>()
             .HasDiscriminator<NotificationType>("NotificationType")
             .HasValue<MessageNotification>(NotificationType.Message)
             .HasValue<ContactRequestNotification>(NotificationType.ContactRequest)
             .HasValue<SystemNotification>(NotificationType.System)
             .HasValue<ChatInviteNotification>(NotificationType.ChatInvite);

            // 13. ContactRequest: Конфигурация для ContactRequest
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
