using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public interface IGetLastChatMessageUseCase
    {
        Task<LastChatMessageDto> Execute(string chatId, string userId);
    }
    public class GetLastChatMessageUseCase : IGetLastChatMessageUseCase
    {
        private readonly IChatRepository _chatRepository;

        public GetLastChatMessageUseCase(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        }

        public async Task<LastChatMessageDto> Execute(string chatId, string userId)
        {
            //  Здесь можно добавить дополнительную бизнес-логику, если необходимо
            //  Например, проверить, имеет ли пользователь доступ к чату,
            //  или выполнить какие-либо преобразования данных

            Guid chatIdGuid;
            if (!Guid.TryParse(chatId, out chatIdGuid))
            {
                //  Обработка ошибки: не удалось преобразовать chatId в Guid
                //  Например, выбросить исключение или вернуть null
                throw new ArgumentException($"Invalid ChatId format: {chatId}");
                // return null; // Если возвращаете null, не забудьте обработать его в контроллере
            }

            var lastChatMessage = await _chatRepository.GetLastChatMessage(chatId);

            //  Проверка на null
            if (lastChatMessage == null)
            {
                return null; //  Или выбросить исключение, если это необходимо
            }

            return lastChatMessage;
        }
    }
}