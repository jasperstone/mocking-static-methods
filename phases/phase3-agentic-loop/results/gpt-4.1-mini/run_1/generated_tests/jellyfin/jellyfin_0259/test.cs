using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
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
        Task SendAsync(object message, System.Threading.CancellationToken cancellationToken);
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
        public void KeepAliveSockets_LogsInformation_ForInactiveAndLostWebSockets()
        {
            // Arrange
            var listener = new SessionWebSocketListener(
                _loggerMock.Object,
                _sessionManagerMock.Object,
                _userManagerMock.Object,
                _loggerFactoryMock.Object);

            var webSocket1 = new TestWebSocketConnection();
            var webSocket2 = new TestWebSocketConnection();

            // Use reflection to access private fields
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var webSocketsLockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField!.GetValue(listener)!;
            var webSocketsLock = webSocketsLockField!.GetValue(listener);

            // Lock and add webSockets
            lock (webSocketsLock!)
            {
                webSockets.Add(webSocket1);
                webSockets.Add(webSocket2);
            }

            // Set LastKeepAliveDate to simulate inactive and lost
            var now = DateTime.UtcNow;
            // webSocket1 is inactive (elapsed between 45 and 60 seconds)
            webSocket1.LastKeepAliveDate = now.AddSeconds(-60 * 0.75 - 1);
            // webSocket2 is lost (elapsed >= 60 seconds)
            webSocket2.LastKeepAliveDate = now.AddSeconds(-60 - 1);

            // Act
            // Call the private KeepAliveSockets method via reflection
            var keepAliveSocketsMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            keepAliveSocketsMethod!.Invoke(listener, new object?[] { null, null });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending ForceKeepAlive message to 1 inactive WebSockets.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

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
