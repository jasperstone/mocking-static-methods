using Xunit;
using Moq;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using System.Threading;
using MediaBrowser.Controller.SyncPlay;

namespace MediaBrowser.Controller.Tests.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        [Fact]
        public void SessionLeaving_ResumePlaying_LogsDebugAndChangesState()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var sessionInfo = new SessionInfo(sessionManagerMock.Object, loggerMock.Object) { Id = "session1" };
            var groupId = Guid.NewGuid();

            contextMock.Setup(x => x.GroupId).Returns(groupId);
            contextMock.Setup(x => x.IsBuffering()).Returns(false);

            var waitingGroupState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = true
            };

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionInfo, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    "Session {SessionId} left group {GroupId}, notifying others to resume.",
                    "session1",
                    groupId.ToString()),
                Times.Once);

            contextMock.Verify(x => x.SetState(It.IsAny<PlayingGroupState>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_DoesNotResumePlaying_LogsDebugAndChangesState()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var sessionInfo = new SessionInfo(sessionManagerMock.Object, loggerMock.Object) { Id = "session1" };
            var groupId = Guid.NewGuid();

            contextMock.Setup(x => x.GroupId).Returns(groupId);
            contextMock.Setup(x => x.IsBuffering()).Returns(false);

            var waitingGroupState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = false
            };

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionInfo, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    "Session {SessionId} left group {GroupId}, returning to previous state.",
                    "session1",
                    groupId.ToString()),
                Times.Once);

            contextMock.Verify(x => x.SetState(It.IsAny<PausedGroupState>()), Times.Once);
        }
    }
}
