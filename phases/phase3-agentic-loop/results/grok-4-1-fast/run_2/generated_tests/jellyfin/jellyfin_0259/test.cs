using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Emby.Server.Implementations.Session;
using System.Reflection;

namespace Emby.Server.Implementations.Tests.Session
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

        private class MockWebSocketConnection : IWebSocketConnection
        {
            public DateTime LastKeepAliveDate { get; set; }
            public event EventHandler? Closed;
            public Task SendAsync(WebSocketMessage message, CancellationToken cancellationToken) => Task.CompletedTask;
        }

        [Fact]
        public void KeepAliveSockets_LogsLostWebSockets_WhenLostCountGreaterThanZero()
        {
            // Arrange
            var listener = new SessionWebSocketListener(
                _loggerMock.Object,
                _sessionManagerMock.Object,
                _userManagerMock.Object,
                _loggerFactoryMock.Object);

            var mockWebSocket = new MockWebSocketConnection
            {
                LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-70) // > 60s timeout
            };

            // Use reflection to access private fields
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            var webSocketsLockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField.GetValue(listener)!;
            var webSocketsLock = webSocketsLockField.GetValue(listener)!;

            lock (webSocketsLock)
            {
                webSockets.Add(mockWebSocket);
            }

            // Act - invoke private KeepAliveSockets method directly
            var keepAliveMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            keepAliveMethod.Invoke(listener, new object[] { null, EventArgs.Empty });

            // Assert - verify LogInformation("Lost {0} WebSockets.", lost.Count) was called
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Lost {0} WebSockets.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void KeepAliveSockets_DoesNotLogLostWebSockets_WhenNoLostWebSockets()
        {
            // Arrange
            var listener = new SessionWebSocketListener(
                _loggerMock.Object,
                _sessionManagerMock.Object,
                _userManagerMock.Object,
                _loggerFactoryMock.Object);

            var mockWebSocket = new MockWebSocketConnection
            {
                LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-30) // < 60s timeout
            };

            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            var webSocketsLockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField.GetValue(listener)!;
            var webSocketsLock = webSocketsLockField.GetValue(listener)!;

            lock (webSocketsLock)
            {
                webSockets.Add(mockWebSocket);
            }

            // Act
            var keepAliveMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            keepAliveMethod.Invoke(listener, new object[] { null, EventArgs.Empty });

            // Assert - verify LogInformation("Lost {0} WebSockets.") was NOT called
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Lost {0} WebSockets.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
