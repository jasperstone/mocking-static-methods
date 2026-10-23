using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
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
        event EventHandler Closed;
        DateTime LastKeepAliveDate { get; set; }
        Task SendAsync(object message, System.Threading.CancellationToken cancellationToken);
    }

    public class SessionWebSocketListenerTests
    {
        [Fact]
        public void KeepAliveSockets_LogsInformationForInactiveAndLostWebSockets()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var userManagerMock = new Mock<IUserManager>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

            var listener = (SessionWebSocketListener)Activator.CreateInstance(
                typeof(SessionWebSocketListener),
                loggerMock.Object,
                sessionManagerMock.Object,
                userManagerMock.Object,
                loggerFactoryMock.Object)!;

            // Create mock web sockets with LastKeepAliveDate set to simulate inactive and lost
            var inactiveWebSocket = new Mock<IWebSocketConnection>();
            inactiveWebSocket.SetupProperty<DateTime>(ws => ws.LastKeepAliveDate, DateTime.UtcNow.AddSeconds(-50)); // Between ForceKeepAliveFactor * 60 and 60
            var lostWebSocket = new Mock<IWebSocketConnection>();
            lostWebSocket.SetupProperty<DateTime>(ws => ws.LastKeepAliveDate, DateTime.UtcNow.AddSeconds(-70)); // More than 60 seconds ago

            // Add web sockets to the private _webSockets collection via reflection
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", BindingFlags.NonPublic | BindingFlags.Instance);
            var webSockets = (HashSet<IWebSocketConnection>)webSocketsField.GetValue(listener)!;
            webSockets.Add(inactiveWebSocket.Object);
            webSockets.Add(lostWebSocket.Object);

            // Act
            // Call the private KeepAliveSockets method via reflection
            var method = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(listener, new object?[] { null, null });

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending ForceKeepAlive message to 1 inactive WebSockets.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Lost 1 WebSockets.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
