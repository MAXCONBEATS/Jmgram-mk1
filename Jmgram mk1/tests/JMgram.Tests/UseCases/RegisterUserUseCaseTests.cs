using Moq;
using System.Threading.Tasks;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Jmgram_mk1.src.JMgram.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jmgram_mk1.tests.JMgram.Tests.UseCases
{
    [TestClass]
    public class RegisterUserUseCaseTests
    {
        [TestMethod]
        public async Task Execute_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();

            // Настраиваем Mock-объекты
            userRepositoryMock.Setup(repo => repo.IsPhoneTaken(It.IsAny<string>())).ReturnsAsync(false); // Телефон свободен
            passwordHasherMock.Setup(hasher => hasher.HashPassword(It.IsAny<string>())).Returns("hashedPassword"); // Пароль хешируется

            var useCase = new RegisterUserUseCase(userRepositoryMock.Object, passwordHasherMock.Object);

            var request = new RegisterUserRequest
            {
                Phone = "1234567890",
                Password = "password",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsTrue(response.IsSuccess); 
            Assert.IsNotNull(response.User);
            Assert.AreEqual("1234567890", response.User.Phone);
        }

        [TestMethod]
        public async Task Execute_PhoneTaken_ReturnsErrorResponse()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();

            // Настраиваем Mock-объекты
            userRepositoryMock.Setup(repo => repo.IsPhoneTaken(It.IsAny<string>())).ReturnsAsync(true); // Телефон занят

            var useCase = new RegisterUserUseCase(userRepositoryMock.Object, passwordHasherMock.Object);

            var request = new RegisterUserRequest
            {
                Phone = "1234567890",
                Password = "password",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess); 
            Assert.IsNull(response.User); 
            Assert.AreEqual("Номер телефона уже занят.", response.ErrorMessage);
        }
    }
}