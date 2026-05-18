using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests
{
    public class WaitingGroupStateTests
    {
        private readonly Mock<ILogger<WaitingGroupState>> _loggerMock;
        private readonly Mock<IGroupStateContext> _contextMock;
        private readonly WaitingGroupState _waitingGroupState;

        public WaitingGroupStateTests()
        {
            _loggerMock = new Mock<ILogger<WaitingGroupState>>();
            _contextMock = new Mock<IGroupStateContext>();
            _waitingGroupState = new WaitingGroupState(new LoggerFactory());
        }

        [Fact]
        public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsTrue()
        {
            // Arrange
            var sessionInfo = new SessionInfo { Id = "SessionId" };
            _contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            _waitingGroupState.ResumePlaying = true;

            // Act
            _waitingGroupState.SessionLeaving(_contextMock.Object, MediaBrowser.Controller.SyncPlay.GroupStateType.Idle, sessionInfo, default);

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsFalse()
        {
            // Arrange
            var sessionInfo = new SessionInfo { Id = "SessionId" };
            _contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            _waitingGroupState.ResumePlaying = false;

            // Act
            _waitingGroupState.SessionLeaving(_contextMock.Object, MediaBrowser.Controller.SyncPlay.GroupStateType.Idle, sessionInfo, default);

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
