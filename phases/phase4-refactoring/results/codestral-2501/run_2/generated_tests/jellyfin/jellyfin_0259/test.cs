using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using Emby.Server.Implementations.Session;
using MediaBrowser.Model.Session;

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
        webSocketMock1.Setup(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-61)); // Lost WebSocket

        var webSocketMock2 = new Mock<IWebSocketConnection>();
        webSocketMock2.Setup(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-59)); // Inactive WebSocket

        var webSockets = new HashSet<IWebSocketConnection> { webSocketMock1.Object, webSocketMock2.Object };

        var listener = new SessionWebSocketListener(
            loggerMock.Object,
            sessionManagerMock.Object,
            userManagerMock.Object,
            loggerFactoryMock.Object);

        // Inject the webSockets field via reflection
        var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        webSocketsField.SetValue(listener, webSockets);

        // Act
        await listener.KeepAliveSockets(null, null);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation("Lost {0} WebSockets.", It.IsAny<object[]>()),
            Times.Once);
    }
}
