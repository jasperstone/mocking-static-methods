using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionWebSocketListenerTests
    {
        private readonly Mock<ISessionManager> _mockSessionManager;
        private readonly Mock<IUserManager> _mockUserManager;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;

        public SessionWebSocketListenerTests()
        {
            _mockSessionManager = new Mock<ISessionManager>();
            _mockUserManager = new Mock<IUserManager>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
        }

        [Fact]
        public void KeepAliveSockets_LogsLostWebSockets_WhenLostCountGreaterThanZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionWebSocketListener>>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Lost 1 WebSockets.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var listener = new SessionWebSocketListener(
                mockLogger.Object,
                _mockSessionManager.Object,
                _mockUserManager.Object,
                _mockLoggerFactory.Object);

            // Create a mock WebSocket that appears lost (LastKeepAliveDate > 60 seconds ago)
            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(x => x.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-70));

            // Add WebSocket to private field using reflection
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField.GetValue(listener)!;
            webSockets.Add(mockWebSocket.Object);

            // Get the private method
            var method = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            method.Invoke(listener, new object?[] { null, null });

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Lost 1 WebSockets.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void KeepAliveSockets_LogsInactiveWebSockets_WhenInactiveCountGreaterThanZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionWebSocketListener>>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending ForceKeepAlive message to 1 inactive WebSockets.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var listener = new SessionWebSocketListener(
                mockLogger.Object,
                _mockSessionManager.Object,
                _mockUserManager.Object,
                _mockLoggerFactory.Object);

            // Create a mock WebSocket that appears inactive (45-60 seconds elapsed)
            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(x => x.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-50));

            // Add WebSocket to private field using reflection
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField.GetValue(listener)!;
            webSockets.Add(mockWebSocket.Object);

            // Get the private method
            var method = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            method.Invoke(listener, new object?[] { null, null });

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending ForceKeepAlive message to 1 inactive WebSockets.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
