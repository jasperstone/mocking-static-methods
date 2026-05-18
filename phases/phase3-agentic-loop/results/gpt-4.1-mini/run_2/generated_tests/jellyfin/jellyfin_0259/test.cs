using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    // Minimal interface mocks to allow compilation
    public interface ISessionManager
    {
        void OnSessionControllerConnected(object session);
    }

    public interface IUserManager
    {
    }

    public interface IWebSocketConnection
    {
        event EventHandler? Closed;
        DateTime LastKeepAliveDate { get; set; }
        Task SendAsync(object message, CancellationToken cancellationToken);
    }

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
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
            _loggerFactoryMock.Setup(f => f.CreateLogger<WebSocketController>()).Returns(Mock.Of<ILogger<WebSocketController>>());
        }

        private class TestWebSocketConnection : IWebSocketConnection
        {
            public event EventHandler? Closed;

            public DateTime LastKeepAliveDate { get; set; }

            public bool SendAsyncCalled { get; private set; }
            public bool ThrowOnSend { get; set; }

            public Task SendAsync(object message, CancellationToken cancellationToken)
            {
                SendAsyncCalled = true;
                if (ThrowOnSend)
                {
                    throw new WebSocketException("Send failed");
                }
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

            // Set LastKeepAliveDate to simulate lost and inactive sockets
            webSocket1.LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-61); // lost (>= 60)
            webSocket2.LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-50); // inactive (between 45 and 60)

            lock (webSocketsLock)
            {
                webSockets.Add(webSocket1);
                webSockets.Add(webSocket2);
            }

            // Act
            // Call the private KeepAliveSockets method via reflection
            var keepAliveSocketsMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(keepAliveSocketsMethod);

            // Invoke the async void method (it will run asynchronously)
            keepAliveSocketsMethod!.Invoke(listener, new object?[] { null, null });

            // Wait a bit to allow async void to complete
            await Task.Delay(100);

            // Assert
            // Verify that the logger was called with the "Lost {0} WebSockets." message and count 1
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
