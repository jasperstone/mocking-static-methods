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
        [Fact]
        public void SessionLeaving_ResumePlayingTrue_LogsAndSetsPlayingState()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>();
            sessionMock.Setup(s => s.Id).Returns("sessionId");
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            var waitingState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = true
            };

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    "Session {SessionId} left group {GroupId}, notifying others to resume.",
                    It.IsAny<object[]>()),
                Times.Once);

            contextMock.Verify(c => c.SetState(It.IsAny<PlayingGroupState>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_LogsAndSetsPausedState()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>();
            sessionMock.Setup(s => s.Id).Returns("sessionId");
            contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            var waitingState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = false
            };

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    "Session {SessionId} left group {GroupId}, returning to previous state.",
                    It.IsAny<object[]>()),
                Times.Once);

            contextMock.Verify(c => c.SetState(It.IsAny<PausedGroupState>()), Times.Once);
        }
    }
}
