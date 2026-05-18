using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using System;
using System.Reflection;
using System.Threading;
using Xunit;

public class WaitingGroupStateTests
{
    [Fact]
    public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsTrue()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<WaitingGroupState>>();
        var contextMock = new Mock<IGroupStateContext>();
        var sessionManagerMock = new Mock<ISessionManager>();
        var session = new SessionInfo(sessionManagerMock.Object, Mock.Of<ILogger>());
        var cancellationToken = CancellationToken.None;

        contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
        contextMock.Setup(c => c.IsBuffering()).Returns(false);

        var waitingState = new WaitingGroupState(Mock.Of<ILoggerFactory>())
        {
            ResumePlaying = true
        };

        // Use reflection to set the private _logger field
        var loggerField = typeof(WaitingGroupState).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);
        loggerField.SetValue(waitingState, loggerMock.Object);

        // Act
        waitingState.SessionLeaving(contextMock.Object, GroupStateType.Playing, session, cancellationToken);

        // Assert
        loggerMock.Verify(
            l => l.LogDebug(
                "Session {SessionId} left group {GroupId}, notifying others to resume.",
                It.Is<object[]>(o => o[0] == session.Id && o[1] is Guid),
                It.IsAny<Exception>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsFalse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<WaitingGroupState>>();
        var contextMock = new Mock<IGroupStateContext>();
        var sessionManagerMock = new Mock<ISessionManager>();
        var session = new SessionInfo(sessionManagerMock.Object, Mock.Of<ILogger>());
        var cancellationToken = CancellationToken.None;

        contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
        contextMock.Setup(c => c.IsBuffering()).Returns(false);

        var waitingState = new WaitingGroupState(Mock.Of<ILoggerFactory>())
        {
            ResumePlaying = false
        };

        // Use reflection to set the private _logger field
        var loggerField = typeof(WaitingGroupState).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);
        loggerField.SetValue(waitingState, loggerMock.Object);

        // Act
        waitingState.SessionLeaving(contextMock.Object, GroupStateType.Playing, session, cancellationToken);

        // Assert
        loggerMock.Verify(
            l => l.LogDebug(
                "Session {SessionId} left group {GroupId}, returning to previous state.",
                It.Is<object[]>(o => o[0] == session.Id && o[1] is Guid),
                It.IsAny<Exception>()
            ),
            Times.Once
        );
    }
}
