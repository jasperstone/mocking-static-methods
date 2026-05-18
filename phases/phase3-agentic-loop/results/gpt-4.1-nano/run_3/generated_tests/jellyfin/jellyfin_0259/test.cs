using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Net.WebSocketMessages.Outbound;

namespace Emby.Tests.Session
{
    public class SessionWebSocketListenerTests
    {
        private readonly Mock<ILogger<SessionWebSocketListener>> _loggerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;

        public SessionWebSocketListenerTests()
        {
            _loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _userManagerMock = new Mock<IUserManager>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
        }

        [Fact]
        public async Task KeepAliveSockets_ShouldLogInformation_WhenInactiveWebSockets()
        {
            // Arrange
            var listener = new SessionWebSocketListener(
                _loggerMock.Object,
                _sessionManagerMock.Object,
                _userManagerMock.Object,
                _loggerFactoryMock.Object);

            var mockWebSocket1 = new Mock<IWebSocketConnection>();
            var mockWebSocket2 = new Mock<IWebSocketConnection>();

            var webSockets = new HashSet<IWebSocketConnection> { mockWebSocket1.Object, mockWebSocket2.Object };

            // Setup LastKeepAliveDate to simulate inactive WebSockets
            var now = DateTime.UtcNow;
            mockWebSocket1.Setup(w => w.LastKeepAliveDate).Returns(now.AddSeconds(-70));
            mockWebSocket2.Setup(w => w.LastKeepAliveDate).Returns(now.AddSeconds(-80));

            // Use reflection to set the private _webSockets field
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            webSocketsField.SetValue(listener, webSockets);

            // Setup lock object
            var lockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var lockObj = new object();
            lockField.SetValue(listener, lockObj);

            // Act
            await typeof(SessionWebSocketListener)
                .GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(listener, new object[] { null, null }) as Task;

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Sending ForceKeepAlive message to {0} inactive WebSockets.", 2),
                Times.Once);
        }
    }
}
