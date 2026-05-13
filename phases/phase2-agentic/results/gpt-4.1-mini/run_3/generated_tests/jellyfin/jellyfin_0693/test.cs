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
        public void SessionLeaving_ResumePlayingTrue_LogsDebugAndSetsPlayingState()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session1" };
            var cancellationToken = CancellationToken.None;
            var groupId = Guid.NewGuid();
            contextMock.SetupGet(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            // We want ResumePlaying to be true to hit the log on line 107
            _waitingGroupState.ResumePlaying = true;

            // Capture the state set to verify it is PlayingGroupState
            IGroupState setState = null;
            contextMock.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => setState = state);

            // Act
            _waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Playing, session, cancellationToken);

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
            Assert.Equal("Playing", setState.Type.ToString());
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_LogsDebugAndSetsPausedState()
        {
            // Arrange
            var contextMock = new Mock<IGroupStateContext>();
            var session = new SessionInfo { Id = "session2" };
            var cancellationToken = CancellationToken.None;
            var groupId = Guid.NewGuid();
            contextMock.SetupGet(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            // ResumePlaying false to hit the else branch
            _waitingGroupState.ResumePlaying = false;

            IGroupState setState = null;
            contextMock.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => setState = state);

            // Act
            _waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Paused, session, cancellationToken);

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
            Assert.Equal("Paused", setState.Type.ToString());
        }
    }
}
