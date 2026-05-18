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
    public class SessionWebSocketListenerTests
    {
        private class TestWebSocketConnection : IWebSocketConnection
        {
            public event EventHandler? Closed;

            public DateTime LastKeepAliveDate { get; set; } = DateTime.UtcNow;

            public bool SendAsyncThrows { get; set; } = false;

            public List<object> SentMessages { get; } = new();

            public Task SendAsync(object message, CancellationToken cancellationToken)
            {
                if (SendAsyncThrows)
                {
                    throw new WebSocketException("Send failed");
                }
                SentMessages.Add(message);
                return Task.CompletedTask;
            }

            public void RaiseClosed()
            {
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        [Fact]
        public async Task KeepAliveSockets_LogsInformationOnInactiveAndLostWebSockets()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var userManagerMock = new Mock<IUserManager>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

            var listener = new SessionWebSocketListener(
                loggerMock.Object,
                sessionManagerMock.Object,
                userManagerMock.Object,
                loggerFactoryMock.Object);

            // Create two test web sockets: one inactive, one lost
            var inactiveSocket = new TestWebSocketConnection();
            var lostSocket = new TestWebSocketConnection();

            // Add sockets to the listener's private _webSockets collection via reflection
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var webSocketsLockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(webSocketsField);
            Assert.NotNull(webSocketsLockField);

            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField.GetValue(listener)!;
            var webSocketsLock = (object)webSocketsLockField.GetValue(listener)!;

            // Set LastKeepAliveDate to simulate elapsed time
            inactiveSocket.LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-SessionWebSocketListener.WebSocketLostTimeout * 0.8);
            lostSocket.LastKeepAliveDate = DateTime.UtcNow.AddSeconds(-SessionWebSocketListener.WebSocketLostTimeout * 1.1);

            lock (webSocketsLock)
            {
                webSockets.Add(inactiveSocket);
                webSockets.Add(lostSocket);
            }

            // Act
            // Call the private KeepAliveSockets method via reflection
            var keepAliveSocketsMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(keepAliveSocketsMethod);

            // The method is async void, so just invoke and wait a bit
            keepAliveSocketsMethod.Invoke(listener, new object?[] { null, null });

            // Wait a short time to allow async void to complete
            await Task.Delay(100);

            // Assert
            // Verify that LogInformation was called with the expected messages
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending ForceKeepAlive message to 1 inactive WebSockets.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

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
