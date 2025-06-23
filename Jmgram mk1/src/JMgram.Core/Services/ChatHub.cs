using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Services
{
    public class ChatHub : Hub
    {
        private readonly ISendMessageUseCase _sendMessageUseCase;
        private readonly IChatRepository _chatRepository;
        private readonly UpdateMessageStatusUseCase _updateMessageStatusUseCase;
        private readonly UpdateMessageTextUseCase _updateMessageTextUseCase;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ISendMessageUseCase sendMessageUseCase, ILogger<ChatHub> logger, UpdateMessageStatusUseCase updateMessageStatusUseCase, UpdateMessageTextUseCase updateMessageTextUseCase, IChatRepository chatRepository)
        {
            _sendMessageUseCase = sendMessageUseCase ?? throw new ArgumentNullException(nameof(sendMessageUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _updateMessageStatusUseCase = updateMessageStatusUseCase;
            _updateMessageTextUseCase = updateMessageTextUseCase;
            _chatRepository = chatRepository;
        }

        public async Task SendMessage(string chatId, string message)
        {
            _logger.LogInformation($"SendMessage request received. ChatId: {chatId}, Message: {message}, User: {Context.UserIdentifier}");
            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("User ID is null or empty.");
                    return;
                }

                var request = new SendMessageRequest
                {
                    Message = new MessageForSendingDto
                    {
                        ChatId = chatId,
                        Text = message,
                    }
                };

                var response = await _sendMessageUseCase.Execute(request, userId);

                if (response.IsSuccess)
                {
                    _logger.LogInformation($"Message sending success. ChatId: {chatId}, User: {Context.UserIdentifier}");
                    await Clients.Group(chatId).SendAsync("ReceiveMessage", userId, message);
                    _logger.LogInformation($"Sent to group {chatId} User: {Context.UserIdentifier} Message: {message}");
                }
                else
                {
                    _logger.LogError($"Message sending failed. ChatId: {chatId}, User: {Context.UserIdentifier}, Error: {response.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending message.");
            }
        }
        public async Task UpdateMessage(int messageId, string newText)
        {
            _logger.LogInformation($"UpdateMessage request received. MessageId: {messageId}, NewText: {newText}, User: {Context.UserIdentifier}");

            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("User ID is null or empty.");
                    return;
                }

                var request = new UpdateMessageTextRequest
                {
                    MessageId = messageId,
                    Text = newText
                };

                var response = await _updateMessageTextUseCase.Execute(request);

                if (response.IsSuccess)
                {
                    _logger.LogInformation($"Message update success. MessageId: {messageId}, User: {Context.UserIdentifier}");

                    // Получаем chatId для сообщения (нужно добавить в use case или получить отдельно)
                    var message = await _chatRepository.GetMessageById(messageId);
                    if (message != null)
                    {
                        await Clients.Group(message.ChatId).SendAsync("MessageUpdated", messageId, newText, userId);
                        _logger.LogInformation($"Sent message update to group {message.ChatId}");
                    }
                }
                else
                {
                    _logger.LogError($"Message update failed. MessageId: {messageId}, User: {Context.UserIdentifier}, Error: {response.ErrorMessage}");
                    await Clients.Caller.SendAsync("MessageUpdateFailed", messageId, response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating message.");
                await Clients.Caller.SendAsync("MessageUpdateFailed", messageId, "Internal server error");
            }
        }
        public async Task JoinChat(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            _logger.LogInformation($"User {Context.ConnectionId} joined chat {chatId}");
        }

        public async Task LeaveChat(string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
            _logger.LogInformation($"User {Context.ConnectionId} left chat {chatId}");
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"User connected with connection ID: {Context.ConnectionId} and UserId: {userId}");
            await base.OnConnectedAsync();
        }
    }
}
