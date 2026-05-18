using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionWebSocketListenerTests
    {
        private readonly Mock<ILogger<SessionWebSocketListener>> _loggerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly SessionWebSocketListener _listener;

        public SessionWebSocketListenerTests()
        {
            _loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _userManagerMock = new Mock<IUserManager>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();

            _listener = new SessionWebSocketListener(
                _loggerMock.Object,
                _sessionManagerMock.Object,
                _userManagerMock.Object,
                _loggerFactoryMock.Object);
        }

        [Fact]
        public void KeepAliveSockets_LogsLostWebSockets_WhenLostCountGreaterThanZero()
        {
            // Arrange
            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(ws => ws.LastKeepAliveDate)
                .Returns(DateTime.UtcNow.AddSeconds(-70)); // > 60 seconds (WebSocketLostTimeout)

            // Set up private _webSockets field via reflection
            var webSocketsField = typeof(SessionWebSocketListener)
                .GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance);
            webSocketsField?.SetValue(_listener, new HashSet<IWebSocketConnection> { mockWebSocket.Object });

            var keepAliveSocketsMethod = typeof(SessionWebSocketListener)
                .GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            keepAliveSocketsMethod?.Invoke(_listener, new object[] { null, EventArgs.Empty });

            // Assert - Verify LogInformation call on line 246: "Lost {0} WebSockets."
            _loggerMock.Verify(
                x => x.LogInformation("Lost {0} WebSockets.", 1),
                Times.Once);
        }

        [Fact]
        public void KeepAliveSockets_LogsInactiveWebSockets_WhenInactiveCountGreaterThanZero()
        {
            // Arrange
            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(ws => ws.LastKeepAliveDate)
                .Returns(DateTime.UtcNow.AddSeconds(-50)); // Between 45s (0.75*60) and 60s

            var webSocketsField = typeof(SessionWebSocketListener)
                .GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance);
            webSocketsField?.SetValue(_listener, new HashSet<IWebSocketConnection> { mockWebSocket.Object });

            var keepAliveSocketsMethod = typeof(SessionWebSocketListener)
                .GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            keepAliveSocketsMethod?.Invoke(_listener, new object[] { null, EventArgs.Empty });

            // Assert - Verify inactive logging
            _loggerMock.Verify(
                x => x.LogInformation("Sending ForceKeepAlive message to {0} inactive WebSockets.", 1),
                Times.Once);
        }

        [Fact]
        public void KeepAliveSockets_DoesNotLogLostWebSockets_WhenNoLostWebSockets()
        {
            // Arrange
            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(ws => ws.LastKeepAliveDate)
                .Returns(DateTime.UtcNow.AddSeconds(-10)); // Recent activity

            var webSocketsField = typeof(SessionWebSocketListener)
                .GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance);
            webSocketsField?.SetValue(_listener, new HashSet<IWebSocketConnection> { mockWebSocket.Object });

            var keepAliveSocketsMethod = typeof(SessionWebSocketListener)
                .GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            keepAliveSocketsMethod?.Invoke(_listener, new object[] { null, EventArgs.Empty });

            // Assert - No "Lost" log message
            _loggerMock.Verify(
                x => x.LogInformation("Lost {0} WebSockets.", It.IsAny<int>()),
                Times.Never);
        }
    }
}
