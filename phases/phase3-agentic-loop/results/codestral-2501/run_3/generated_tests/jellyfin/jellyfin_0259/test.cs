using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Authentication;
using MediaBrowser.Controller.Events.Session;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Session;
using MediaBrowser.Model.SyncPlay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Episode = MediaBrowser.Controller.Entities.TV.Episode;

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
        webSocketMock2.Setup(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-45)); // Inactive WebSocket

        var webSockets = new HashSet<IWebSocketConnection> { webSocketMock1.Object, webSocketMock2.Object };

        var sessionWebSocketListener = new SessionWebSocketListener(
            loggerMock.Object,
            sessionManagerMock.Object,
            userManagerMock.Object,
            loggerFactoryMock.Object);

        var sessionWebSocketListenerType = typeof(SessionWebSocketListener);
        var webSocketsField = sessionWebSocketListenerType.GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        webSocketsField.SetValue(sessionWebSocketListener, webSockets);

        // Act
        var keepAliveSocketsMethod = sessionWebSocketListenerType.GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        keepAliveSocketsMethod.Invoke(sessionWebSocketListener, new object[] { null, null });

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Lost 1 WebSockets.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
