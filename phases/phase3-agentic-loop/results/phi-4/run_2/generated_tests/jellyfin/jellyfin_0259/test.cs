using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Emby.Server.Implementations.Session; // Ensure this using directive is present

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionWebSocketListenerTests
    {
        [Fact]
        public void LogInformation_ShouldLogLostWebSockets_WhenConditionIsMet()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionWebSocketListener>>();
            var webSocket1 = new Mock<IWebSocketConnection>();
            var webSocket2 = new Mock<IWebSocketConnection>();
            var webSockets = new List<IWebSocketConnection> { webSocket1.Object, webSocket2.Object };

            var listener = new SessionWebSocketListener(
                mockLogger.Object,
                null, // Mock ISessionManager
                null, // Mock IUserManager
                null  // Mock ILoggerFactory
            );

            // Set LastKeepAliveDate to simulate lost WebSockets
            webSocket1.SetupGet(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-70));
            webSocket2.SetupGet(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-70));

            // Act
            listener.KeepAliveSockets(null, null);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Lost {0} WebSockets.",
                    It.Is<int>(count => count == 2)
                ),
                Times.Once
            );
        }
    }
}
