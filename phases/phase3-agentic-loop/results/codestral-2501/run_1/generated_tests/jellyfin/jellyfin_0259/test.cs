using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        var webSocketMock2 = new Mock<IWebSocketConnection>();

        webSocketMock1.Setup(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-70));
        webSocketMock2.Setup(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-50));

        var webSockets = new HashSet<IWebSocketConnection> { webSocketMock1.Object, webSocketMock2.Object };

        var sessionWebSocketListener = new SessionWebSocketListener(
            loggerMock.Object,
            sessionManagerMock.Object,
            userManagerMock.Object,
            loggerFactoryMock.Object);

        var privateObject = new PrivateObject(sessionWebSocketListener);
        privateObject.SetFieldOrProperty("_webSockets", webSockets);

        // Act
        var methodInfo = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)methodInfo.Invoke(sessionWebSocketListener, new object[] { null, null });

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
