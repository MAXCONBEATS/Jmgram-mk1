using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.Responses
{
    public class GetContactListResponse
    {
        public bool IsSuccess { get; set; } = true; // По умолчанию считаем успешным
        public string? ErrorMessage { get; set; }
        public List<ContactDto> Contacts { get; set; } = new List<ContactDto>(); // Инициализируем список, чтобы избежать null reference exceptions
    }
}




