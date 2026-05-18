using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Net.WebSocketMessages.Outbound;

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
    public async Task KeepAliveSockets_ShouldLogInformation_WhenInactiveWebSockets()
    {
        // Arrange
        var logger = _loggerMock.Object;
        var sessionManager = _sessionManagerMock.Object;
        var userManager = _userManagerMock.Object;
        var loggerFactory = _loggerFactoryMock.Object;

        var listener = new SessionWebSocketListener(logger, sessionManager, userManager, loggerFactory);

        var mockWebSocket = new Mock<IWebSocketConnection>();
        mockWebSocket.SetupGet(w => w.LastKeepAliveDate).Returns(DateTime.UtcNow.AddSeconds(-50));
        mockWebSocket.SetupGet(w => w.Closed).Returns((EventHandler)null);
        var webSockets = new HashSet<IWebSocketConnection> { mockWebSocket.Object };

        // Use reflection or internal access to set _webSockets for testing
        var webSocketsField = typeof(SessionWebSocketListener).GetField("_webSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var lockField = typeof(SessionWebSocketListener).GetField("_webSocketsLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var timerField = typeof(SessionWebSocketListener).GetField("_keepAlive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var lockObj = new object();
        lockField.SetValue(listener, new System.Timers.Timer()); // dummy, will be replaced
        webSocketsField.SetValue(listener, webSockets);
        lock (lockObj)
        {
            // forcibly invoke the private method
            var method = typeof(SessionWebSocketListener).GetMethod("KeepAliveSockets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(listener, new object[] { null, null });
        }

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation("Sending ForceKeepAlive message to {0} inactive WebSockets.", 1),
            Times.AtLeastOnce);
    }
}
