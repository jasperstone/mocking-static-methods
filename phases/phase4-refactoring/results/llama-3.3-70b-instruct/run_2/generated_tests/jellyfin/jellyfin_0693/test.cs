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
            var sessionInfo = new SessionInfo(null, null);
            sessionInfo.Id = Guid.NewGuid();
            var waitingGroupState = new WaitingGroupState(new LoggerFactory());

            waitingGroupState.ResumePlaying = true;
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, sessionInfo, default);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var contextMock = new Mock<IGroupStateContext>();
            var sessionInfo = new SessionInfo(null, null);
            sessionInfo.Id = Guid.NewGuid();
            var waitingGroupState = new WaitingGroupState(new LoggerFactory());

            waitingGroupState.ResumePlaying = false;
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, sessionInfo, default);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
