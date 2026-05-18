using Xunit;
using Moq;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using System.Threading;
using System;
using MediaBrowser.Model.SyncPlay;

namespace MediaBrowser.Controller.Tests.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        [Fact]
        public void SessionLeaving_ShouldLogDebug_WhenResumePlayingIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<WaitingGroupState>()).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>();
            var sessionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            sessionMock.Setup(s => s.Id).Returns(sessionId);
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            var waitingGroupState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = true
            };

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {sessionId} left group {groupId}, notifying others to resume.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_ShouldLogDebug_WhenResumePlayingIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<WaitingGroupState>()).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>();
            var sessionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            sessionMock.Setup(s => s.Id).Returns(sessionId);
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            var waitingGroupState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = false
            };

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {sessionId} left group {groupId}, returning to previous state.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
