using Xunit;
using Moq;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using System.Threading;
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
            var contextMock = new Mock<IGroupStateContext>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var sessionLoggerMock = new Mock<ILogger<SessionInfo>>();
            var sessionInfo = new SessionInfo(sessionManagerMock.Object, sessionLoggerMock.Object) { Id = "session1" };
            var groupId = "group1";
            var cancellationToken = new CancellationToken();

            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            var waitingGroupState = new WaitingGroupState(Mock.Of<ILoggerFactory>());
            waitingGroupState.ResumePlaying = true;

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionInfo, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session1 left group group1, notifying others to resume.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_ShouldLogDebug_WhenResumePlayingIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var contextMock = new Mock<IGroupStateContext>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var sessionLoggerMock = new Mock<ILogger<SessionInfo>>();
            var sessionInfo = new SessionInfo(sessionManagerMock.Object, sessionLoggerMock.Object) { Id = "session1" };
            var groupId = "group1";
            var cancellationToken = new CancellationToken();

            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            var waitingGroupState = new WaitingGroupState(Mock.Of<ILoggerFactory>());
            waitingGroupState.ResumePlaying = false;

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionInfo, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session1 left group group1, returning to previous state.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
