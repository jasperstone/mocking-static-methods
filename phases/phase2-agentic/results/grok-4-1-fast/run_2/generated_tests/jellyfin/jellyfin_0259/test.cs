using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Session
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

            // Add websocket to the internal collection by calling KeepAliveWebSocket
            await _listener.KeepAliveWebSocket(mockWebSocket.Object);

            // Trigger KeepAliveSockets by invoking it directly
            _listener.GetType()
                .GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(_listener, new object?[] { null, EventArgs.Empty });

            // Act & Assert
            _loggerMock.Verify(
                logger => logger.LogInformation("Lost {0} WebSockets.", 1),
                Times.Once());
        }

        [Fact]
        public void KeepAliveSockets_DoesNotLogLostWebSockets_WhenNoLostWebSockets()
        {
            // Arrange
            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(ws => ws.LastKeepAliveDate)
                .Returns(DateTime.UtcNow); // Recent activity

            await _listener.KeepAliveWebSocket(mockWebSocket.Object);

            _listener.GetType()
                .GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(_listener, new object?[] { null, EventArgs.Empty });

            // Act & Assert
            _loggerMock.Verify(
                logger => logger.LogInformation("Lost {0} WebSockets.", It.IsAny<int>()),
                Times.Never());
        }

        [Fact]
        public async Task KeepAliveSockets_LogsInactiveWebSockets_WhenInactiveCountGreaterThanZero()
        {
            // Arrange
            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(ws => ws.LastKeepAliveDate)
                .Returns(DateTime.UtcNow.AddSeconds(-50)); // Between 45s (0.75*60) and 60s

            await _listener.KeepAliveWebSocket(mockWebSocket.Object);

            _listener.GetType()
                .GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(_listener, new object?[] { null, EventArgs.Empty });

            // Act & Assert
            _loggerMock.Verify(
                logger => logger.LogInformation("Sending ForceKeepAlive message to {0} inactive WebSockets.", 1),
                Times.Once());
        }
    }
}
