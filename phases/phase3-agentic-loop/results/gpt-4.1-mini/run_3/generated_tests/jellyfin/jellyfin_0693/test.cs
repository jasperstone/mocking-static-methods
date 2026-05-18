using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.SyncPlay;

namespace MediaBrowser.Tests.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        [Fact]
        public void SessionLeaving_ResumePlayingTrue_LogsNotifyResumeAndSetsPlayingState()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(WaitingGroupState))).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>(MockBehavior.Loose, null, null);
            sessionMock.SetupGet(s => s.Id).Returns("session1");
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            var waitingState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = true
            };

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, sessionMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session1 left group")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            contextMock.Verify(c => c.SetState(It.IsAny<IGroupState>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_LogsReturnToPreviousStateAndSetsPausedState()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(WaitingGroupState))).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>(MockBehavior.Loose, null, null);
            sessionMock.SetupGet(s => s.Id).Returns("session2");
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            var waitingState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = false
            };

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, sessionMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session2 left group")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            contextMock.Verify(c => c.SetState(It.IsAny<IGroupState>()), Times.Once);
        }
    }
}
