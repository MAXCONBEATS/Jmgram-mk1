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
    public class LoginUseCaseTests
    {
        [TestMethod]
        public async Task Execute_ValidCredentials_ReturnsSuccessResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();

            // Mock the user repository to return a user
            var user = new User { Id = 1, Phone = "1234567890", PasswordHash = "hashedPassword" };
            userRepositoryMock.Setup(repo => repo.GetByPhone("1234567890")).ReturnsAsync(user);

            // Mock the password hasher to return success
            passwordHasherMock.Setup(hasher => hasher.VerifyPassword("hashedPassword", "password"))
                .Returns(PasswordVerificationResult.Success);

            var useCase = new LoginUseCase(userRepositoryMock.Object, passwordHasherMock.Object);

            var request = new LoginRequest { Phone = "1234567890", Password = "password" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.User);
            Assert.AreEqual("1", response.User.Id.ToString());
            Assert.AreEqual("1234567890", response.User.Phone);
        }

        [TestMethod]
        public async Task Execute_InvalidPhone_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();

            // Mock the user repository to return null (user not found)
            userRepositoryMock.Setup(repo => repo.GetByPhone(It.IsAny<string>())).ReturnsAsync((User)null);

            var useCase = new LoginUseCase(userRepositoryMock.Object, passwordHasherMock.Object);

            var request = new LoginRequest { Phone = "1234567890", Password = "password" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.User);
            Assert.AreEqual("Пользователь с таким номером телефона не найден.", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_InvalidPassword_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();

            // Mock the user repository to return a user
            var user = new User { Id = 1, Phone = "1234567890", PasswordHash = "hashedPassword" };
            userRepositoryMock.Setup(repo => repo.GetByPhone("1234567890")).ReturnsAsync(user);

            // Mock the password hasher to return failed verification
            passwordHasherMock.Setup(hasher => hasher.VerifyPassword("hashedPassword", "password"))
                .Returns(PasswordVerificationResult.Failed);

            var useCase = new LoginUseCase(userRepositoryMock.Object, passwordHasherMock.Object);

            var request = new LoginRequest { Phone = "1234567890", Password = "password" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.User);
            Assert.AreEqual("Неверный пароль.", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_MissingCredentials_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();

            var useCase = new LoginUseCase(userRepositoryMock.Object, passwordHasherMock.Object);

            var request = new LoginRequest { Phone = "", Password = "" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.User);
            Assert.AreEqual("Номер телефона и пароль должны быть заполнены.", response.ErrorMessage);
        }
    }
}
