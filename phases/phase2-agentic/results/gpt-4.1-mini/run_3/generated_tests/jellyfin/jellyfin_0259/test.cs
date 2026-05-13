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

            public DateTime LastKeepAliveDate { get; set; }

            public bool SendAsyncCalled { get; private set; }

            public Task SendAsync(object message, System.Threading.CancellationToken cancellationToken)
            {
                SendAsyncCalled = true;
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

            var webSocket1 = new Mock<IWebSocketConnection>();
            var webSocket2 = new Mock<IWebSocketConnection>();

            // Setup LastKeepAliveDate to simulate lost and inactive sockets
            var now = DateTime.UtcNow;
            // webSocket1 is inactive (between ForceKeepAliveFactor * timeout and timeout)
            webSocket1.SetupGet(ws => ws.LastKeepAliveDate).Returns(now.AddSeconds(-45)); // 60 * 0.75 = 45
            // webSocket2 is lost (>= timeout)
            webSocket2.SetupGet(ws => ws.LastKeepAliveDate).Returns(now.AddSeconds(-61));

            // Add webSockets to the private _webSockets collection via reflection
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var webSocketsLockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var webSocketsLock = webSocketsLockField!.GetValue(listener);

            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField!.GetValue(listener)!;

            // Lock and add sockets
            var lockType = webSocketsLock!.GetType();
            var lockObj = webSocketsLock;

            lock (lockObj)
            {
                webSockets.Add(webSocket1.Object);
                webSockets.Add(webSocket2.Object);
            }

            // Act
            // Call the private KeepAliveSockets method via reflection
            var method = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Call with null args
            method!.Invoke(listener, new object?[] { null, null });

            // Assert
            // Verify that LogInformation was called with "Lost {0} WebSockets." and count 1 (webSocket2)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Lost 1 WebSockets.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
