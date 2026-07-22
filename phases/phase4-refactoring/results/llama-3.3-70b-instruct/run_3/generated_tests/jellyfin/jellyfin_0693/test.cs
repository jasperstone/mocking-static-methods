using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.Session;

namespace MediaBrowser.Controller.SyncPlay.Tests
{
    public class WaitingGroupStateTests
    {
        [Fact]
        public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>();
            var waitingGroupState = new WaitingGroupState(new LoggerFactory());

            waitingGroupState.ResumePlaying = true;
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            sessionMock.Setup(s => s.Id).Returns(Guid.NewGuid().ToString());

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionMock.Object, default);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>();
            var waitingGroupState = new WaitingGroupState(new LoggerFactory());

            waitingGroupState.ResumePlaying = false;
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            sessionMock.Setup(s => s.Id).Returns(Guid.NewGuid().ToString());

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionMock.Object, default);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
