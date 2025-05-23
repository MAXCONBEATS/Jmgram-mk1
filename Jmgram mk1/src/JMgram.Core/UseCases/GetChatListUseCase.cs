

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Microsoft.Extensions.Logging;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public interface IGetChatListUseCase
    {
        Task<GetChatListResponse> Execute(string chatId, string userId);
    }
    public class GetChatListUseCase : IGetChatListUseCase
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<GetChatListUseCase> _logger;

        public GetChatListUseCase(IChatRepository chatRepository, ILogger<GetChatListUseCase> logger)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GetChatListResponse> Execute(string chatId, string userId)
        {
            _logger.LogInformation($"GetChatListUseCase.Execute: Attempting to retrieve chat list for ChatId = {chatId} and UserId = {userId}.");

            // 1. Проверить входные данные
            if (string.IsNullOrEmpty(chatId) || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetChatListUseCase.Execute: ChatId or UserId is null or empty.");
                return new GetChatListResponse { IsSuccess = false, ErrorMessage = "ChatId и UserId не могут быть пустыми", Users = new List<UserDto>() };
            }

            // 2. Проверить, что пользователь является участником чата
            if (!await _chatRepository.IsUserInChat(chatId, userId))
            {
                _logger.LogWarning($"GetChatListUseCase.Execute: User {userId} is not a member of chat {chatId}.");
                return new GetChatListResponse { IsSuccess = false, ErrorMessage = "Вы не являетесь участником этого чата", Users = new List<UserDto>() };
            }

            // 3. Получаем все записи ChatUser по ChatId
            var chatUsers = await _chatRepository.GetChatUsers(chatId);

            // 4. Если чат не найден (нет записей ChatUser), вернуть пустой список или ошибку
            if (chatUsers == null || chatUsers.Count == 0)
            {
                _logger.LogInformation($"GetChatListUseCase.Execute: Chat {chatId} not found or has no members.");
                return new GetChatListResponse { IsSuccess = true, Users = new List<UserDto>() };
            }

            // 5. Преобразуем ChatUser в UserDto
            var userDtos = chatUsers.Select(cu => new UserDto
            {
                Id = cu.UserId,
                Phone = cu.User.PhoneNumber,
                FirstName = cu.User.FirstName
            }).ToList();

            _logger.LogInformation($"GetChatListUseCase.Execute: Successfully retrieved chat list for ChatId = {chatId} and UserId = {userId}.");
            return new GetChatListResponse { IsSuccess = true, Users = userDtos };
        }
    }


}
