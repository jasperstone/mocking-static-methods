using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionWebSocketListenerTests
    {
        [Fact]
        public void LogInformation_LogsMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            var sessionManagerMock = new Mock<MediaBrowser.Controller.Session.ISessionManager>();
            var userManagerMock = new Mock<MediaBrowser.Controller.Users.IUserManager>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var sessionWebSocketListener = new SessionWebSocketListener(loggerMock.Object, sessionManagerMock.Object, userManagerMock.Object, loggerFactoryMock.Object);

            // Act
            loggerMock.Object.LogInformation("Test message");

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<EventId>(), It.IsAny<LogLevel>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<ILogger, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void LogInformation_LogsMessageWithException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            var sessionManagerMock = new Mock<MediaBrowser.Controller.Session.ISessionManager>();
            var userManagerMock = new Mock<MediaBrowser.Controller.Users.IUserManager>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var sessionWebSocketListener = new SessionWebSocketListener(loggerMock.Object, sessionManagerMock.Object, userManagerMock.Object, loggerFactoryMock.Object);
            var exception = new Exception("Test exception");

            // Act
            loggerMock.Object.LogInformation(exception, "Test message");

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<EventId>(), It.IsAny<LogLevel>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<ILogger, Exception, string>>()), Times.Once);
        }
    }
}
