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
    public interface ISessionInfo
    {
        string Id { get; set; }
    }

    public class MockSessionInfo : ISessionInfo
    {
        public string Id { get; set; }
    }

    public class WaitingGroupStateTests
    {
        [Fact]
        public void SessionLeaving_ShouldLogDebug_WhenResumePlayingIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<ISessionInfo>();
            var cancellationToken = new CancellationToken();

            var waitingGroupState = new WaitingGroupState(Mock.Of<ILoggerFactory>());
            waitingGroupState.ResumePlaying = true;

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionMock.Object, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    "Session {SessionId} left group {GroupId}, notifying others to resume.",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void SessionLeaving_ShouldLogDebug_WhenResumePlayingIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<ISessionInfo>();
            var cancellationToken = new CancellationToken();

            var waitingGroupState = new WaitingGroupState(Mock.Of<ILoggerFactory>());
            waitingGroupState.ResumePlaying = false;

            // Act
            waitingGroupState.SessionLeaving(contextMock.Object, GroupStateType.Playing, sessionMock.Object, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    "Session {SessionId} left group {GroupId}, returning to previous state.",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
