#nullable enable

using System;
using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.SyncPlay.PlaybackRequests;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace MediaBrowser.Controller.Tests.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        private readonly Mock<ILogger<WaitingGroupState>> _loggerMock;
        private readonly Mock<IGroupStateContext> _contextMock;
        private readonly Mock<SessionInfo> _sessionMock;
        private readonly WaitingGroupState _state;

        public WaitingGroupStateTests()
        {
            _loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(_loggerMock.Object);

            _contextMock = new Mock<IGroupStateContext>();
            _contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);
            _contextMock.Setup(c => c.SetBuffering(It.IsAny<SessionInfo>(), It.IsAny<bool>())).Returns(default);
            
            _sessionMock = new Mock<SessionInfo>();
            _sessionMock.Setup(s => s.Id).Returns("test-session-id");

            _state = new WaitingGroupState(loggerFactoryMock.Object);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingTrue_NoBuffering_CallsLogDebugResume()
        {
            // Arrange
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);
            _state.ResumePlaying = true;
            _contextMock.Setup(c => c.SetBuffering(It.IsAny<SessionInfo>(), false)).Returns(default);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _sessionMock.Object, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Session test-session-id left group") && v.ToString()!.Contains("notifying others to resume.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_NoBuffering_CallsLogDebugPreviousState()
        {
            // Arrange
            _contextMock.Setup(c => c.IsBuffering()).Returns(false);
            _state.ResumePlaying = false;
            _contextMock.Setup(c => c.SetBuffering(It.IsAny<SessionInfo>(), false)).Returns(default);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _sessionMock.Object, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Session test-session-id left group") && v.ToString()!.Contains("returning to previous state.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_GroupBuffering_DoesNotCallLogDebug()
        {
            // Arrange
            _contextMock.Setup(c => c.IsBuffering()).Returns(true);
            _contextMock.Setup(c => c.SetBuffering(It.IsAny<SessionInfo>(), false)).Returns(default);

            // Act
            _state.SessionLeaving(_contextMock.Object, GroupStateType.Paused, _sessionMock.Object, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
