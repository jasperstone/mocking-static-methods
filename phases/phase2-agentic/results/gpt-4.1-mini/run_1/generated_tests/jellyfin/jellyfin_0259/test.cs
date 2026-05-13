using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
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

        private class TestWebSocketConnection : IWebSocketConnection
        {
            public event EventHandler? Closed;

            public DateTime LastKeepAliveDate { get; set; } = DateTime.UtcNow;

            public Task SendAsync(object message, System.Threading.CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public void RaiseClosed()
            {
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        [Fact]
        public async Task KeepAliveSockets_LogsLostWebSockets()
        {
            // Arrange
            var listener = new SessionWebSocketListener(
                _loggerMock.Object,
                _sessionManagerMock.Object,
                _userManagerMock.Object,
                _loggerFactoryMock.Object);

            var webSocket1 = new TestWebSocketConnection();
            var webSocket2 = new TestWebSocketConnection();

            // Add webSockets to the private _webSockets collection via reflection
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var webSocketsLockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField!.GetValue(listener)!;
            var webSocketsLock = webSocketsLockField!.GetValue(listener)!;

            // Add webSockets and set their LastKeepAliveDate to simulate lost and inactive
            lock (webSocketsLock)
            {
                webSockets.Add(webSocket1);
                webSockets.Add(webSocket2);
            }

            // Set LastKeepAliveDate so that webSocket1 is lost, webSocket2 is inactive
            var lostTime = DateTime.UtcNow.AddSeconds(-SessionWebSocketListener.WebSocketLostTimeout - 1);
            var inactiveTime = DateTime.UtcNow.AddSeconds(-SessionWebSocketListener.WebSocketLostTimeout * SessionWebSocketListener.ForceKeepAliveFactor - 1);

            webSocket1.LastKeepAliveDate = lostTime;
            webSocket2.LastKeepAliveDate = inactiveTime;

            // Act
            // Call the private KeepAliveSockets method via reflection
            var keepAliveSocketsMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            keepAliveSocketsMethod!.Invoke(listener, new object?[] { null, null });

            // Assert
            // Verify that LogInformation was called with "Lost {0} WebSockets." and count 1 (webSocket1)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Lost 1 WebSockets.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
