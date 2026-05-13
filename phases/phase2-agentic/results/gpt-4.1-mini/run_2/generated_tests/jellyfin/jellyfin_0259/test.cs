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
        private class TestWebSocketConnection : IWebSocketConnection
        {
            public event EventHandler? Closed;
            public DateTime LastKeepAliveDate { get; set; }
            public bool SendAsyncCalled { get; private set; }
            public bool ThrowOnSend { get; set; }

            public Task SendAsync(object message, System.Threading.CancellationToken cancellationToken)
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
            var loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var userManagerMock = new Mock<IUserManager>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<WebSocketController>()).Returns(Mock.Of<ILogger<WebSocketController>>());

            var listener = new SessionWebSocketListener(
                loggerMock.Object,
                sessionManagerMock.Object,
                userManagerMock.Object,
                loggerFactoryMock.Object);

            var webSocket1 = new TestWebSocketConnection();
            var webSocket2 = new TestWebSocketConnection();

            // Add webSockets to the private _webSockets collection via reflection
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var webSocketsLockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField!.GetValue(listener)!;
            var webSocketsLock = webSocketsLockField!.GetValue(listener)!;

            // Set LastKeepAliveDate to simulate lost and inactive
            webSocket1.LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-61); // lost (>= 60)
            webSocket2.LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-50); // inactive (> 45 and < 60)

            lock (webSocketsLock)
            {
                webSockets.Add(webSocket1);
                webSockets.Add(webSocket2);
            }

            // Act
            // Call the private KeepAliveSockets method via reflection
            var keepAliveSocketsMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            keepAliveSocketsMethod!.Invoke(listener, new object?[] { null, null });

            // Wait a bit for async void to complete
            await Task.Delay(100);

            // Assert
            // Check that the logger logged the "Lost {0} WebSockets." message with count 1
            loggerMock.Verify(
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
