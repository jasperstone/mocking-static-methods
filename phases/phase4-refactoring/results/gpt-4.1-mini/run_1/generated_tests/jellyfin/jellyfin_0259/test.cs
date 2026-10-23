using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Session;
using System.Reflection;

namespace Emby.Server.Implementations.Session.Tests
{
    // Minimal stub for IWebSocketConnection interface
    public interface IWebSocketConnection : IDisposable
    {
        event EventHandler? Closed;
        DateTime LastKeepAliveDate { get; set; }
        Task SendAsync(object message, CancellationToken cancellationToken);
    }

    // Minimal stub for ISessionManager interface
    public interface ISessionManager
    {
        void OnSessionControllerConnected(object session);
    }

    // Minimal stub for IUserManager interface
    public interface IUserManager
    {
    }

    public class SessionWebSocketListenerTests
    {
        private class TestWebSocketConnection : IWebSocketConnection
        {
            public event EventHandler? Closed;
            public DateTime LastKeepAliveDate { get; set; } = DateTime.UtcNow;
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

            public void Dispose() { }
        }

        [Fact]
        public async Task KeepAliveSockets_LogsInformationForInactiveAndLostWebSockets()
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
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance);
            var webSocketsLockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", BindingFlags.NonPublic | BindingFlags.Instance);
            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField!.GetValue(listener)!;
            var webSocketsLock = webSocketsLockField!.GetValue(listener)!;

            // Set LastKeepAliveDate to simulate inactive and lost
            webSocket1.LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-50); // inactive (between 45 and 60)
            webSocket2.LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-61); // lost (>= 60)

            lock (webSocketsLock)
            {
                webSockets.Add(webSocket1);
                webSockets.Add(webSocket2);
            }

            // Act
            // Call the private KeepAliveSockets method via reflection
            var keepAliveSocketsMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(keepAliveSocketsMethod);

            // Because KeepAliveSockets is async void, we run it and wait a bit for async tasks to complete
            keepAliveSocketsMethod!.Invoke(listener, new object?[] { null, null });

            // Wait a short time to allow async void to complete
            await Task.Delay(100);

            // Assert
            // Verify that LogInformation was called with the expected messages
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending ForceKeepAlive message to 1 inactive WebSockets.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Lost 1 WebSockets.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
