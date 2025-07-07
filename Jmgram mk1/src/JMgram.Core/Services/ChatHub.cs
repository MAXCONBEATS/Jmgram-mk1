using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Microsoft.AspNetCore.Identity;
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
        private readonly IUserRepository _userRepository;
        private readonly UpdateMessageStatusUseCase _updateMessageStatusUseCase;
        private readonly UpdateMessageTextUseCase _updateMessageTextUseCase;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ISendMessageUseCase sendMessageUseCase, ILogger<ChatHub> logger, UpdateMessageStatusUseCase updateMessageStatusUseCase, UpdateMessageTextUseCase updateMessageTextUseCase, IChatRepository chatRepository, IUserRepository userRepository)
        {
            _sendMessageUseCase = sendMessageUseCase ?? throw new ArgumentNullException(nameof(sendMessageUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _updateMessageStatusUseCase = updateMessageStatusUseCase;
            _updateMessageTextUseCase = updateMessageTextUseCase;
            _chatRepository = chatRepository;
            _userRepository = userRepository;
        }

        public async Task SendMessage(string chatId, string message)
        {
            string status = "Sent";
            try
            {
                var userId = Context.UserIdentifier;
                var userName = Context.User?.Identity?.Name ?? "Unknown";

                _logger.LogInformation($"SendMessage attempt - User: {userName} ({userId}), Chat: {chatId}");

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("User ID is null or empty.");
                    await Clients.Caller.SendAsync("MessageSendFailed", "Пользователь не авторизован");
                    return;
                }

                // ДОБАВЛЯЕМ: Проверка прав на отправку сообщений
                var chat = await _chatRepository.GetById(chatId);
                if (chat == null)
                {
                    _logger.LogWarning($"Chat not found: {chatId}");
                    await Clients.Caller.SendAsync("MessageSendFailed", "Чат не найден");
                    return;
                }

                // Проверяем права в зависимости от типа чата
                bool canSend = false;
                string errorMessage = "";

                switch (chat.ChatType)
                {
                    case ChatType.Channel:
                        // В канале писать может только создатель
                        canSend = chat.CreatorUserId == userId;
                        errorMessage = canSend ? "" : "В канале может писать только создатель";
                        break;

                    case ChatType.Group:
                        // В группе могут писать все участники
                        canSend = await _chatRepository.IsUserInChat(chatId, userId);
                        errorMessage = canSend ? "" : "Вы не являетесь участником группы";
                        break;

                    case ChatType.Private:
                        // В приватном чате могут писать оба участника
                        canSend = await _chatRepository.IsUserInChat(chatId, userId);
                        errorMessage = canSend ? "" : "Вы не являетесь участником чата";
                        break;

                    default:
                        canSend = false;
                        errorMessage = "Неизвестный тип чата";
                        break;
                }

                _logger.LogInformation($"Permission check - User: {userId}, Chat: {chatId}, ChatType: {chat.ChatType}, CanSend: {canSend}");

                if (!canSend)
                {
                    _logger.LogWarning($"User {userId} denied sending message to chat {chatId}: {errorMessage}");
                    await Clients.Caller.SendAsync("MessageSendFailed", errorMessage);
                    return;
                }

                // Отправляем сообщение
                var request = new SendMessageRequest
                {
                    Message = new MessageForSendingDto
                    {
                        ChatId = chatId,
                        Text = message,
                        Status = status
                    }
                };

                var response = await _sendMessageUseCase.Execute(request, userId);

                if (response.IsSuccess)
                {
                    _logger.LogInformation($"Message sent successfully - ChatId: {chatId}, User: {userId}");

                    var user = await _userRepository.GetById(userId);
                    var senderName = user?.FirstName ?? user?.Phone ?? "Unknown";

                    await Clients.Group(chatId).SendAsync(
                        "ReceiveMessage",
                        senderName,
                        message,
                        response.Id.ToString() ?? Guid.NewGuid().ToString(),
                        userId,
                        status
                    );

                    _logger.LogInformation($"Message broadcasted to group {chatId}");
                }
                else
                {
                    _logger.LogError($"Message sending failed - ChatId: {chatId}, User: {userId}, Error: {response.ErrorMessage}");
                    await Clients.Caller.SendAsync("MessageSendFailed", response.ErrorMessage ?? "Ошибка отправки сообщения");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in SendMessage - ChatId: {chatId}");
                await Clients.Caller.SendAsync("MessageSendFailed", "Произошла ошибка при отправке сообщения");
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
            var userId = Context.UserIdentifier;
            var userName = Context.User?.Identity?.Name ?? "Unknown";

            _logger.LogInformation($"User connected - {userName} ({userId})");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            var userName = Context.User?.Identity?.Name ?? "Unknown";

            _logger.LogInformation($"User disconnected - {userName} ({userId})");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
