#nullable enable

using System;
using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests : IDisposable
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<WaitingGroupState>> _loggerMock;
        private readonly Mock<IGroupStateContext> _contextMock;
        private readonly SessionInfo _session;
        private readonly WaitingGroupState _state;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<ILogger<SessionInfo>> _sessionLoggerMock;

        public WaitingGroupStateTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<WaitingGroupState>>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(_loggerMock.Object);

            _contextMock = new Mock<IGroupStateContext>();
            var groupId = Guid.NewGuid();
            _contextMock.SetupGet(c => c.GroupId).Returns(groupId);

            _sessionManagerMock = new Mock<ISessionManager>();
            _sessionLoggerMock = new Mock<ILogger<SessionInfo>>();
            _session = new SessionInfo(_sessionManagerMock.Object, _sessionLoggerMock.Object);
            _session.Id = "test-session-id";

            _state = new WaitingGroupState(_loggerFactoryMock.Object);
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

        [Fact]
        public void SessionLeaving_ResumePlayingTrue_NoBuffering_LogsResumeDebugMessage()
        {
            // Arrange
            _state.ResumePlaying = true;
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert - exact template and argument match
            _loggerMock.Verify(
                l => l.LogDebug(
                    "Session {SessionId} left group {GroupId}, notifying others to resume.",
                    "test-session-id",
                    It.Is<Guid>(g => g == _contextMock.Object.GroupId)),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_NoBuffering_LogsPreviousStateDebugMessage()
        {
            // Arrange
            _state.ResumePlaying = false;
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert - exact template and argument match
            _loggerMock.Verify(
                l => l.LogDebug(
                    "Session {SessionId} left group {GroupId}, returning to previous state.",
                    "test-session-id",
                    It.Is<Guid>(g => g == _contextMock.Object.GroupId)),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_GroupBuffering_LogsNothing()
        {
            // Arrange
            _state.ResumePlaying = true;
            _contextMock.Setup(c => c.IsBuffering()).Returns(true);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<Exception>()), Times.Never);
        }
    }
}
