using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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

        var webSocketMock1 = new Mock<IWebSocketConnection>();
        webSocketMock1.Setup(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-61)); // Lost
        var webSocketMock2 = new Mock<IWebSocketConnection>();
        webSocketMock2.Setup(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-45)); // Inactive

        var webSockets = new HashSet<IWebSocketConnection> { webSocketMock1.Object, webSocketMock2.Object };

        var sessionWebSocketListener = new SessionWebSocketListener(
            loggerMock.Object,
            sessionManagerMock.Object,
            userManagerMock.Object,
            loggerFactoryMock.Object);

        // Inject the webSockets field via reflection for testing purposes
        var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        webSocketsField.SetValue(sessionWebSocketListener, webSockets);

        // Act
        sessionWebSocketListener.KeepAliveSockets(null, null);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Lost 1 WebSockets.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
