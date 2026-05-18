using System;
using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.SyncPlay.Tests.GroupStates
{
    public class WaitingGroupStateTests
    {
        [Fact]
        public void SessionLeaving_ResumePlayingTrue_LogsNotifyResumeAndSetsPlayingState()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(loggerMock.Object);

            var waitingState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = true
            };

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>(MockBehavior.Strict, null, null);
            sessionMock.SetupGet(s => s.Id).Returns("session1");
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            IGroupState capturedState = null;
            contextMock.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => capturedState = state);

            var cancellationToken = CancellationToken.None;

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, sessionMock.Object, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session1 left group")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.NotNull(capturedState);
            Assert.IsType<PlayingGroupState>(capturedState);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_LogsReturnToPreviousStateAndSetsPausedState()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(loggerMock.Object);

            var waitingState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = false
            };

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>(MockBehavior.Strict, null, null);
            sessionMock.SetupGet(s => s.Id).Returns("session2");
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            IGroupState capturedState = null;
            contextMock.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => capturedState = state);

            var cancellationToken = CancellationToken.None;

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, sessionMock.Object, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session2 left group")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.NotNull(capturedState);
            Assert.IsType<PausedGroupState>(capturedState);
        }
    }
}
