
using Swashbuckle.AspNetCore.Annotations;

namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public class ChatDto
    {
        [SwaggerSchema(ReadOnly = true)]
        public string? ChatId { get; init; }
        public string Name { get; set; }
    }


}
