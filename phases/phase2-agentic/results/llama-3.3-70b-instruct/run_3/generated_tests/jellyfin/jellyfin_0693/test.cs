using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        private readonly Mock<ILogger<WaitingGroupState>> _loggerMock;
        private readonly Mock<IGroupStateContext> _contextMock;
        private readonly Mock<SessionInfo> _sessionMock;

        public WaitingGroupStateTests()
        {
            _loggerMock = new Mock<ILogger<WaitingGroupState>>();
            _contextMock = new Mock<IGroupStateContext>();
            _sessionMock = new Mock<SessionInfo>();
        }

        [Fact]
        public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsTrue()
        {
            // Arrange
            var waitingGroupState = new WaitingGroupState(new LoggerFactory());
            waitingGroupState.ResumePlaying = true;
            _sessionMock.SetupGet(s => s.Id).Returns("SessionId");
            _contextMock.SetupGet(c => c.GroupId).Returns(Guid.NewGuid());

            // Act
            waitingGroupState.SessionLeaving(_contextMock.Object, GroupStateType.Playing, _sessionMock.Object, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogDebug("Session {SessionId} left group {GroupId}, notifying others to resume.", "SessionId", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsFalse()
        {
            // Arrange
            var waitingGroupState = new WaitingGroupState(new LoggerFactory());
            waitingGroupState.ResumePlaying = false;
            _sessionMock.SetupGet(s => s.Id).Returns("SessionId");
            _contextMock.SetupGet(c => c.GroupId).Returns(Guid.NewGuid());

            // Act
            waitingGroupState.SessionLeaving(_contextMock.Object, GroupStateType.Playing, _sessionMock.Object, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogDebug("Session {SessionId} left group {GroupId}, returning to previous state.", "SessionId", It.IsAny<string>()), Times.Once);
        }
    }
}
