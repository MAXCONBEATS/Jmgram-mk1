using Microsoft.EntityFrameworkCore;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Jmgram_mk1.src.JMgram.Core.Storage
{
    public class JMgramDbContext : IdentityDbContext<AppIdentityUser>
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Contact> Contacts { get; set; } = null!;
        public DbSet<SystemNotification> SystemNotifications { get; set; }
        public DbSet<MessageNotification> MessageNotifications { get; set; }
        public DbSet<ContactRequestNotification> ContactRequestNotifications { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public DbSet<ChatUser> ChatUsers { get; set; } = null!;
        public JMgramDbContext(DbContextOptions<JMgramDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>()
        .HasKey(up => up.UserId);

            modelBuilder.Entity<AppIdentityUser>()
                .HasOne(u => u.UserProfile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId);

            // Связь один ко многим между User и Contact (по UserId)
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.User)
                .WithMany(u => u.Contacts)  //Добавим связь к контактам
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Запретить удаление пользователя, если есть контакты

            // Связь один ко многим между User и Contact (по ContactUserId)
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.ContactUser)
                .WithMany() // У User нет коллекции ContactUsers
                .HasForeignKey(c => c.ContactUserId)
                .OnDelete(DeleteBehavior.Restrict); // Запретить удаление контактного пользователя, если он используется в контактах

            // Составной ключ для таблицы Contact (UserId, ContactUserId)
            modelBuilder.Entity<Contact>()
                .HasKey(c => new { c.UserId, c.ContactUserId });
            modelBuilder.Entity<ChatUser>().HasKey(cu => new { cu.ChatId, cu.UserId });

            // ChatUser: Связь с Chat
            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.Chat)
                .WithMany(c => c.ChatUsers)
                .HasForeignKey(cu => cu.ChatId);

            // ChatUser: Связь с User
            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.ChatUsers)
                .HasForeignKey(cu => cu.UserId);
            // Message: Связь с Chat
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany() //  У чата нет коллекции Messages
                .HasForeignKey(m => m.ChatId);

            // Message: Связь с User (Sender)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany() // У пользователя нет коллекции SentMessages
                .HasForeignKey(m => m.SenderId);
            // Notification: Все типы уведомлений в одной таблице
            modelBuilder.Entity<Notification>()
                .HasDiscriminator<string>("NotificationType")
                .HasValue<SystemNotification>("System")
                .HasValue<MessageNotification>("Message")
                .HasValue<ContactRequestNotification>("ContactRequest");

            base.OnModelCreating(modelBuilder);
        }

    }
}
