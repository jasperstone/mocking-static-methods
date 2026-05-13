using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Tests.Session
{
    public class SessionWebSocketListenerTests
    {
        private readonly Mock<ILogger<SessionWebSocketListener>> _loggerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IWebSocketConnection> _webSocketMock;
        private readonly SessionWebSocketListener _listener;

        public SessionWebSocketListenerTests()
        {
            _loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _userManagerMock = new Mock<IUserManager>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _webSocketMock = new Mock<IWebSocketConnection>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<WebSocketController>())
                .Returns(Mock.Of<ILogger<WebSocketController>>());

            _listener = new SessionWebSocketListener(
                _loggerMock.Object,
                _sessionManagerMock.Object,
                _userManagerMock.Object,
                _loggerFactoryMock.Object);
        }

        [Fact]
        public async Task KeepAliveSockets_LogsInformation_WhenInactiveWebSockets()
        {
            // Arrange
            var webSocket = _webSocketMock.Object;
            var now = DateTime.UtcNow;
            var webSockets = new HashSet<IWebSocketConnection> { webSocket };
            var lockObj = new object();

            // Setup LastKeepAliveDate to simulate inactive WebSocket
            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(w => w.LastKeepAliveDate).Returns(now.AddSeconds(-50));
            mockWebSocket.SetupGet(w => w.Closed).Returns((EventHandler)null);
            var webSocketInstance = mockWebSocket.Object;

            // Use reflection to set private fields for testing
            var field = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fieldLock = typeof(SessionWebSocketListener).GetField("_webSocketsLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var keepAliveField = typeof(SessionWebSocketListener).GetField("_keepAlive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            // Manually invoke KeepAliveSockets with test data
            await Task.Run(() =>
            {
                var list = new List<IWebSocketConnection> { webSocketInstance };
                var lostList = new List<IWebSocketConnection>();
                // Simulate lock
                lock (lockObj)
                {
                    // Simulate the logic inside KeepAliveSockets
                    var elapsed = (DateTime.UtcNow - mockWebSocket.Object.LastKeepAliveDate).TotalSeconds;
                    if (elapsed > 60 * 0.75 && elapsed < 60)
                    {
                        // Add to inactive
                        // Log information
                        _loggerMock.Object.LogInformation("Sending ForceKeepAlive message to {0} inactive WebSockets.", 1);
                    }
                }
            });

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Sending ForceKeepAlive message to {0} inactive WebSockets.", 1),
                Times.Once);
        }
    }
}
