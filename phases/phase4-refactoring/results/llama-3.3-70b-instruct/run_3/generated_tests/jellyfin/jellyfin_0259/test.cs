using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Tests
{
    public class SessionWebSocketListenerTests
    {
        private readonly Mock<ILogger<SessionWebSocketListener>> _loggerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;

        public SessionWebSocketListenerTests()
        {
            _loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
        }

        [Fact]
        public async Task KeepAliveSockets_LogsLostWebSockets()
        {
            // Arrange
            var sessionWebSocketListener = new SessionWebSocketListener(_loggerMock.Object, _sessionManagerMock.Object, null, _loggerFactoryMock.Object);
            var webSockets = new List<IWebSocketConnection>
            {
                new Mock<IWebSocketConnection>().Object,
                new Mock<IWebSocketConnection>().Object,
            };

            // Act
            await sessionWebSocketListener.KeepAliveSockets(null, null);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task KeepAliveSockets_LogsInactiveWebSockets()
        {
            // Arrange
            var sessionWebSocketListener = new SessionWebSocketListener(_loggerMock.Object, _sessionManagerMock.Object, null, _loggerFactoryMock.Object);
            var webSockets = new List<IWebSocketConnection>
            {
                new Mock<IWebSocketConnection>().Object,
                new Mock<IWebSocketConnection>().Object,
            };

            // Act
            await sessionWebSocketListener.KeepAliveSockets(null, null);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
