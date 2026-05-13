using Xunit;
using Moq;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using System.Threading;
using System;

namespace MediaBrowser.Controller.Tests.SyncPlay.GroupStates
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
            _waitingGroupState = new WaitingGroupState(_loggerMock.Object);
        }

        [Fact]
        public void SessionLeaving_ResumePlaying_LogsDebugMessage()
        {
            // Arrange
            var session = new SessionInfo { Id = Guid.NewGuid() };
            var context = _contextMock.Object;
            var cancellationToken = CancellationToken.None;

            _contextMock.Setup(c => c.IsBuffering()).Returns(false);
            _waitingGroupState.ResumePlaying = true;

            // Act
            _waitingGroupState.SessionLeaving(context, GroupStateType.Playing, session, cancellationToken);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    "Session {SessionId} left group {GroupId}, notifying others to resume.",
                    session.Id,
                    context.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_NotResumePlaying_LogsDebugMessage()
        {
            // Arrange
            var session = new SessionInfo { Id = Guid.NewGuid() };
            var context = _contextMock.Object;
            var cancellationToken = CancellationToken.None;

            _contextMock.Setup(c => c.IsBuffering()).Returns(false);
            _waitingGroupState.ResumePlaying = false;

            // Act
            _waitingGroupState.SessionLeaving(context, GroupStateType.Playing, session, cancellationToken);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    "Session {SessionId} left group {GroupId}, returning to previous state.",
                    session.Id,
                    context.GroupId.ToString()),
                Times.Once);
        }
    }
}
