using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests
{
    public class WaitingGroupStateTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IGroupStateContext> _groupStateContextMock;
        private readonly Mock<SessionInfo> _sessionInfoMock;

        public WaitingGroupStateTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _groupStateContextMock = new Mock<IGroupStateContext>();
            _sessionInfoMock = new Mock<SessionInfo>();
        }

        [Fact]
        public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsTrue()
        {
            // Arrange
            var waitingGroupState = new WaitingGroupState(_loggerFactoryMock.Object);
            waitingGroupState.ResumePlaying = true;
            _sessionInfoMock.SetupGet(s => s.Id).Returns("SessionId");
            _groupStateContextMock.SetupGet(g => g.GroupId).Returns(Guid.NewGuid());

            // Act
            waitingGroupState.SessionLeaving(_groupStateContextMock.Object, GroupStateType.Waiting, _sessionInfoMock.Object, default);

            // Assert
            _loggerFactoryMock.Verify(l => l.CreateLogger(It.IsAny<string>()), Times.Once);
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            _loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void SessionLeaving_LogsDebugMessage_WhenResumePlayingIsFalse()
        {
            // Arrange
            var waitingGroupState = new WaitingGroupState(_loggerFactoryMock.Object);
            waitingGroupState.ResumePlaying = false;
            _sessionInfoMock.SetupGet(s => s.Id).Returns("SessionId");
            _groupStateContextMock.SetupGet(g => g.GroupId).Returns(Guid.NewGuid());

            // Act
            waitingGroupState.SessionLeaving(_groupStateContextMock.Object, GroupStateType.Waiting, _sessionInfoMock.Object, default);

            // Assert
            _loggerFactoryMock.Verify(l => l.CreateLogger(It.IsAny<string>()), Times.Once);
            var loggerMock = new Mock<ILogger<WaitingGroupState>>();
            _loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
