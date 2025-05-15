using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Jmgram_mk1.src.JMgram.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
namespace Jmgram_mk1.tests.JMgram.Tests.UseCases
{
    [TestClass]
    public class ChangePasswordUseCaseTests
    {
        [TestMethod]
        public async Task Execute_ValidRequest_ChangesPasswordSuccessfully()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<AppIdentityUser>>(
                Mock.Of<IUserStore<AppIdentityUser>>(), null, null, null, null, null, null, null, null);
            var loggerMock = new Mock<ILogger<ChangePasswordUseCase>>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Настраиваем HttpContextAccessor
            var httpContextMock = new Mock<HttpContext>();
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(cp => cp.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, "testUserId")); // Укажите значение для UserId
            httpContextMock.Setup(hc => hc.User).Returns(claimsPrincipalMock.Object);
            httpContextAccessorMock.Setup(hca => hca.HttpContext).Returns(httpContextMock.Object);

            var useCase = new ChangePasswordUseCase(userManagerMock.Object, loggerMock.Object, httpContextAccessorMock.Object); // Добавляем httpContextAccessorMock

            var request = new ChangePasswordRequest { OldPassword = "oldPassword", NewPassword = "newPassword" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.User);
            userManagerMock.Verify(um => um.ChangePasswordAsync(It.IsAny<AppIdentityUser>(), "oldPassword", "newPassword"), Times.Once);
        }
        [TestMethod]
        public async Task Execute_Unauthorized_ReturnsErrorResponse()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<AppIdentityUser>>(
                Mock.Of<IUserStore<AppIdentityUser>>(), null, null, null, null, null, null, null, null);
            var loggerMock = new Mock<ILogger<ChangePasswordUseCase>>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // HttpContextAccessor не настроен, UserId не будет найден
            httpContextAccessorMock.Setup(hca => hca.HttpContext).Returns((HttpContext)null);

            var useCase = new ChangePasswordUseCase(userManagerMock.Object, loggerMock.Object, httpContextAccessorMock.Object);

            var request = new ChangePasswordRequest { OldPassword = "oldPassword", NewPassword = "newPassword" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.User);
            Assert.AreEqual("Unauthorized", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_UserNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<AppIdentityUser>>(
                Mock.Of<IUserStore<AppIdentityUser>>(), null, null, null, null, null, null, null, null);
            var loggerMock = new Mock<ILogger<ChangePasswordUseCase>>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var userId = "testUserId";

            // Настраиваем UserManager
            userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync((AppIdentityUser)null); // Пользователь не найден

            // Настраиваем HttpContextAccessor
            var httpContextMock = new Mock<HttpContext>();
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(cp => cp.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, userId));
            httpContextMock.Setup(hc => hc.User).Returns(claimsPrincipalMock.Object);
            httpContextAccessorMock.Setup(hca => hca.HttpContext).Returns(httpContextMock.Object);

            var useCase = new ChangePasswordUseCase(userManagerMock.Object, loggerMock.Object, httpContextAccessorMock.Object);

            var request = new ChangePasswordRequest { OldPassword = "oldPassword", NewPassword = "newPassword" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.User);
            Assert.AreEqual("User not found", response.ErrorMessage);
        }

        [TestMethod]
        public async Task Execute_InvalidOldPassword_ReturnsErrorResponse()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<AppIdentityUser>>(
                Mock.Of<IUserStore<AppIdentityUser>>(), null, null, null, null, null, null, null, null);
            var loggerMock = new Mock<ILogger<ChangePasswordUseCase>>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var userId = "testUserId";
            var user = new AppIdentityUser { Id = userId, PhoneNumber = "1234567890" };

            // Настраиваем UserManager
            userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.CheckPasswordAsync(user, "oldPassword")).ReturnsAsync(false); // Неверный пароль

            // Настраиваем HttpContextAccessor
            var httpContextMock = new Mock<HttpContext>();
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(cp => cp.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, userId));
            httpContextMock.Setup(hc => hc.User).Returns(claimsPrincipalMock.Object);
            httpContextAccessorMock.Setup(hca => hca.HttpContext).Returns(httpContextMock.Object);

            var useCase = new ChangePasswordUseCase(userManagerMock.Object, loggerMock.Object, httpContextAccessorMock.Object);

            var request = new ChangePasswordRequest { OldPassword = "oldPassword", NewPassword = "newPassword" };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.User);
            Assert.AreEqual("Invalid old password.", response.ErrorMessage);
        }
    }
}
