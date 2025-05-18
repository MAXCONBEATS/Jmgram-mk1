namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class Chat
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<ChatUser> ChatUsers { get; set; } = new List<ChatUser>();
        public string CreatorUserId { get; set; }
    }



}
