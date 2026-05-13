using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

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
        public async Task KeepAliveSockets_LogsLostWebSockets()
        {
            // Arrange
            var sessionWebSocketListener = new SessionWebSocketListener(_loggerMock.Object, _sessionManagerMock.Object, _userManagerMock.Object, _loggerFactoryMock.Object);
            var webSockets = new List<IWebSocketConnection>
            {
                new Mock<IWebSocketConnection>().Object,
                new Mock<IWebSocketConnection>().Object,
            };

            foreach (var webSocket in webSockets)
            {
                await sessionWebSocketListener.KeepAliveWebSocket(webSocket).ConfigureAwait(false);
            }

            // Simulate lost WebSockets
            webSockets[0].LastKeepAliveDate = DateTime.UtcNow - TimeSpan.FromSeconds(SessionWebSocketListener.WebSocketLostTimeout + 1);
            webSockets[1].LastKeepAliveDate = DateTime.UtcNow - TimeSpan.FromSeconds(SessionWebSocketListener.WebSocketLostTimeout + 1);

            // Act
            sessionWebSocketListener.KeepAliveSockets(null, null);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task KeepAliveSockets_LogsInactiveWebSockets()
        {
            // Arrange
            var sessionWebSocketListener = new SessionWebSocketListener(_loggerMock.Object, _sessionManagerMock.Object, _userManagerMock.Object, _loggerFactoryMock.Object);
            var webSockets = new List<IWebSocketConnection>
            {
                new Mock<IWebSocketConnection>().Object,
                new Mock<IWebSocketConnection>().Object,
            };

            foreach (var webSocket in webSockets)
            {
                await sessionWebSocketListener.KeepAliveWebSocket(webSocket).ConfigureAwait(false);
            }

            // Simulate inactive WebSockets
            webSockets[0].LastKeepAliveDate = DateTime.UtcNow - TimeSpan.FromSeconds(SessionWebSocketListener.WebSocketLostTimeout * SessionWebSocketListener.ForceKeepAliveFactor + 1);
            webSockets[1].LastKeepAliveDate = DateTime.UtcNow - TimeSpan.FromSeconds(SessionWebSocketListener.WebSocketLostTimeout * SessionWebSocketListener.ForceKeepAliveFactor + 1);

            // Act
            sessionWebSocketListener.KeepAliveSockets(null, null);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
