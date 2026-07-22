using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.Controller.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        private readonly Mock<ILogger<WaitingGroupState>> _loggerMock;
        private readonly Mock<IGroupStateContext> _contextMock;
        private readonly Mock<SessionInfo> _sessionMock;

        public WaitingGroupStateTests()
        {
            _loggerMock = new Mock<ILogger<WaitingGroupState>>();
            _contextMock = new Mock<IGroupStateContext>();
            _sessionMock = new Mock<SessionInfo>();
        }

        [Fact]
        public async Task SessionLeaving_LogsDebugMessage_WhenResumePlayingIsTrue()
        {
            // Arrange
            var waitingGroupState = new WaitingGroupState(new LoggerFactory());
            waitingGroupState.ResumePlaying = true;
            _contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            _sessionMock.Setup(s => s.Id).Returns(Guid.NewGuid().ToString());

            // Act
            waitingGroupState.SessionLeaving(_contextMock.Object, MediaBrowser.Controller.SyncPlay.GroupStateType.Waiting, _sessionMock.Object, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task SessionLeaving_LogsDebugMessage_WhenResumePlayingIsFalse()
        {
            // Arrange
            var waitingGroupState = new WaitingGroupState(new LoggerFactory());
            waitingGroupState.ResumePlaying = false;
            _contextMock.Setup(c => c.GroupId).Returns(Guid.NewGuid());
            _sessionMock.Setup(s => s.Id).Returns(Guid.NewGuid().ToString());

            // Act
            waitingGroupState.SessionLeaving(_contextMock.Object, MediaBrowser.Controller.SyncPlay.GroupStateType.Waiting, _sessionMock.Object, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
