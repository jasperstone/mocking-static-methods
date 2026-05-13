using System;
using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.SyncPlay.PlaybackRequests;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.SyncPlay.Tests.GroupStates
{
    public class WaitingGroupStateTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<WaitingGroupState>> _loggerMock;
        private readonly WaitingGroupState _waitingGroupState;

        public WaitingGroupStateTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<WaitingGroupState>>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(_loggerMock.Object);
            _waitingGroupState = new WaitingGroupState(_loggerFactoryMock.Object);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingTrue_LogsNotifyResumeAndSetsPlayingState()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session1" };
            var cancellationToken = CancellationToken.None;
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            // Setup SetBuffering to do nothing
            contextMock.Setup(c => c.SetBuffering(session, false));

            // Setup SetState to capture the state set
            IGroupState setState = null;
            contextMock.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => setState = state);

            // Setup PlayingGroupState.HandleRequest to be verifiable
            var playingGroupStateMock = new Mock<PlayingGroupState>(_loggerFactoryMock.Object) { CallBase = true };
            bool handleRequestCalled = false;
            playingGroupStateMock
                .Setup(p => p.HandleRequest(It.IsAny<UnpauseGroupRequest>(), contextMock.Object, _waitingGroupState.Type, session, cancellationToken))
                .Callback(() => handleRequestCalled = true);

            // We need to intercept creation of PlayingGroupState inside WaitingGroupState
            // Since it is created directly, we cannot inject it, so we will use a workaround:
            // We will replace the PlayingGroupState type with a derived test type that sets a flag.
            // But since we cannot do that easily, we will just verify the logger call and that SetState was called with a PlayingGroupState.

            _waitingGroupState.ResumePlaying = true;

            // Act
            _waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, session, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session1 left group")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.NotNull(setState);
            Assert.IsType<PlayingGroupState>(setState);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_LogsReturnToPreviousStateAndSetsPausedState()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session2" };
            var cancellationToken = CancellationToken.None;
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            contextMock.Setup(c => c.SetBuffering(session, false));

            IGroupState setState = null;
            contextMock.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => setState = state);

            _waitingGroupState.ResumePlaying = false;

            // Act
            _waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, session, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session2 left group")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.NotNull(setState);
            Assert.IsType<PausedGroupState>(setState);
        }

        [Fact]
        public void SessionLeaving_Buffering_DoesNotLogOrSetState()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session3" };
            var cancellationToken = CancellationToken.None;
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(true);

            contextMock.Setup(c => c.SetBuffering(session, false));

            // Act
            _waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, session, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);

            contextMock.Verify(c => c.SetState(It.IsAny<IGroupState>()), Times.Never);
        }
    }
}
