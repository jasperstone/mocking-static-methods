using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Net.WebSocketMessages.Outbound;

namespace Emby.Tests.Session
{
    public class SessionWebSocketListenerTests
    {
        private readonly Mock<ILogger<SessionWebSocketListener>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly SessionWebSocketListener _listener;

        public SessionWebSocketListenerTests()
        {
            _loggerMock = new Mock<ILogger<SessionWebSocketListener>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<WebSocketController>()).Returns(Mock.Of<ILogger<WebSocketController>>());

            _sessionManagerMock = new Mock<ISessionManager>();
            _userManagerMock = new Mock<IUserManager>();

            _listener = new SessionWebSocketListener(
                _loggerMock.Object,
                _sessionManagerMock.Object,
                _userManagerMock.Object,
                _loggerFactoryMock.Object);
        }

        [Fact]
        public async Task KeepAliveSockets_LogsInformation_WhenWebSocketsAreInactive()
        {
            // Arrange
            var mockWebSocket = new Mock<IWebSocketConnection>();
            var now = DateTime.UtcNow;
            mockWebSocket.SetupGet(w => w.LastKeepAliveDate).Returns(now.AddSeconds(-50));
            var webSockets = new HashSet<IWebSocketConnection> { mockWebSocket.Object };

            // Use reflection to set the private _webSockets field
            var field = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(_listener, webSockets);

            // Act
            await Task.Run(() => _listener.GetType().GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_listener, new object[] { null, null }));

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Watching")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
