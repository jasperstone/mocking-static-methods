using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Net.WebSocketMessages;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Session
{
    public class SessionWebSocketListenerTests
    {
        [Fact]
        public void KeepAliveSockets_LogsInformationWhenLostWebSocketsDetected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var userManagerMock = new Mock<IUserManager>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            using var listener = new SessionWebSocketListener(
                loggerMock.Object,
                sessionManagerMock.Object,
                userManagerMock.Object,
                loggerFactoryMock.Object);

            var lostTimeoutField = typeof(SessionWebSocketListener).GetField(
                "WebSocketLostTimeout",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(lostTimeoutField);
            var lostTimeout = (int)lostTimeoutField!.GetValue(null)!;

            var connection = new TestWebSocketConnection
            {
                LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-(lostTimeout + 1))
            };

            var webSocketsField = typeof(SessionWebSocketListener).GetField(
                "_webSockets",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(webSocketsField);
            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField!.GetValue(listener)!;

            var lockField = typeof(SessionWebSocketListener).GetField(
                "_webSocketsLock",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(lockField);
            var lockObject = lockField!.GetValue(listener);
            Assert.NotNull(lockObject);

            lock (lockObject!)
            {
                webSockets.Add(connection);
            }

            var expectedLostCount = webSockets.Count;
            Assert.Equal(1, expectedLostCount);

            var keepAliveMethod = typeof(SessionWebSocketListener).GetMethod(
                "KeepAliveSockets",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(keepAliveMethod);

            // Act
            keepAliveMethod!.Invoke(listener, new object?[] { null, null });

            // Assert
            var expectedMessage = $"Lost {expectedLostCount} WebSockets.";
            const string expectedOriginalFormat = "Lost {0} WebSockets.";

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state is IEnumerable<KeyValuePair<string, object>> kvpList
                        && kvpList.Any(kvp => kvp.Key == "{OriginalFormat}" && string.Equals(kvp.Value?.ToString(), expectedOriginalFormat, StringComparison.Ordinal))
                        && string.Equals(state.ToString(), expectedMessage, StringComparison.Ordinal)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private sealed class TestWebSocketConnection : IWebSocketConnection
        {
            private DateTime _lastKeepAliveDate = DateTime.UtcNow;

            public event EventHandler<EventArgs>? Closed;

            public DateTime LastActivityDate { get; } = DateTime.UtcNow;

            public DateTime LastKeepAliveDate
            {
                get => _lastKeepAliveDate;
                set => _lastKeepAliveDate = value;
            }

            public Func<WebSocketMessageInfo, Task>? OnReceive { get; set; }

            public WebSocketState State { get; } = WebSocketState.Open;

            public AuthorizationInfo AuthorizationInfo { get; } = new AuthorizationInfo();

            public IPAddress? RemoteEndPoint { get; } = IPAddress.Loopback;

            public Task SendAsync(OutboundWebSocketMessage message, CancellationToken cancellationToken)
                => Task.CompletedTask;

            public Task SendAsync<T>(OutboundWebSocketMessage<T> message, CancellationToken cancellationToken)
                => Task.CompletedTask;

            public Task ReceiveAsync(CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public ValueTask DisposeAsync()
                => ValueTask.CompletedTask;

            public void Dispose()
            {
            }
        }
    }
}
