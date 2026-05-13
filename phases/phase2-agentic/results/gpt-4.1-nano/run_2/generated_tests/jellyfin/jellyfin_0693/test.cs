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
        private readonly WaitingGroupState _waitingState;

        public WaitingGroupStateTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<WaitingGroupState>>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(_loggerMock.Object);
            _waitingState = new WaitingGroupState(_loggerFactoryMock.Object);
        }

        [Fact]
        public void SessionLeaving_ShouldLogDebugAndChangeState_WhenNotBufferingAndResumePlaying()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session1" };
            var prevState = GroupStateType.Playing;
            var cancellationToken = CancellationToken.None;

            var bufferStatus = false;
            contextMock.Setup(c => c.IsBuffering()).Returns(bufferStatus);
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());

            // Act
            _waitingState.ResumePlaying = true;
            _waitingState.SessionLeaving(contextMock.Object, prevState, session, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(It.Is<string>(s => s.Contains("Session {SessionId} left group {GroupId}, notifying others to resume.")),
                It.IsAny<object[]>()
            );
            contextMock.Verify(c => c.SetState(It.IsAny<PlayingGroupState>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_ShouldLogDebugAndChangeState_ToPaused_WhenNotBufferingAndNotResumePlaying()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session2" };
            var prevState = GroupStateType.Paused;
            var cancellationToken = CancellationToken.None;

            var bufferStatus = false;
            contextMock.Setup(c => c.IsBuffering()).Returns(bufferStatus);
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());

            // Act
            _waitingState.ResumePlaying = false;
            _waitingState.SessionLeaving(contextMock.Object, prevState, session, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(It.Is<string>(s => s.Contains("Session {SessionId} left group {GroupId}, returning to previous state.")),
                It.IsAny<object[]>()
            );
            contextMock.Verify(c => c.SetState(It.IsAny<PausedGroupState>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_ShouldNotLogOrChangeState_WhenBuffering()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session3" };
            var prevState = GroupStateType.Playing;
            var cancellationToken = CancellationToken.None;

            contextMock.Setup(c => c.IsBuffering()).Returns(true);
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());

            // Act
            _waitingState.ResumePlaying = true;
            _waitingState.SessionLeaving(contextMock.Object, prevState, session, cancellationToken);

            // Assert
            _loggerMock.Verify(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            contextMock.Verify(c => c.SetState(It.IsAny<PlayingGroupState>()), Times.Never);
            contextMock.Verify(c => c.SetState(It.IsAny<PausedGroupState>()), Times.Never);
        }
    }
}
