using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
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
        public void SessionLeaving_ResumePlayingTrue_LogsNotifyOthersToResumeAndSetsPlayingState()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session1" };
            var groupId = Guid.NewGuid();
            contextMock.SetupGet(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            // Setup SetState to capture the state set
            IGroupState setState = null;
            contextMock.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => setState = state);

            // We want ResumePlaying to be true to hit the log on line 107
            _waitingGroupState.ResumePlaying = true;

            var cancellationToken = CancellationToken.None;

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
        public void SessionLeaving_ResumePlayingFalse_LogsReturningToPreviousStateAndSetsPausedState()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session2" };
            var groupId = Guid.NewGuid();
            contextMock.SetupGet(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            IGroupState setState = null;
            contextMock.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => setState = state);

            _waitingGroupState.ResumePlaying = false;

            var cancellationToken = CancellationToken.None;

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
        public void SessionLeaving_WhenBuffering_DoesNotLogOrSetState()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session3" };
            contextMock.Setup(c => c.IsBuffering()).Returns(true);

            _waitingGroupState.ResumePlaying = true;

            var cancellationToken = CancellationToken.None;

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

            // No state should be set
            Mock.Get(contextMock.Object).Verify(c => c.SetState(It.IsAny<IGroupState>()), Times.Never);
        }
    }
}
