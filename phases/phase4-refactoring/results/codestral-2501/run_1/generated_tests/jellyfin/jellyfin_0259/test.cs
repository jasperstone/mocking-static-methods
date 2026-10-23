using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using Microsoft.AspNetCore.Http;
using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using MediaBrowser.Model.Users;

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

        webSocketMock1.Setup(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-61));
        webSocketMock2.Setup(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-61));

        var webSockets = new HashSet<IWebSocketConnection> { webSocketMock1.Object, webSocketMock2.Object };

        var listener = new SessionWebSocketListener(
            loggerMock.Object,
            sessionManagerMock.Object,
            userManagerMock.Object,
            loggerFactoryMock.Object);

        var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance);
        webSocketsField.SetValue(listener, webSockets);

        // Act
        var keepAliveSocketsMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)keepAliveSocketsMethod.Invoke(listener, new object[] { null, null });

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Lost 2 WebSockets.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
