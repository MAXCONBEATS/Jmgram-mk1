using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.tests.JMgram.Tests.UseCases
{
    [TestClass]
    public class SendNotificationUseCaseTests
    {
        [TestMethod]
        public async Task Execute_ValidSystemNotificationRequest_SendsNotificationSuccessfully()
        {
            // Arrange
            var notificationRepositoryMock = new Mock<INotificationRepository>();

            // Setup the mock repository
            notificationRepositoryMock.Setup(repo => repo.AddNotification(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask); // Just complete the task

            var useCase = new SendNotificationUseCase(notificationRepositoryMock.Object);

            var request = new SendNotificationRequest
            {
                Notification = new SystemNotificationDto
                {
                    UserId = 1,
                    Message = "Test system notification",
                    NotificationType = NotificationType.System,
                    Source = "Test Source"
                }
            };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsTrue(response.IsSuccess, $"IsSuccess should be true, but was {response.IsSuccess}. Error message: {response.ErrorMessage}");
            Assert.IsNotNull(response.Notification);
            Assert.AreEqual(1, response.Notification.UserId);
            Assert.AreEqual("Test system notification", response.Notification.Message);
            notificationRepositoryMock.Verify(repo => repo.AddNotification(It.IsAny<Notification>()), Times.Once);
        }

        [TestMethod]
        public async Task Execute_NullRequest_ReturnsError()
        {
            // Arrange
            var notificationRepositoryMock = new Mock<INotificationRepository>();
            var useCase = new SendNotificationUseCase(notificationRepositoryMock.Object);

            // Act
            var response = await useCase.Execute(null);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("Request cannot be null.", response.ErrorMessage);
            Assert.IsNull(response.Notification);
            notificationRepositoryMock.Verify(repo => repo.AddNotification(It.IsAny<Notification>()), Times.Never);
        }

        [TestMethod]
        public async Task Execute_NullNotification_ReturnsError()
        {
            // Arrange
            var notificationRepositoryMock = new Mock<INotificationRepository>();
            var useCase = new SendNotificationUseCase(notificationRepositoryMock.Object);

            var request = new SendNotificationRequest { Notification = null };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("Notification in request cannot be null.", response.ErrorMessage);
            Assert.IsNull(response.Notification);
            notificationRepositoryMock.Verify(repo => repo.AddNotification(It.IsAny<Notification>()), Times.Never);
        }

        [TestMethod]
        public async Task Execute_InvalidUserId_ReturnsError()
        {
            // Arrange
            var notificationRepositoryMock = new Mock<INotificationRepository>();
            var useCase = new SendNotificationUseCase(notificationRepositoryMock.Object);

            var request = new SendNotificationRequest
            {
                Notification = new NotificationDto
                {
                    UserId = 0, // Invalid UserId
                    Message = "Test notification",
                    NotificationType = NotificationType.System
                }
            };

            // Act
            var response = await useCase.Execute(request);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("UserId must be greater than 0.", response.ErrorMessage);
            Assert.IsNull(response.Notification);
            notificationRepositoryMock.Verify(repo => repo.AddNotification(It.IsAny<Notification>()), Times.Never);
        }
    }
}