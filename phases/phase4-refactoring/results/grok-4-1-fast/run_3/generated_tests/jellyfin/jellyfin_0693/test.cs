using System;
using System.Collections.Generic;
using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.SyncPlay.PlaybackRequests;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace MediaBrowser.Controller.SyncPlay.Tests.GroupStates
{
    public class WaitingGroupStateTests
    {
        private readonly Mock<ILogger<WaitingGroupState>> _loggerMock;
        private readonly Mock<IGroupStateContext> _contextMock;
        private readonly SessionInfo _session;
        private readonly WaitingGroupState _state;

        public WaitingGroupStateTests()
        {
            _loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
            _state = new WaitingGroupState(loggerFactoryMock.Object);

            var sessionManagerMock = new Mock<ISessionManager>();
            var sessionLoggerMock = new Mock<ILogger<SessionInfo>>();
            _session = new SessionInfo(sessionManagerMock.Object, sessionLoggerMock.Object) { Id = "test-session" };

            _contextMock = new Mock<IGroupStateContext>();
            _contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
        }

        [Fact]
        public void SessionLeaving_ResumePlayingTrue_NoBuffering_LogsResumeMessage()
        {
            // Arrange
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);
            _state.ResumePlaying = true;

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(
                    "Session {SessionId} left group {GroupId}, notifying others to resume.",
                    _session.Id,
                    _contextMock.Object.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_NoBuffering_LogsPreviousStateMessage()
        {
            // Arrange
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);
            _state.ResumePlaying = false;

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(
                    "Session {SessionId} left group {GroupId}, returning to previous state.",
                    _session.Id,
                    _contextMock.Object.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_ValidRequest_LogsNewPlayQueueMessage()
        {
            // Arrange
            var request = new PlayGroupRequest(new List<Guid>(), 0, 0);
            _contextMock.Setup(c => c.SetPlayQueue(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<int>(), It.IsAny<long>())).Returns(true);

            // Act
            _state.HandleRequest(request, _contextMock.Object, GroupStateType.Idle, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(
                    "Session {SessionId} set a new play queue in group {GroupId}.",
                    _session.Id,
                    _contextMock.Object.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_SetQueueFails_LogsErrorMessage()
        {
            // Arrange
            var request = new PlayGroupRequest(new List<Guid>(), 0, 0);
            _contextMock.Setup(c => c.SetPlayQueue(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<int>(), It.IsAny<long>())).Returns(false);

            // Act
            _state.HandleRequest(request, _contextMock.Object, GroupStateType.Idle, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    "Unable to set playing queue in group {GroupId}.",
                    _contextMock.Object.GroupId.ToString()),
                Times.Once);
        }
    }
}
