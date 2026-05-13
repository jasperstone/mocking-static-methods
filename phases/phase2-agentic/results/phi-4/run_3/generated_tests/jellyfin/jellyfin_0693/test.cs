using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests.Controller.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        [Fact]
        public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsTrue()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>();
            sessionMock.Setup(s => s.Id).Returns("SessionId");
            contextMock.Setup(c => c.GroupId).Returns("GroupId");
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            var waitingState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = true
            };

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogDebug(
                    It.Is<string>(s => s.Contains("Session {SessionId} left group {GroupId}, notifying others to resume.")),
                    It.Is<object[]>(o => o[0].ToString() == "SessionId" && o[1].ToString() == "GroupId")),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsFalse()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(loggerMock.Object);

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>();
            sessionMock.Setup(s => s.Id).Returns("SessionId");
            contextMock.Setup(c => c.GroupId).Returns("GroupId");
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            var waitingState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = false
            };

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogDebug(
                    It.Is<string>(s => s.Contains("Session {SessionId} left group {GroupId}, returning to previous state.")),
                    It.Is<object[]>(o => o[0].ToString() == "SessionId" && o[1].ToString() == "GroupId")),
                Times.Once);
        }
    }
}
