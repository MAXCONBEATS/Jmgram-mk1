using Microsoft.EntityFrameworkCore;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Jmgram_mk1.src.JMgram.Core.Storage
{
    public class JMgramDbContext : IdentityDbContext<AppIdentityUser>
    {
        public DbSet<AppIdentityUser> Users { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Contact> Contacts { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MessageNotification> MessageNotifications { get; set; }
        public DbSet<ContactRequestNotification> ContactRequestNotifications { get; set; }
        public DbSet<SystemNotification> SystemNotifications { get; set; }
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
            // 11.Contact
            modelBuilder.Entity<Contact>().HasKey(c => c.Id);

            modelBuilder.Entity<Contact>()
                .Property(c => c.Id)
                .UseIdentityColumn();

            //автоинкрементное id чата
            modelBuilder.Entity<Chat>()
             .Property(c => c.Id)
             .ValueGeneratedOnAdd();

            // 6. ChatUser: Составной ключ (ChatId, UserId)
            modelBuilder.Entity<ChatUser>()
             .HasKey(cu => new { cu.ChatId, cu.UserId });

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
            //автоинкрементное id сообщения
            modelBuilder.Entity<Message>()
             .Property(m => m.Id)
             .ValueGeneratedOnAdd();

            // 10. Message - AppIdentityUser (Sender): Связь один ко многим
            modelBuilder.Entity<Message>()
             .HasOne(m => m.Sender)
             .WithMany() // У пользователя нет коллекции SentMessages
             .HasForeignKey(m => m.SenderId);

            // 11. Notification: Дискриминатор для типов уведомлений
            modelBuilder.Entity<Notification>()
             .HasDiscriminator<NotificationType>("NotificationType")
             .HasValue<MessageNotification>(NotificationType.Message)
             .HasValue<ContactRequestNotification>(NotificationType.ContactRequest)
             .HasValue<SystemNotification>(NotificationType.System);

            modelBuilder.Entity<ContactRequest>()
             .HasKey(cr => cr.Id); // Primary key

            modelBuilder.Entity<ContactRequest>()
             .Property(cr => cr.Id)
             .UseIdentityColumn(); // Configure Id to be an identity column

            modelBuilder.Entity<ContactRequest>()
             .Property(cr => cr.Status)
             .HasConversion<string>(); // Store enum as string

            modelBuilder.Entity<ContactRequest>()
             .HasOne<UserProfile>() // Assuming you have a UserProfile entity
             .WithMany()
             .HasForeignKey(cr => cr.SenderUserId)
             .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<ContactRequest>()
             .HasOne<UserProfile>() // Assuming you have a UserProfile entity
             .WithMany()
             .HasForeignKey(cr => cr.RecipientUserId)
             .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<ContactRequest>()
             .HasIndex(cr => new { cr.SenderUserId, cr.RecipientUserId })
             .IsUnique(); // Optional: Ensure only one pending request between users
        }
    }
}
