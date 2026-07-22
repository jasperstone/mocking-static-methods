using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.SyncPlay;

namespace MediaBrowser.Controller.Tests.SyncPlay.GroupStates
{
    public class WaitingGroupStateTests
    {
        private SessionInfo CreateSession(string id)
        {
            // SessionInfo requires ISessionManager and ILogger, so we mock them
            var mockSessionManager = new Mock<ISessionManager>();
            var mockLogger = new Mock<ILogger>();
            var session = new SessionInfo(mockSessionManager.Object, mockLogger.Object)
            {
                Id = id
            };
            return session;
        }

        [Fact]
        public void SessionLeaving_ResumePlayingTrue_LogsNotifyResumeAndSetsPlayingState()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var waitingState = new WaitingGroupState(loggerFactory);
            waitingState.ResumePlaying = true;

            var mockContext = new Mock<IGroupStateContext>();
            var groupId = Guid.NewGuid();
            mockContext.Setup(c => c.GroupId).Returns(groupId);
            mockContext.Setup(c => c.IsBuffering()).Returns(false);

            var session = CreateSession("session1");

            IGroupState capturedState = null;
            mockContext.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => capturedState = state);

            // Act
            waitingState.SessionLeaving(mockContext.Object, GroupStateType.Playing, session, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedState);
            Assert.IsType<PlayingGroupState>(capturedState);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_LogsReturnToPreviousStateAndSetsPausedState()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var waitingState = new WaitingGroupState(loggerFactory);
            waitingState.ResumePlaying = false;

            var mockContext = new Mock<IGroupStateContext>();
            var groupId = Guid.NewGuid();
            mockContext.Setup(c => c.GroupId).Returns(groupId);
            mockContext.Setup(c => c.IsBuffering()).Returns(false);

            var session = CreateSession("session2");

            IGroupState capturedState = null;
            mockContext.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => capturedState = state);

            // Act
            waitingState.SessionLeaving(mockContext.Object, GroupStateType.Paused, session, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedState);
            Assert.IsType<PausedGroupState>(capturedState);
        }

        [Fact]
        public void SessionLeaving_ContextIsBuffering_DoesNotChangeState()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var waitingState = new WaitingGroupState(loggerFactory);
            waitingState.ResumePlaying = true;

            var mockContext = new Mock<IGroupStateContext>();
            mockContext.Setup(c => c.IsBuffering()).Returns(true);

            var session = CreateSession("session3");

            bool setStateCalled = false;
            mockContext.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback(() => setStateCalled = true);

            // Act
            waitingState.SessionLeaving(mockContext.Object, GroupStateType.Playing, session, CancellationToken.None);

            // Assert
            Assert.False(setStateCalled);
        }
    }
}
