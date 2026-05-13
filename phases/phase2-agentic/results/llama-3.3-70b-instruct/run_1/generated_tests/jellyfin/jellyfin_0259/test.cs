using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Net;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
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
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var webSocket1 = new Mock<IWebSocketConnection>();
            webSocket1.Setup(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-61));

            var webSocket2 = new Mock<IWebSocketConnection>();
            webSocket2.Setup(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-61));

            var sessionWebSocketListener = new SessionWebSocketListener(loggerMock.Object, sessionManagerMock.Object, userManagerMock.Object, loggerFactoryMock.Object);
            sessionWebSocketListener._webSockets.Add(webSocket1.Object);
            sessionWebSocketListener._webSockets.Add(webSocket2.Object);

            // Act
            await sessionWebSocketListener.KeepAliveSockets(null, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Lost {0} WebSockets.", 2), Times.Once);
        }

        [Fact]
        public async Task KeepAliveSockets_LogsInactiveWebSockets()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var userManagerMock = new Mock<IUserManager>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var webSocket1 = new Mock<IWebSocketConnection>();
            webSocket1.Setup(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-45));

            var webSocket2 = new Mock<IWebSocketConnection>();
            webSocket2.Setup(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-45));

            var sessionWebSocketListener = new SessionWebSocketListener(loggerMock.Object, sessionManagerMock.Object, userManagerMock.Object, loggerFactoryMock.Object);
            sessionWebSocketListener._webSockets.Add(webSocket1.Object);
            sessionWebSocketListener._webSockets.Add(webSocket2.Object);

            // Act
            await sessionWebSocketListener.KeepAliveSockets(null, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Sending ForceKeepAlive message to {0} inactive WebSockets.", 2), Times.Once);
        }

        [Fact]
        public async Task KeepAliveSockets_SendsForceKeepAlive()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var userManagerMock = new Mock<IUserManager>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var webSocket1 = new Mock<IWebSocketConnection>();
            webSocket1.Setup(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-45));

            var webSocket2 = new Mock<IWebSocketConnection>();
            webSocket2.Setup(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-45));

            var sessionWebSocketListener = new SessionWebSocketListener(loggerMock.Object, sessionManagerMock.Object, userManagerMock.Object, loggerFactoryMock.Object);
            sessionWebSocketListener._webSockets.Add(webSocket1.Object);
            sessionWebSocketListener._webSockets.Add(webSocket2.Object);

            // Act
            await sessionWebSocketListener.KeepAliveSockets(null, null);

            // Assert
            webSocket1.Verify(w => w.SendAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
            webSocket2.Verify(w => w.SendAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
