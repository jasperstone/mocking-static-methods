using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Session;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionWebSocketListenerTests
    {
        private readonly Mock<ILogger<SessionWebSocketListener>> _loggerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;

        public SessionWebSocketListenerTests()
        {
            _loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _userManagerMock = new Mock<IUserManager>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
        }

        [Fact]
        public void KeepAliveSockets_LogsLostWebSockets_WhenLostCountGreaterThanZero()
        {
            // Arrange
            var listener = new SessionWebSocketListener(
                _loggerMock.Object,
                _sessionManagerMock.Object,
                _userManagerMock.Object,
                _loggerFactoryMock.Object);

            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-70)); // > 60s timeout

            var webSocketsField = typeof(SessionWebSocketListener)
                .GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var webSocketsLockField = typeof(SessionWebSocketListener)
                .GetField("_webSocketsLock", BindingFlags.NonPublic | BindingFlags.Instance)!;

            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField.GetValue(listener)!;
            var webSocketsLock = webSocketsLockField.GetValue(listener)!;

            lock (webSocketsLock)
            {
                webSockets.Add(mockWebSocket.Object);
            }

            // Act
            typeof(SessionWebSocketListener)
                .GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(listener, new object[] { null, EventArgs.Empty });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Lost 1 WebSockets.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void KeepAliveSockets_LogsInactiveWebSockets_WhenInactiveCountGreaterThanZero()
        {
            // Arrange
            var listener = new SessionWebSocketListener(
                _loggerMock.Object,
                _sessionManagerMock.Object,
                _userManagerMock.Object,
                _loggerFactoryMock.Object);

            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-50)); // 45s-60s range

            var webSocketsField = typeof(SessionWebSocketListener)
                .GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var webSocketsLockField = typeof(SessionWebSocketListener)
                .GetField("_webSocketsLock", BindingFlags.NonPublic | BindingFlags.Instance)!;

            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField.GetValue(listener)!;
            var webSocketsLock = webSocketsLockField.GetValue(listener)!;

            lock (webSocketsLock)
            {
                webSockets.Add(mockWebSocket.Object);
            }

            // Act
            typeof(SessionWebSocketListener)
                .GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(listener, new object[] { null, EventArgs.Empty });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending ForceKeepAlive message to 1 inactive WebSockets.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
