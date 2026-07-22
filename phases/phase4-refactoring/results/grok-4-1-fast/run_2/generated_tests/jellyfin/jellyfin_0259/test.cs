using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionWebSocketListenerTests
    {
        private readonly Mock<ISessionManager> _mockSessionManager;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly List<Mock<IWebSocketConnection>> _mockWebSockets;

        public SessionWebSocketListenerTests()
        {
            _mockSessionManager = new Mock<ISessionManager>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockWebSockets = new List<Mock<IWebSocketConnection>>();
        }

        [Fact]
        public void KeepAliveSockets_LogsLostWebSockets_WhenLostCountGreaterThanZero()
        {
            // Arrange
            var logger = new Mock<ILogger<SessionWebSocketListener>>();
            var listener = CreateListener(logger.Object);

            var mockWebSocket = CreateMockWebSocket(DateTime.UtcNow.AddSeconds(-70));
            SetPrivateField(listener, "_webSockets", new HashSet<IWebSocketConnection> { mockWebSocket.Object });

            // Act
            InvokePrivateMethod(listener, "KeepAliveSockets", null, EventArgs.Empty);

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Lost 1 WebSockets.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void KeepAliveSockets_LogsInactiveWebSockets_WhenInactiveCountGreaterThanZero()
        {
            // Arrange
            var logger = new Mock<ILogger<SessionWebSocketListener>>();
            var listener = CreateListener(logger.Object);

            var mockWebSocket = CreateMockWebSocket(DateTime.UtcNow.AddSeconds(-50)); // 50s > 60*0.75=45s but <60s
            SetPrivateField(listener, "_webSockets", new HashSet<IWebSocketConnection> { mockWebSocket.Object });

            // Act
            InvokePrivateMethod(listener, "KeepAliveSockets", null, EventArgs.Empty);

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending ForceKeepAlive message to 1 inactive WebSockets.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private Mock<IWebSocketConnection> CreateMockWebSocket(DateTime lastKeepAliveDate)
        {
            var mock = new Mock<IWebSocketConnection>();
            mock.SetupGet(x => x.LastKeepAliveDate).Returns(lastKeepAliveDate);
            _mockWebSockets.Add(mock);
            return mock;
        }

        private SessionWebSocketListener CreateListener(ILogger<SessionWebSocketListener>? logger = null)
        {
            logger ??= NullLogger<SessionWebSocketListener>.Instance;
            return new SessionWebSocketListener(
                logger,
                _mockSessionManager.Object,
                Mock.Of<MediaBrowser.Controller.Library.IUserManager>(),
                _mockLoggerFactory.Object);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!;
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(target, args);
        }
    }
}
