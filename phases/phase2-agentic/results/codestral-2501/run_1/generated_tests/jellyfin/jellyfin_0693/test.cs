using Xunit;
using Moq;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using System.Threading;
using System;

namespace MediaBrowser.Controller.Tests.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IGroupStateContext> _contextMock;
        private readonly Mock<ILogger<WaitingGroupState>> _loggerMock;
        private readonly WaitingGroupState _waitingGroupState;

        public WaitingGroupStateTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _contextMock = new Mock<IGroupStateContext>();
            _loggerMock = new Mock<ILogger<WaitingGroupState>>();
            _loggerFactoryMock.Setup(x => x.CreateLogger<WaitingGroupState>()).Returns(_loggerMock.Object);
            _waitingGroupState = new WaitingGroupState(_loggerFactoryMock.Object);
        }

        [Fact]
        public void SessionLeaving_ResumePlaying_LogsDebugAndSetsPlayingState()
        {
            // Arrange
            var session = new SessionInfo { Id = Guid.NewGuid() };
            var groupId = Guid.NewGuid();
            _contextMock.Setup(x => x.GroupId).Returns(groupId);
            _contextMock.Setup(x => x.IsBuffering()).Returns(false);
            _waitingGroupState.ResumePlaying = true;

            // Act
            _waitingGroupState.SessionLeaving(_contextMock.Object, GroupStateType.Playing, session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(x => x.LogDebug("Session {SessionId} left group {GroupId}, notifying others to resume.", session.Id, groupId.ToString()), Times.Once);
            _contextMock.Verify(x => x.SetState(It.IsAny<PlayingGroupState>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_NotResumePlaying_LogsDebugAndSetsPausedState()
        {
            // Arrange
            var session = new SessionInfo { Id = Guid.NewGuid() };
            var groupId = Guid.NewGuid();
            _contextMock.Setup(x => x.GroupId).Returns(groupId);
            _contextMock.Setup(x => x.IsBuffering()).Returns(false);
            _waitingGroupState.ResumePlaying = false;

            // Act
            _waitingGroupState.SessionLeaving(_contextMock.Object, GroupStateType.Playing, session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(x => x.LogDebug("Session {SessionId} left group {GroupId}, returning to previous state.", session.Id, groupId.ToString()), Times.Once);
            _contextMock.Verify(x => x.SetState(It.IsAny<PausedGroupState>()), Times.Once);
        }
    }
}
