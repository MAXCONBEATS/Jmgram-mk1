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
    public class GetContactListUseCaseTests
    {
        [TestMethod]
        public async Task Execute_ValidRequest_ReturnsContactList()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var contactRepositoryMock = new Mock<IContactRepository>();

            // Setup Mocks
            var userId = 1;
            var user = new User { Id = userId, Phone = "1234567890" };
            userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);

            var contacts = new List<Contact>
            {
                new Contact { Id = 1, UserId = userId, ContactUserId = 2, AddedAt = DateTime.UtcNow },
                new Contact { Id = 2, UserId = userId, ContactUserId = 3, AddedAt = DateTime.UtcNow }
            };
            contactRepositoryMock.Setup(repo => repo.GetContactsForUser(userId)).ReturnsAsync(contacts);

            var useCase = new GetContactListUseCase(userRepositoryMock.Object, contactRepositoryMock.Object);

            var request = new GetContactListRequest { UserId = userId };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.Contacts);
            Assert.AreEqual(2, response.Contacts.Count);
            Assert.AreEqual(1, response.Contacts[0].UserId);
            Assert.AreEqual(2, response.Contacts[0].ContactUserId);
        }

        [TestMethod]
        public async Task Execute_InvalidUserId_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var contactRepositoryMock = new Mock<IContactRepository>();

            var useCase = new GetContactListUseCase(userRepositoryMock.Object, contactRepositoryMock.Object);

            var request = new GetContactListRequest { UserId = 0 }; // Invalid UserId

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNotNull(response.Contacts);
            Assert.AreEqual(0, response.Contacts.Count);
            Assert.AreEqual("UserId must be greater than 0.", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_UserNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var contactRepositoryMock = new Mock<IContactRepository>();

            var userId = 1;
            userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync((User)null); // User not found

            var useCase = new GetContactListUseCase(userRepositoryMock.Object, contactRepositoryMock.Object);

            var request = new GetContactListRequest { UserId = userId };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNotNull(response.Contacts);
            Assert.AreEqual(0, response.Contacts.Count);
            Assert.AreEqual($"User with UserId {userId} not found.", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_ExceptionThrownByRepository_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var contactRepositoryMock = new Mock<IContactRepository>();

            var userId = 1;
            var user = new User { Id = userId, Phone = "1234567890" };
            userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);

            contactRepositoryMock.Setup(repo => repo.GetContactsForUser(userId))
                .ThrowsAsync(new Exception("Test Exception")); // Simulate exception

            var useCase = new GetContactListUseCase(userRepositoryMock.Object, contactRepositoryMock.Object);

            var request = new GetContactListRequest { UserId = userId };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNotNull(response.Contacts);
            Assert.AreEqual(0, response.Contacts.Count);
            Assert.AreEqual("Error retrieving contacts: Test Exception", response.ErrorMessage);
        }
    }
}
