using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using MediaBrowser.Controller.Net;
using System.Threading.Tasks;
using MediaBrowser.Controller.Session;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionWebSocketListenerTests
    {
        [Fact]
        public async Task KeepAliveSockets_LogsLostWebSockets()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var userManagerMock = new Mock<IUserManager>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var webSocketMock = new Mock<IWebSocketConnection>();

            var sessionWebSocketListener = new SessionWebSocketListener(
                loggerMock.Object,
                sessionManagerMock.Object,
                userManagerMock.Object,
                loggerFactoryMock.Object);

            webSocketMock.SetupGet(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-61));

            sessionWebSocketListener._webSockets.Add(webSocketMock.Object);

            // Act
            sessionWebSocketListener.KeepAliveSockets(null, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Lost {0} WebSockets.", 1), Times.Once);
        }
    }
}
