using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Net;
using Emby.Server.Implementations.Session;

namespace Emby.Tests
{
    public class SessionWebSocketListenerTests
    {
        [Fact]
        public async Task KeepAliveSockets_ShouldLogInformation_WhenInactiveWebSocketsExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<WebSocketController>()).Returns(Mock.Of<ILogger<WebSocketController>>());

            var listener = new SessionWebSocketListener(
                loggerMock.Object,
                null, // sessionManager
                null, // userManager
                loggerFactoryMock.Object);

            // Create mock IWebSocketConnection objects
            var webSocketMock1 = new Mock<IWebSocketConnection>();
            var webSocketMock2 = new Mock<IWebSocketConnection>();

            // Set LastKeepAliveDate to simulate inactivity
            var now = DateTime.UtcNow;
            webSocketMock1.SetupGet(w => w.LastKeepAliveDate).Returns(now.AddSeconds(-70));
            webSocketMock2.SetupGet(w => w.LastKeepAliveDate).Returns(now.AddSeconds(-80));

            // Access private fields via reflection
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var lockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var webSockets = new HashSet<IWebSocketConnection> { webSocketMock1.Object, webSocketMock2.Object };
            webSocketsField.SetValue(listener, webSockets);
            var lockObj = new object();
            lockField.SetValue(listener, lockObj);

            // Invoke KeepAliveSockets via reflection
            var method = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method.Invoke(listener, new object[] { null, null });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending ForceKeepAlive message to")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
