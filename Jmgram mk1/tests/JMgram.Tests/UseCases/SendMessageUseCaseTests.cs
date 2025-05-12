
using Moq;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.tests.JMgram.Tests.UseCases
{
    [TestClass]
    public class SendMessageUseCaseTests
    {
        [TestMethod]
        public async Task Execute_ValidRequest_SendsMessageSuccessfully()
        {
            // Arrange
            var messageRepositoryMock = new Mock<IMessageRepository>();
            var chatRepositoryMock = new Mock<IChatRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            // Настраиваем Mock-объекты
            var chatId = 1;
            var senderId = 10;
            chatRepositoryMock.Setup(repo => repo.GetById(chatId)).ReturnsAsync(new Chat()); // Chat exists
            userRepositoryMock.Setup(repo => repo.GetById(senderId)).ReturnsAsync(new User()); // User exists
            messageRepositoryMock.Setup(repo => repo.Add(It.IsAny<Message>())).ReturnsAsync(123); // Message added successfully - Mocking the return ID

            var useCase = new SendMessageUseCase(messageRepositoryMock.Object, chatRepositoryMock.Object, userRepositoryMock.Object);

            var request = new SendMessageRequest
            {
                Message = new MessageDto
                {
                    ChatId = chatId,
                    SenderId = senderId,
                    Text = "Test message",
                    Timestamp = DateTime.UtcNow 
                }
            };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.Message);
            Assert.AreEqual(chatId, response.Message.ChatId);
            Assert.AreEqual(senderId, response.Message.SenderId);
            Assert.AreEqual("Test message", response.Message.Text);
            messageRepositoryMock.Verify(repo => repo.Add(It.IsAny<Message>()), Times.Once); // Verify that Add was called
        }

        [TestMethod]
        public async Task Execute_NullRequest_ReturnsError()
        {
            // Arrange
            var messageRepositoryMock = new Mock<IMessageRepository>();
            var chatRepositoryMock = new Mock<IChatRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var useCase = new SendMessageUseCase(messageRepositoryMock.Object, chatRepositoryMock.Object, userRepositoryMock.Object);

            // Act
            var response = await useCase.Execute(null); // Pass null request

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("Request cannot be null.", response.ErrorMessage);
            Assert.IsNull(response.Message);
            messageRepositoryMock.Verify(repo => repo.Add(It.IsAny<Message>()), Times.Never); // Add should not be called
        }

        [TestMethod]
        public async Task Execute_NullMessageInRequest_ReturnsError()
        {
            // Arrange
            var messageRepositoryMock = new Mock<IMessageRepository>();
            var chatRepositoryMock = new Mock<IChatRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var useCase = new SendMessageUseCase(messageRepositoryMock.Object, chatRepositoryMock.Object, userRepositoryMock.Object);

            var request = new SendMessageRequest { Message = null };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("Message in request cannot be null.", response.ErrorMessage);
            Assert.IsNull(response.Message);
            messageRepositoryMock.Verify(repo => repo.Add(It.IsAny<Message>()), Times.Never); // Add should not be called
        }

        [TestMethod]
        public async Task Execute_ChatDoesNotExist_ReturnsError()
        {
            // Arrange
            var messageRepositoryMock = new Mock<IMessageRepository>();
            var chatRepositoryMock = new Mock<IChatRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var chatId = 1;
            var senderId = 10;

            chatRepositoryMock.Setup(repo => repo.GetById(chatId)).ReturnsAsync((Chat)null); // Chat does not exist
            userRepositoryMock.Setup(repo => repo.GetById(senderId)).ReturnsAsync(new User()); // User exists

            var useCase = new SendMessageUseCase(messageRepositoryMock.Object, chatRepositoryMock.Object, userRepositoryMock.Object);

            var request = new SendMessageRequest
            {
                Message = new MessageDto
                {
                    ChatId = chatId,
                    SenderId = senderId,
                    Text = "Test message",
                }
            };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual($"Chat with Id {chatId} does not exist.", response.ErrorMessage);
            Assert.IsNull(response.Message);
            messageRepositoryMock.Verify(repo => repo.Add(It.IsAny<Message>()), Times.Never); // Add should not be called
        }

        [TestMethod]
        public async Task Execute_UserDoesNotExist_ReturnsError()
        {
            // Arrange
            var messageRepositoryMock = new Mock<IMessageRepository>();
            var chatRepositoryMock = new Mock<IChatRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var chatId = 1;
            var senderId = 10;

            chatRepositoryMock.Setup(repo => repo.GetById(chatId)).ReturnsAsync(new Chat()); // Chat exists
            userRepositoryMock.Setup(repo => repo.GetById(senderId)).ReturnsAsync((User)null); // User does not exist

            var useCase = new SendMessageUseCase(messageRepositoryMock.Object, chatRepositoryMock.Object, userRepositoryMock.Object);

            var request = new SendMessageRequest
            {
                Message = new MessageDto
                {
                    ChatId = chatId,
                    SenderId = senderId,
                    Text = "Test message",
                }
            };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual($"User with Id {senderId} does not exist.", response.ErrorMessage);
            Assert.IsNull(response.Message);
            messageRepositoryMock.Verify(repo => repo.Add(It.IsAny<Message>()), Times.Never); // Add should not be called
        }
        [TestMethod]
        public async Task Execute_EmptyMessageText_ReturnsError()
        {
            // Arrange
            var messageRepositoryMock = new Mock<IMessageRepository>();
            var chatRepositoryMock = new Mock<IChatRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var chatId = 1;
            var senderId = 10;

            chatRepositoryMock.Setup(repo => repo.GetById(chatId)).ReturnsAsync(new Chat()); // Chat exists
            userRepositoryMock.Setup(repo => repo.GetById(senderId)).ReturnsAsync(new User()); // User exists

            var useCase = new SendMessageUseCase(messageRepositoryMock.Object, chatRepositoryMock.Object, userRepositoryMock.Object);

            var request = new SendMessageRequest
            {
                Message = new MessageDto
                {
                    ChatId = chatId,
                    SenderId = senderId,
                    Text = "", // Empty message
                }
            };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("Message text cannot be empty.", response.ErrorMessage);
            Assert.IsNull(response.Message);
            messageRepositoryMock.Verify(repo => repo.Add(It.IsAny<Message>()), Times.Never); // Add should not be called
        }
    }
}