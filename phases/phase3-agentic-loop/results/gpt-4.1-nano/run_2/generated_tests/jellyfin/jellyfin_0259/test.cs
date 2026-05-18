using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Net.WebSocketMessages.Outbound;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using System.Threading;

namespace Emby.Tests.Session
{
    public class WebSocketControllerTests
    {
        private readonly Mock<ILogger<WebSocketController>> _loggerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly SessionInfo _session;

        public WebSocketControllerTests()
        {
            _loggerMock = new Mock<ILogger<WebSocketController>>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _session = new SessionInfo { Id = "session1" };
        }

        [Fact]
        public void AddWebSocket_ShouldAddSocketAndLog()
        {
            var controller = new WebSocketController(_loggerMock.Object, _session, _sessionManagerMock.Object);
            var mockSocket = new Mock<IWebSocketConnection>();
            mockSocket.Setup(s => s.State).Returns(WebSocketState.Open);
            controller.AddWebSocket(mockSocket.Object);
            _loggerMock.Verify(l => l.LogDebug("Adding websocket to session {Session}", _session.Id), Times.Once);
        }

        [Fact]
        public async Task SendMessage_ShouldSendToOpenSocket()
        {
            var controller = new WebSocketController(_loggerMock.Object, _session, _sessionManagerMock.Object);
            var mockSocket = new Mock<IWebSocketConnection>();
            mockSocket.Setup(s => s.State).Returns(WebSocketState.Open);
            mockSocket.Setup(s => s.LastActivityDate).Returns(DateTime.UtcNow);
            controller.AddWebSocket(mockSocket.Object);
            var messageId = Guid.NewGuid();
            await controller.SendMessage(SessionMessageType.Play, messageId, new { test = "data" }, CancellationToken.None);
            mockSocket.Verify(s => s.SendAsync(It.IsAny<OutboundWebSocketMessage<object>>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task SendMessage_ShouldNotSendIfNoOpenSocket()
        {
            var controller = new WebSocketController(_loggerMock.Object, _session, _sessionManagerMock.Object);
            var mockSocket = new Mock<IWebSocketConnection>();
            mockSocket.Setup(s => s.State).Returns(WebSocketState.Closed);
            controller.AddWebSocket(mockSocket.Object);
            var result = await controller.SendMessage(SessionMessageType.Play, Guid.NewGuid(), new { test = "data" }, CancellationToken.None);
            Assert.Same(Task.CompletedTask, result);
        }

        [Fact]
        public async Task OnConnectionClosed_ShouldRemoveSocketAndCallCloseIfNeeded()
        {
            var controller = new WebSocketController(_loggerMock.Object, _session, _sessionManagerMock.Object);
            var mockSocket = new Mock<IWebSocketConnection>();
            mockSocket.Setup(s => s.State).Returns(WebSocketState.Open);
            mockSocket.Setup(s => s.LastActivityDate).Returns(DateTime.UtcNow);
            controller.AddWebSocket(mockSocket.Object);
            mockSocket.Setup(s => s.Dispose()).Verifiable();
            mockSocket.Setup(s => s.Closed).Raises(s => s.Closed += null, EventArgs.Empty);
            await controller.DisposeAsync();
            mockSocket.Verify(s => s.Dispose(), Times.Once);
        }
    }
}
