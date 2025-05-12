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
    public class ChangePasswordUseCaseTests
    {
        [TestMethod]
        public async Task Execute_ValidRequest_ChangesPasswordSuccessfully()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();

            var userId = 1;
            var user = new User { Id = userId, Phone = "1234567890", PasswordHash = "oldHashedPassword" };
            userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);
            passwordHasherMock.Setup(hasher => hasher.VerifyPassword("oldHashedPassword", "oldPassword"))
                .Returns(PasswordVerificationResult.Success);
            passwordHasherMock.Setup(hasher => hasher.HashPassword("newPassword")).Returns("newHashedPassword");
            userRepositoryMock.Setup(repo => repo.Update(It.IsAny<User>())).Returns(Task.CompletedTask); // Assuming Update returns Task

            var useCase = new ChangePasswordUseCase(userRepositoryMock.Object, passwordHasherMock.Object);

            var request = new ChangePasswordRequest { UserId = userId, OldPassword = "oldPassword", NewPassword = "newPassword" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.User);
            Assert.AreEqual(userId, response.User.Id);
            userRepositoryMock.Verify(repo => repo.Update(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public async Task Execute_InvalidUserId_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();

            var useCase = new ChangePasswordUseCase(userRepositoryMock.Object, passwordHasherMock.Object);

            var request = new ChangePasswordRequest { UserId = 0, OldPassword = "oldPassword", NewPassword = "newPassword" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.User);
            Assert.AreEqual("Неверные входные данные.", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_UserNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();

            var userId = 1;
            userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync((User)null);

            var useCase = new ChangePasswordUseCase(userRepositoryMock.Object, passwordHasherMock.Object);

            var request = new ChangePasswordRequest { UserId = userId, OldPassword = "oldPassword", NewPassword = "newPassword" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.User);
            Assert.AreEqual("Пользователь не найден.", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_InvalidOldPassword_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();

            var userId = 1;
            var user = new User { Id = userId, Phone = "1234567890", PasswordHash = "oldHashedPassword" };
            userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);
            passwordHasherMock.Setup(hasher => hasher.VerifyPassword("oldHashedPassword", "oldPassword"))
                .Returns(PasswordVerificationResult.Failed);

            var useCase = new ChangePasswordUseCase(userRepositoryMock.Object, passwordHasherMock.Object);

            var request = new ChangePasswordRequest { UserId = userId, OldPassword = "oldPassword", NewPassword = "newPassword" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.User);
            Assert.AreEqual("Неверный старый пароль.", response.ErrorMessage);
        }
    }
}
