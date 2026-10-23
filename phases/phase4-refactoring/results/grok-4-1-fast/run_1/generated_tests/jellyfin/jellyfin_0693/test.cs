#nullable enable

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
    public sealed class WaitingGroupStateTests : IDisposable
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
            _contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());

            _sessionManagerMock = new Mock<ISessionManager>();
            _sessionLoggerMock = new Mock<ILogger<SessionInfo>>();
            _session = new SessionInfo(_sessionManagerMock.Object, _sessionLoggerMock.Object)
            {
                Id = "test-session-id"
            };

            _state = new WaitingGroupState(_loggerFactoryMock.Object);
        }

        public void Dispose()
        {
            _sessionManagerMock?.VerifyNoOtherCalls();
            _sessionLoggerMock?.VerifyNoOtherCalls();
        }

        [Fact]
        public void SessionLeaving_ResumePlayingTrue_NoBuffering_LogsResumeDebugMessage()
        {
            // Arrange
            _state.ResumePlaying = true;
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert - Verify exact template match
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("notifying others to resume.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
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

            // Assert - Verify exact template match
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("returning to previous state.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_WithBuffering_DoesNotLogDebugMessages()
        {
            // Arrange
            _state.ResumePlaying = true;
            _contextMock.Setup(c => c.IsBuffering()).Returns(true);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, 0, It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }
    }
}
