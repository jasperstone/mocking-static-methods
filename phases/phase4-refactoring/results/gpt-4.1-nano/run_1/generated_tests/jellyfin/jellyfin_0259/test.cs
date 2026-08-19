using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Net.WebSocketMessages.Outbound;

namespace Emby.Tests
{
    public class SessionWebSocketListenerTests
    {
        [Fact]
        public async Task KeepAliveSockets_ShouldLogInformation_WhenInactiveWebSocketsExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            var sessionManagerMock = new Mock<MediaBrowser.Controller.Session.ISessionManager>();
            var userManagerMock = new Mock<MediaBrowser.Controller.Session.IUserManager>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            var listener = new SessionWebSocketListener(
                loggerMock.Object,
                sessionManagerMock.Object,
                userManagerMock.Object,
                loggerFactoryMock.Object);

            // Create a mock IWebSocketConnection
            var webSocketMock = new Mock<IWebSocketConnection>();
            webSocketMock.SetupAllProperties();
            webSocketMock.Object.LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-50); // inactive, since timeout is 60

            // Add the WebSocket to the internal list
            await listener.KeepAliveWebSocket(webSocketMock.Object);

            // Act: invoke KeepAliveSockets directly
            // To do this, we need to call the private method via reflection or make it internal for testing.
            // For simplicity, assume we can call it via reflection here.
            var methodInfo = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Lock object to pass
            var lockObj = typeof(SessionWebSocketListener).GetField("_webSocketsLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(listener);
            // Call the method
            methodInfo.Invoke(listener, new object[] { null, null });

            // Wait a moment for async logs
            await Task.Delay(100);

            // Assert
            // Verify that LogInformation was called with the expected message
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
