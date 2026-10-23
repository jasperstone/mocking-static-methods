using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionWebSocketListenerTests
    {
        private readonly Mock<ISessionManager> _mockSessionManager;
        private readonly Mock<IUserManager> _mockUserManager;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly List<string> _logMessages;

        public SessionWebSocketListenerTests()
        {
            _mockSessionManager = new Mock<ISessionManager>();
            _mockUserManager = new Mock<IUserManager>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _logMessages = new List<string>();
        }

        private SessionWebSocketListener CreateListener()
        {
            var logger = new ListLogger(_logMessages);
            return new SessionWebSocketListener(logger, _mockSessionManager.Object, _mockUserManager.Object, _mockLoggerFactory.Object);
        }

        [Fact]
        public void KeepAliveSockets_LogsLostWebSockets_WhenLostCountGreaterThanZero()
        {
            // Arrange
            var listener = CreateListener();
            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-70)); // > 60s timeout

            // Manually add websocket
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField.GetValue(listener)!;
            webSockets.Add(mockWebSocket.Object);

            // Create ElapsedEventArgs for timer invocation
            var elapsedArgs = new System.Timers.ElapsedEventArgs(DateTime.UtcNow);

            // Act - Directly invoke the KeepAliveSockets method via reflection
            var keepAliveSocketsMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance)!;
            keepAliveSocketsMethod.Invoke(listener, new object?[] { null, elapsedArgs });

            // Assert
            Assert.Contains(_logMessages, msg => msg.Contains("Lost 1 WebSockets.", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void KeepAliveSockets_LogsInactiveWebSockets_WhenInactiveCountGreaterThanZero()
        {
            // Arrange
            var listener = CreateListener();
            var mockWebSocket = new Mock<IWebSocketConnection>();
            mockWebSocket.SetupGet(ws => ws.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-50)); // 45s < elapsed < 60s

            // Manually add websocket
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField.GetValue(listener)!;
            webSockets.Add(mockWebSocket.Object);

            // Create ElapsedEventArgs for timer invocation
            var elapsedArgs = new System.Timers.ElapsedEventArgs(DateTime.UtcNow);

            // Act
            var keepAliveSocketsMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance)!;
            keepAliveSocketsMethod.Invoke(listener, new object?[] { null, elapsedArgs });

            // Assert
            Assert.Contains(_logMessages, msg => msg.Contains("Sending ForceKeepAlive message to 1 inactive WebSockets.", StringComparison.OrdinalIgnoreCase));
        }

        private class ListLogger : ILogger<SessionWebSocketListener>
        {
            private readonly List<string> _messages;

            public ListLogger(List<string> messages)
            {
                _messages = messages;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _messages.Add(formatter(state, exception));
            }
        }
    }
}
