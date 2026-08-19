using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Xunit;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.SyncPlay;

namespace MediaBrowser.Controller.Tests.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        [Fact]
        public void SessionLeaving_ResumePlayingTrue_SetsPlayingState()
        {
            // Arrange
            using var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));
            var waitingState = new WaitingGroupState(loggerFactory);
            waitingState.ResumePlaying = true;

            var contextMock = new Moq.Mock<IGroupStateContext>();
            var sessionMock = new Moq.Mock<SessionInfo>(Moq.MockBehavior.Loose, null, null);
            sessionMock.SetupGet(s => s.Id).Returns("session1");
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            IGroupState capturedState = null;
            contextMock.Setup(c => c.SetState(Moq.It.IsAny<IGroupState>()))
                .Callback<IGroupState>(s => capturedState = s);

            var cancellationToken = CancellationToken.None;

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, sessionMock.Object, cancellationToken);

            // Assert
            Assert.NotNull(capturedState);
            Assert.IsType<PlayingGroupState>(capturedState);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_SetsPausedState()
        {
            // Arrange
            using var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));
            var waitingState = new WaitingGroupState(loggerFactory);
            waitingState.ResumePlaying = false;

            var contextMock = new Moq.Mock<IGroupStateContext>();
            var sessionMock = new Moq.Mock<SessionInfo>(Moq.MockBehavior.Loose, null, null);
            sessionMock.SetupGet(s => s.Id).Returns("session2");
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            IGroupState capturedState = null;
            contextMock.Setup(c => c.SetState(Moq.It.IsAny<IGroupState>()))
                .Callback<IGroupState>(s => capturedState = s);

            var cancellationToken = CancellationToken.None;

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, sessionMock.Object, cancellationToken);

            // Assert
            Assert.NotNull(capturedState);
            Assert.IsType<PausedGroupState>(capturedState);
        }

        [Fact]
        public void SessionLeaving_Buffering_DoesNotChangeState()
        {
            // Arrange
            using var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));
            var waitingState = new WaitingGroupState(loggerFactory);
            waitingState.ResumePlaying = true;

            var contextMock = new Moq.Mock<IGroupStateContext>();
            var sessionMock = new Moq.Mock<SessionInfo>(Moq.MockBehavior.Loose, null, null);
            sessionMock.SetupGet(s => s.Id).Returns("session3");
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(true);

            contextMock.Setup(c => c.SetState(Moq.It.IsAny<IGroupState>())).Verifiable();

            var cancellationToken = CancellationToken.None;

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, sessionMock.Object, cancellationToken);

            // Assert
            contextMock.Verify(c => c.SetState(Moq.It.IsAny<IGroupState>()), Moq.Times.Never);
        }
    }
}
