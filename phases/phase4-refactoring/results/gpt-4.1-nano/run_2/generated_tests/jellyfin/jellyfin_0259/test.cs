using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.Net.WebSocketMessages.Outbound;

namespace Emby.Tests.Session
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

        [Fact]
        public async Task KeepAliveSockets_LogsLostWebSocketsInformation()
        {
            // Arrange
            var listener = new SessionWebSocketListener(
                _loggerMock.Object,
                _sessionManagerMock.Object,
                _userManagerMock.Object,
                _loggerFactoryMock.Object);

            var webSocketMock1 = new Mock<IWebSocketConnection>();
            var webSocketMock2 = new Mock<IWebSocketConnection>();

            var now = DateTime.UtcNow;

            // Setup LastKeepAliveDate to simulate inactive and lost WebSockets
            webSocketMock1.Setup(w => w.LastKeepAliveDate).Returns(now.AddSeconds(-65)); // should be in lost
            webSocketMock2.Setup(w => w.LastKeepAliveDate).Returns(now.AddSeconds(-50)); // should be inactive

            // Add WebSockets to the internal set
            var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var webSocketsSet = (HashSet<IWebSocketConnection>)webSocketsField.GetValue(listener);
            webSocketsSet.Add(webSocketMock1.Object);
            webSocketsSet.Add(webSocketMock2.Object);

            // Trigger the timer callback
            var keepAliveMethod = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Call with nulls for parameters
            keepAliveMethod.Invoke(listener, new object[] { null, null });

            // Wait a moment for async logs
            await Task.Delay(100);

            // Assert that LogDebug was called with "Watching {0} WebSockets."
            _loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Watching 2 WebSockets.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            // Assert that LogInformation for lost WebSockets was called
            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Lost 1 WebSockets.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
