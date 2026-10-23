using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Tests
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
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var sessionWebSocketListener = new SessionWebSocketListener(loggerMock.Object, sessionManagerMock.Object, userManagerMock.Object, loggerFactoryMock.Object);

            var webSocket1 = new Mock<IWebSocketConnection>();
            webSocket1.SetupGet(x => x.LastKeepAliveDate).Returns(DateTime.UtcNow - TimeSpan.FromSeconds(61));

            var webSocket2 = new Mock<IWebSocketConnection>();
            webSocket2.SetupGet(x => x.LastKeepAliveDate).Returns(DateTime.UtcNow - TimeSpan.FromSeconds(61));

            sessionWebSocketListener._webSockets.Add(webSocket1.Object);
            sessionWebSocketListener._webSockets.Add(webSocket2.Object);

            // Act
            sessionWebSocketListener.KeepAliveSockets(null, null);

            // Assert
            loggerMock.Verify(x => x.LogInformation("Lost {0} WebSockets.", 2), Times.Once);
        }
    }
}
