using Xunit;
using Moq;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using System.Threading;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Model.Session;

namespace MediaBrowser.Controller.Tests.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        [Fact]
        public void SessionLeaving_ResumePlaying_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<WaitingGroupState>()).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            contextMock.Setup(x => x.IsBuffering()).Returns(false);

            var sessionManagerMock = new Mock<ISessionManager>();
            var sessionInfo = new SessionInfo(sessionManagerMock.Object, loggerMock.Object) { Id = "session1" };
            var groupId = "group1";

            contextMock.Setup(x => x.GroupId).Returns(Guid.Parse(groupId));

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
                    groupId),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_NotResumePlaying_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<WaitingGroupState>()).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            contextMock.Setup(x => x.IsBuffering()).Returns(false);

            var sessionManagerMock = new Mock<ISessionManager>();
            var sessionInfo = new SessionInfo(sessionManagerMock.Object, loggerMock.Object) { Id = "session1" };
            var groupId = "group1";

            contextMock.Setup(x => x.GroupId).Returns(Guid.Parse(groupId));

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
                    groupId),
                Times.Once);
        }
    }
}
