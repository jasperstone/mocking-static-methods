using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Api.Extensions;
using Jellyfin.Api.Helpers;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Net.WebSocketMessages.Outbound;
using MediaBrowser.Controller.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionWebSocketListenerTests
    {
        [Fact]
        public void LogInformation_ShouldBeCalled_WhenWebSocketsAreLost()
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

            // Use an object to lock on
            object lockObject = listener._webSocketsLock;

            lock (lockObject)
            {
                listener._webSockets.Add(webSocket1.Object);
                listener._webSockets.Add(webSocket2.Object);
            }

            // Act
            listener.KeepAliveSockets(null, null);

            // Assert
            mockLogger.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Lost 2 WebSockets."))),
                Times.Once);
        }
    }
}
