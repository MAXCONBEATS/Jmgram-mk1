using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.Responses
{
    public class GetContactListResponse
    {
        public bool IsSuccess { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public List<ContactDto> Contacts { get; set; } = new List<ContactDto>();
    }
}




