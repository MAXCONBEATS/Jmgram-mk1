using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Jmgram_mk1.src.JMgram.Core.Services;
using Microsoft.AspNetCore.Identity;
namespace Jmgram_mk1.tests.JMgram.Tests.UseCases
{
    [TestClass]
    public class GetChatMessagesUseCaseTests
    {
        [TestMethod]
        public async Task Execute_ValidRequest_ReturnsChatMessages()
        {
            // Arrange
            var chatRepositoryMock = new Mock<IChatRepository>();
            var messageRepositoryMock = new Mock<IMessageRepository>();

            var chatId = 1;
            var pageNumber = 1;
            var pageSize = 10;
            var totalMessages = 25;

            messageRepositoryMock.Setup(repo => repo.GetTotalMessageCount(chatId)).ReturnsAsync(totalMessages);
            messageRepositoryMock.Setup(repo => repo.GetMessagesForChat(chatId, pageNumber, pageSize))
                .ReturnsAsync(new List<Message>
                {
                    new Message { Id = 1, ChatId = chatId, SenderId = 1, Text = "Message 1", Timestamp = DateTime.UtcNow },
                    new Message { Id = 2, ChatId = chatId, SenderId = 2, Text = "Message 2", Timestamp = DateTime.UtcNow }
                });

            var useCase = new GetChatMessagesUseCase(chatRepositoryMock.Object, messageRepositoryMock.Object);
            var request = new GetChatMessagesRequest { ChatId = chatId, PageNumber = pageNumber, PageSize = pageSize };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Chat);
            Assert.AreEqual(2, response.Chat.Count);
            Assert.AreEqual(totalMessages, response.TotalMessages);
            Assert.AreEqual(3, response.TotalPages); // 25 messages
        }

        [TestMethod]
        public async Task Execute_InvalidChatId_ReturnsEmptyResult()
        {
            // Arrange
            var chatRepositoryMock = new Mock<IChatRepository>();
            var messageRepositoryMock = new Mock<IMessageRepository>();

            var useCase = new GetChatMessagesUseCase(chatRepositoryMock.Object, messageRepositoryMock.Object);
            var request = new GetChatMessagesRequest { ChatId = 0, PageNumber = 1, PageSize = 10 }; // Invalid ChatId

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Chat);
            Assert.AreEqual(0, response.Chat.Count);
            Assert.AreEqual(0, response.TotalMessages);
            Assert.AreEqual(0, response.TotalPages);
        }

        [TestMethod]
        public async Task Execute_InvalidPageNumber_ReturnsEmptyResult()
        {
            // Arrange
            var chatRepositoryMock = new Mock<IChatRepository>();
            var messageRepositoryMock = new Mock<IMessageRepository>();

            var useCase = new GetChatMessagesUseCase(chatRepositoryMock.Object, messageRepositoryMock.Object);
            var request = new GetChatMessagesRequest { ChatId = 1, PageNumber = 0, PageSize = 10 }; // Invalid PageNumber

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Chat);
            Assert.AreEqual(0, response.Chat.Count);
            Assert.AreEqual(0, response.TotalMessages);
            Assert.AreEqual(0, response.TotalPages);
        }

        [TestMethod]
        public async Task Execute_InvalidPageSize_ReturnsEmptyResult()
        {
            // Arrange
            var chatRepositoryMock = new Mock<IChatRepository>();
            var messageRepositoryMock = new Mock<IMessageRepository>();

            var useCase = new GetChatMessagesUseCase(chatRepositoryMock.Object, messageRepositoryMock.Object);
            var request = new GetChatMessagesRequest { ChatId = 1, PageNumber = 1, PageSize = 0 }; // Invalid PageSize

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Chat);
            Assert.AreEqual(0, response.Chat.Count);
            Assert.AreEqual(0, response.TotalMessages);
            Assert.AreEqual(0, response.TotalPages);
        }

        [TestMethod]
        public async Task Execute_NoMessagesInChat_ReturnsEmptyChat()
        {
            // Arrange
            var chatRepositoryMock = new Mock<IChatRepository>();
            var messageRepositoryMock = new Mock<IMessageRepository>();

            var chatId = 1;
            var pageNumber = 1;
            var pageSize = 10;

            messageRepositoryMock.Setup(repo => repo.GetTotalMessageCount(chatId)).ReturnsAsync(0);
            messageRepositoryMock.Setup(repo => repo.GetMessagesForChat(chatId, pageNumber, pageSize))
                .ReturnsAsync(new List<Message>());

            var useCase = new GetChatMessagesUseCase(chatRepositoryMock.Object, messageRepositoryMock.Object);
            var request = new GetChatMessagesRequest { ChatId = chatId, PageNumber = pageNumber, PageSize = pageSize };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Chat);
            Assert.AreEqual(0, response.Chat.Count);
            Assert.AreEqual(0, response.TotalMessages);
            Assert.AreEqual(0, response.TotalPages);
        }
    }
}