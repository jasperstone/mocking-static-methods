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
        public void SessionLeaving_ShouldLogDebugAndChangeState_ToPlaying_WhenResumePlayingIsTrue()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session1" };
            var cancellationToken = CancellationToken.None;

            contextMock.Setup(c => c.IsBuffering()).Returns(false);
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            contextMock.Setup(c => c.SetBuffering(It.IsAny<SessionInfo>(), false));
            contextMock.Setup(c => c.SetState(It.IsAny<PlayingGroupState>)).Verifiable();

            // Act
            _waitingState.ResumePlaying = true;
            _waitingState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, session, cancellationToken);

            // Assert
            _loggerMock.Verify(
                l => l.LogDebug(
                    It.Is<string>(s => s.Contains("Session {SessionId} left group {GroupId}, notifying others to resume.")),
                    session.Id,
                    It.IsAny<string>()),
                Times.Once);
            contextMock.Verify(c => c.SetState(It.IsAny<PlayingGroupState>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_ShouldLogDebugAndChangeState_ToPaused_WhenResumePlayingIsFalse()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session2" };
            var cancellationToken = CancellationToken.None;

            contextMock.Setup(c => c.IsBuffering()).Returns(false);
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            contextMock.Setup(c => c.SetBuffering(It.IsAny<SessionInfo>(), false));
            contextMock.Setup(c => c.SetState(It.IsAny<PausedGroupState>)).Verifiable();

            // Act
            _waitingState.ResumePlaying = false;
            _waitingState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, session, cancellationToken);

            // Assert
            _loggerMock.Verify(
                l => l.LogDebug(
                    It.Is<string>(s => s.Contains("Session {SessionId} left group {GroupId}, returning to previous state.")),
                    session.Id,
                    It.IsAny<string>()),
                Times.Once);
            contextMock.Verify(c => c.SetState(It.IsAny<PausedGroupState>()), Times.Once);
        }

        [Fact]
        public void HandleRequest_ShouldSetStateToPrevious_WhenSetPlayQueueFails()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session3" };
            var prevState = GroupStateType.Paused;
            var request = new PlayGroupRequest
            {
                PlayingQueue = null,
                PlayingItemPosition = 0,
                StartPositionTicks = 0
            };
            var cancellationToken = CancellationToken.None;

            contextMock.Setup(c => c.SetPlayQueue(It.IsAny<object>(), It.IsAny<long>(), It.IsAny<long>())).Returns(false);
            contextMock.Setup(c => c.SetState(It.IsAny<PausedGroupState>)).Verifiable();

            // Act
            _waitingState.HandleRequest(request, contextMock.Object, prevState, session, cancellationToken);

            // Assert
            contextMock.Verify(c => c.SetState(It.IsAny<PausedGroupState>()), Times.Once);
        }

        [Fact]
        public void HandleRequest_ShouldSendGroupUpdateAndSetAllBuffering_WhenSetPlayQueueSucceeds()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session4" };
            var prevState = GroupStateType.Paused;
            var request = new PlayGroupRequest
            {
                PlayingQueue = new object(),
                PlayingItemPosition = 1,
                StartPositionTicks = 100
            };
            var cancellationToken = CancellationToken.None;

            contextMock.Setup(c => c.SetPlayQueue(It.IsAny<object>(), It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            contextMock.Setup(c => c.GetPlayQueueUpdate(It.IsAny<PlayQueueUpdateReason>())).Returns(new object());
            contextMock.Setup(c => c.SendGroupUpdate(It.IsAny<SessionInfo>(), It.IsAny<SyncPlayBroadcastType>(), It.IsAny<object>(), It.IsAny<CancellationToken>()));
            contextMock.Setup(c => c.SetAllBuffering(true));
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());

            // Act
            _waitingState.HandleRequest(request, contextMock.Object, prevState, session, cancellationToken);

            // Assert
            contextMock.Verify(c => c.SendGroupUpdate(It.IsAny<SessionInfo>(), SyncPlayBroadcastType.AllGroup, It.IsAny<object>(), cancellationToken), Times.Once);
            contextMock.Verify(c => c.SetAllBuffering(true), Times.Once);
            _loggerMock.Verify(
                l => l.LogDebug(
                    It.Is<string>(s => s.Contains("Session {SessionId} set a new play queue in group {GroupId}.")),
                    session.Id,
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
