using System;
using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<WaitingGroupState>> _loggerMock;
        private readonly WaitingGroupState _state;
        private readonly Mock<IGroupStateContext> _contextMock;
        private readonly SessionInfo _session;
        private readonly CancellationToken _cancellationToken;

        public WaitingGroupStateTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<WaitingGroupState>>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(_loggerMock.Object);
            _state = new WaitingGroupState(_loggerFactoryMock.Object);
            _contextMock = new Mock<IGroupStateContext>();
            _session = new SessionInfo { Id = Guid.NewGuid().ToString() };
            _cancellationToken = CancellationToken.None;
        }

        [Fact]
        public void SessionLeaving_ShouldLogDebugAndSetStateToPlaying_WhenResumePlayingIsTrue()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);
            _contextMock.Setup(c => c.GroupId).Returns(groupId);
            _contextMock.Setup(c => c.SetBuffering(It.IsAny<SessionInfo>(), false));
            _contextMock.Setup(c => c.SetState(It.IsAny<PlayingGroupState>()));
            _contextMock.Setup(c => c.SetState(It.IsAny<PausedGroupState>()));

            // Act
            _state.ResumePlaying = true;
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Waiting, _session, _cancellationToken);

            // Assert
            _loggerMock.Verify(
                log => log.LogDebug(
                    It.Is<string>(msg => msg.Contains("Session") && msg.Contains("left group")),
                    _session.Id,
                    groupId.ToString()),
                Times.Once);
            _contextMock.Verify(c => c.SetState(It.IsAny<PlayingGroupState>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_ShouldLogDebugAndSetStateToPaused_WhenResumePlayingIsFalse()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);
            _contextMock.Setup(c => c.GroupId).Returns(groupId);
            _contextMock.Setup(c => c.SetBuffering(It.IsAny<SessionInfo>(), false));
            _contextMock.Setup(c => c.SetState(It.IsAny<PausedGroupState>()));

            // Act
            _state.ResumePlaying = false;
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Waiting, _session, _cancellationToken);

            // Assert
            _loggerMock.Verify(
                log => log.LogDebug(
                    It.Is<string>(msg => msg.Contains("Session") && msg.Contains("returning to previous state")),
                    _session.Id,
                    groupId.ToString()),
                Times.Once);
            _contextMock.Verify(c => c.SetState(It.IsAny<PausedGroupState>()), Times.Once);
        }
    }
}
