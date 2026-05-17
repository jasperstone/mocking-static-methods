using System;
using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.PlaybackRequests;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
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

            _contextMock = new Mock<IGroupStateContext>();
            _contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);

            // Create SessionInfo with minimal mocks
            var sessionManagerMock = new Mock<ISessionManager>();
            var sessionLoggerMock = new Mock<ILogger<SessionInfo>>();
            _session = new SessionInfo(sessionManagerMock.Object, sessionLoggerMock.Object);
            _session.Id = "test-session-id";

            _state = new WaitingGroupState(_loggerFactoryMock.Object);
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

        [Fact]
        public void SessionLeaving_WhenNotBufferingAndResumePlayingTrue_ShouldLogResumeMessage()
        {
            // Arrange
            _state.ResumePlaying = true;

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                        func(It.IsAny<It.IsAnyType>(), null)!.Contains("notifying others to resume."))),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_WhenNotBufferingAndResumePlayingFalse_ShouldLogPreviousStateMessage()
        {
            // Arrange
            _state.ResumePlaying = false;

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                        func(It.IsAny<It.IsAnyType>(), null)!.Contains("returning to previous state."))),
                Times.Once);
        }
    }
}
