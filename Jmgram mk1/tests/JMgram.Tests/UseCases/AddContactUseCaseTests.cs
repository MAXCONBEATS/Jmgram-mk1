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
    public class AddContactUseCaseTests
    {
        [TestMethod]
        public async Task Execute_ValidRequest_AddsContactSuccessfully()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var contactRepositoryMock = new Mock<IContactRepository>();

            var userId = 1;
            var contactUserId = 2;
            var user = new User { Id = userId, Phone = "1234567890" };
            var contactUser = new User { Id = contactUserId, Phone = "0987654321" };

            userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);
            userRepositoryMock.Setup(repo => repo.GetById(contactUserId)).ReturnsAsync(contactUser);
            contactRepositoryMock.Setup(repo => repo.IsContact(userId, contactUserId)).ReturnsAsync(false);
            contactRepositoryMock.Setup(repo => repo.Add(It.IsAny<Contact>())).Returns(Task.CompletedTask); // Assuming Add returns Task

            var useCase = new AddContactUseCase(userRepositoryMock.Object, contactRepositoryMock.Object);

            var request = new AddContactRequest { UserId = userId, ContactUserId = contactUserId };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.Contact);
            Assert.AreEqual(userId, response.Contact.UserId);
            Assert.AreEqual(contactUserId, response.Contact.ContactUserId);
            contactRepositoryMock.Verify(repo => repo.Add(It.IsAny<Contact>()), Times.Once);
        }

        [TestMethod]
        public async Task Execute_InvalidUserId_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var contactRepositoryMock = new Mock<IContactRepository>();

            var useCase = new AddContactUseCase(userRepositoryMock.Object, contactRepositoryMock.Object);

            var request = new AddContactRequest { UserId = 0, ContactUserId = 2 }; // Invalid UserId

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.Contact);
            Assert.AreEqual("Неверные идентификаторы пользователей.", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_InvalidContactUserId_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var contactRepositoryMock = new Mock<IContactRepository>();

            var useCase = new AddContactUseCase(userRepositoryMock.Object, contactRepositoryMock.Object);

            var request = new AddContactRequest { UserId = 1, ContactUserId = 0 }; // Invalid ContactUserId

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.Contact);
            Assert.AreEqual("Неверные идентификаторы пользователей.", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_SameUserIds_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var contactRepositoryMock = new Mock<IContactRepository>();

            var userId = 1;
            var useCase = new AddContactUseCase(userRepositoryMock.Object, contactRepositoryMock.Object);

            var request = new AddContactRequest { UserId = userId, ContactUserId = userId }; // Same UserIds

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.Contact);
            Assert.AreEqual("Нельзя добавить себя в контакты.", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_UserNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var contactRepositoryMock = new Mock<IContactRepository>();

            var userId = 1;
            var contactUserId = 2;

            userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync((User)null);
            userRepositoryMock.Setup(repo => repo.GetById(contactUserId)).ReturnsAsync(new User { Id = contactUserId });

            var useCase = new AddContactUseCase(userRepositoryMock.Object, contactRepositoryMock.Object);

            var request = new AddContactRequest { UserId = userId, ContactUserId = contactUserId };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.Contact);
            Assert.AreEqual("Один или оба пользователя не найдены.", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_ContactUserNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var contactRepositoryMock = new Mock<IContactRepository>();

            var userId = 1;
            var contactUserId = 2;

            userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(new User { Id = userId });
            userRepositoryMock.Setup(repo => repo.GetById(contactUserId)).ReturnsAsync((User)null);

            var useCase = new AddContactUseCase(userRepositoryMock.Object, contactRepositoryMock.Object);

            var request = new AddContactRequest { UserId = userId, ContactUserId = contactUserId };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.Contact);
            Assert.AreEqual("Один или оба пользователя не найдены.", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_ContactAlreadyAdded_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var contactRepositoryMock = new Mock<IContactRepository>();

            var userId = 1;
            var contactUserId = 2;
            var user = new User { Id = userId, Phone = "1234567890" };
            var contactUser = new User { Id = contactUserId, Phone = "0987654321" };

            userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);
            userRepositoryMock.Setup(repo => repo.GetById(contactUserId)).ReturnsAsync(contactUser);
            contactRepositoryMock.Setup(repo => repo.IsContact(userId, contactUserId)).ReturnsAsync(true);

            var useCase = new AddContactUseCase(userRepositoryMock.Object, contactRepositoryMock.Object);

            var request = new AddContactRequest { UserId = userId, ContactUserId = contactUserId };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.Contact);
            Assert.AreEqual("Этот пользователь уже добавлен в контакты.", response.ErrorMessage);
        }
    }
}