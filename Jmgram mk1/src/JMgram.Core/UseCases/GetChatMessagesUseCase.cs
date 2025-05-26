
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.AspNetCore.Http;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class GetChatMessagesUseCase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public GetChatMessagesUseCase(IChatRepository chatRepository, IMessageRepository messageRepository, IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userRepository = userRepository;
        }

        public async Task<GetChatMessagesResponse> Execute(GetChatMessagesRequest request, string userId)
        {
            if (string.IsNullOrEmpty(request.ChatId) || request.PageNumber <= 0 || request.PageSize <= 0)
            {
                return new GetChatMessagesResponse
                {
                    Chat = new List<MessageDto>(),
                    TotalMessages = 0,
                    TotalPages = 0
                };
            }
            // 2.1 Check Auth
            if (!await _chatRepository.IsUserInChat(request.ChatId, userId))
    {
                return new GetChatMessagesResponse
                {
                    Chat = new List<MessageDto>(),
                    TotalMessages = 0,
                    TotalPages = 0
                };
            }
            var totalMessages = await _messageRepository.GetTotalMessageCount(request.ChatId);

            var totalPages = (int)Math.Ceiling((double)totalMessages / request.PageSize);

            var messages = await _messageRepository.GetMessagesForChat(request.ChatId, request.PageNumber, request.PageSize);

            var messageDtos = new List<MessageDto>();
            foreach (var m in messages)
            {
                var senderName = await _userRepository.GetUserFirstNameById(m.SenderId);
                messageDtos.Add(new MessageDto
                {
                    ChatId = m.ChatId,
                    Text = m.Text,
                    Timestamp = m.Timestamp,
                    SenderId = m.SenderId,
                    SenderName = senderName
                });
            }

            return new GetChatMessagesResponse
            {
                Chat = messageDtos,
                TotalMessages = totalMessages,
                TotalPages = totalPages
            };
        }

    }

}
