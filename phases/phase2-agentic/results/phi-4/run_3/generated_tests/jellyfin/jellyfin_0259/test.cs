using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Tests.Session
{
    public class SessionWebSocketListenerTests
    {
        [Fact]
        public async Task LogInformationCalledWhenWebSocketsAreLost()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionWebSocketListener>>();
            var mockSessionManager = Mock.Of<ISessionManager>();
            var mockUserManager = Mock.Of<IUserManager>();
            var mockLoggerFactory = Mock.Of<ILoggerFactory>();

            var listener = new SessionWebSocketListener(
                mockLogger.Object,
                mockSessionManager,
                mockUserManager,
                mockLoggerFactory);

            var webSocket1 = new Mock<IWebSocketConnection>();
            webSocket1.SetupGet(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-70));

            var webSocket2 = new Mock<IWebSocketConnection>();
            webSocket2.SetupGet(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-70));

            lock (listener._webSocketsLock)
            {
                listener._webSockets.Add(webSocket1.Object);
                listener._webSockets.Add(webSocket2.Object);
            }

            // Act
            listener.KeepAliveSockets(null, null);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Lost 2 WebSockets.")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
