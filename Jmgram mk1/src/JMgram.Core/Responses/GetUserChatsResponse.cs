using Jmgram_mk1.src.JMgram.Core.Entities;

namespace Jmgram_mk1.src.JMgram.Core.Responses
{
    public class GetUserChatsResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public List<Chat> Chats { get; set; }
    }
}
