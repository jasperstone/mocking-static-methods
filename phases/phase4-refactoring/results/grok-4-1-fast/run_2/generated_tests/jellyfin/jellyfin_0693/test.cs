using System;
using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.PlaybackRequests;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.SyncPlay.GroupStates.Tests
{
    public class WaitingGroupStateTests : IDisposable
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<WaitingGroupState>> _loggerMock;
        private readonly Mock<IGroupStateContext> _contextMock;
        private readonly SessionInfo _session;
        private readonly WaitingGroupState _state;

        public WaitingGroupStateTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<WaitingGroupState>>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(_loggerMock.Object);

            var sessionManagerMock = new Mock<ISessionManager>();
            var loggerMock = new Mock<ILogger<SessionInfo>>();
            _session = new SessionInfo(sessionManagerMock.Object, loggerMock.Object);
            _session.Id = "test-session-id";

            _contextMock = new Mock<IGroupStateContext>();
            _contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());

            _state = new WaitingGroupState(_loggerFactoryMock.Object);
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

        [Fact]
        public void SessionLeaving_ResumePlayingTrue_NoBuffering_LogsResumeMessage()
        {
            // Arrange
            _state.ResumePlaying = true;
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("notifying others to resume")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_NoBuffering_LogsPreviousStateMessage()
        {
            // Arrange
            _state.ResumePlaying = false;
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("returning to previous state")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_StillBuffering_NoLogDebugCalled()
        {
            // Arrange
            _state.ResumePlaying = true;
            _contextMock.Setup(c => c.IsBuffering()).Returns(true);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    0,
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void HandleRequest_PlayGroupRequest_Success_LogsNewPlayQueueMessage()
        {
            // Arrange
            var request = new PlayGroupRequest(new[] { Guid.NewGuid(), Guid.NewGuid() }, 0, 0L);
            _contextMock.Setup(c => c.SetPlayQueue(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<int>(), It.IsAny<long>())).Returns(true);

            // Act
            _state.HandleRequest(request, _contextMock.Object, GroupStateType.Idle, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("set a new play queue")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
