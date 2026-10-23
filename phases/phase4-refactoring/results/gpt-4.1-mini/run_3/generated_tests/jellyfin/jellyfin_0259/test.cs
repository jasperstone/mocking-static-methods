using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Session;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionWebSocketListenerTests
    {
        // Minimal IWebSocketConnection interface for test compilation
        public interface IWebSocketConnection
        {
            event EventHandler? Closed;
            DateTime LastKeepAliveDate { get; set; }
            Task SendAsync(object message, CancellationToken cancellationToken);
        }

        private class TestWebSocketConnection : IWebSocketConnection
        {
            public event EventHandler? Closed;
            public DateTime LastKeepAliveDate { get; set; }
            public bool SendAsyncCalled { get; private set; }
            public Exception? SendAsyncException { get; set; }

            public Task SendAsync(object message, CancellationToken cancellationToken)
            {
                SendAsyncCalled = true;
                if (SendAsyncException != null)
                {
                    throw SendAsyncException;
                }
                return Task.CompletedTask;
            }

            public void RaiseClosed()
            {
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        // Minimal ISessionManager and IUserManager interfaces for test compilation
        public interface ISessionManager
        {
            void OnSessionControllerConnected(SessionInfo session);
        }

        public interface IUserManager { }

        // Minimal stub for SessionInfo to allow compilation
        public class SessionInfo { }

        // We cannot access the private const WebSocketLostTimeout, so we use reflection to get it
        private int GetWebSocketLostTimeout()
        {
            var field = typeof(SessionWebSocketListener).GetField("WebSocketLostTimeout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (int)field!.GetValue(null)!;
        }

        [Fact]
        public void KeepAliveSockets_LogsInformationForInactiveAndLostWebSockets()
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

            var webSocketInactive = new TestWebSocketConnection();
            var webSocketLost = new TestWebSocketConnection();

            int lostTimeout = GetWebSocketLostTimeout();

            // Set LastKeepAliveDate to simulate inactive and lost states
            webSocketInactive.LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-lostTimeout * 0.8);
            webSocketLost.LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-lostTimeout * 1.1);

            // Add web sockets to the private _webSockets collection via reflection
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var webSocketsLockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField!.GetValue(listener)!;
            var webSocketsLock = webSocketsLockField!.GetValue(listener)!;

            lock (webSocketsLock)
            {
                webSockets.Add(webSocketInactive);
                webSockets.Add(webSocketLost);
            }

            // Act
            // Call the private KeepAliveSockets method via reflection
            var method = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method!.Invoke(listener, new object?[] { null, null });

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending ForceKeepAlive message to 1 inactive WebSockets.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Lost 1 WebSockets.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
