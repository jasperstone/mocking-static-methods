using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.SyncPlay.GroupStates.Tests
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
            loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(_loggerMock.Object);

            _contextMock = new Mock<IGroupStateContext>();
            _contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());

            _session = new SessionInfo(null!, null!)
            {
                Id = "test-session-id"
            };

            _state = new WaitingGroupState(loggerFactoryMock.Object);
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
                l => l.LogDebug(
                    It.Is<string>(s => s.Contains("notifying others to resume")),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
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
                l => l.LogDebug(
                    It.Is<string>(s => s.Contains("returning to previous state")),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_StillBuffering_DoesNotLogDebug()
        {
            // Arrange
            _state.ResumePlaying = true;
            _contextMock.Setup(c => c.IsBuffering()).Returns(true);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public void HandleRequest_PlayGroupRequest_SetsResumePlaying_LogsDebug()
        {
            // Arrange
            var request = new PlayGroupRequest();

            // Act
            _state.HandleRequest(request, _contextMock.Object, GroupStateType.Idle, _session, CancellationToken.None);

            // Assert
            Assert.True(_state.ResumePlaying);
            _loggerMock.Verify(
                l => l.LogDebug(
                    It.Is<string>(s => s.Contains("set a new play queue")),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Once);
        }
    }
}
