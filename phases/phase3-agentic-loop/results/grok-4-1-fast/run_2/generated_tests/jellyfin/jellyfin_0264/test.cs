using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.SyncPlay.Requests;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _mockLogger;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<IUserManager> _mockUserManager;
        private readonly Mock<ISessionManager> _mockSessionManager;
        private readonly Mock<ILibraryManager> _mockLibraryManager;

        public GroupTests()
        {
            _mockLogger = new Mock<ILogger<Group>>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory.Setup(f => f.CreateLogger<Group>()).Returns(_mockLogger.Object);
            _mockUserManager = new Mock<IUserManager>();
            _mockSessionManager = new Mock<ISessionManager>();
            _mockLibraryManager = new Mock<ILibraryManager>();
        }

        [Fact]
        public void SessionLeave_LogsInformationMessageWithCorrectParameters()
        {
            // Arrange
            var group = CreateGroup();
            var session = new SessionInfo(_mockSessionManager.Object, Mock.Of<ILogger<SessionInfo>>())
            {
                Id = "test-session-id"
            };
            AddSessionViaReflection(group, session);
            var request = new LeaveGroupRequest();
            var cancellationToken = CancellationToken.None;

            // Mock state to avoid exceptions
            MockStateForLeave(group);

            // Act
            group.SessionLeave(session, request, cancellationToken);

            // Assert - verify the LogInformation extension method call on line 323
            _mockLogger.Verify(
                x => x.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    "test-session-id",
                    group.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public void SessionJoin_LogsInformationMessage()
        {
            // Arrange
            var group = CreateGroup();
            var session = new SessionInfo(_mockSessionManager.Object, Mock.Of<ILogger<SessionInfo>>())
            {
                Id = "test-session-id",
                UserName = "test-user"
            };
            var request = new JoinGroupRequest(group.GroupId);
            var cancellationToken = CancellationToken.None;

            // Mock state to avoid exceptions
            MockStateForJoin(group);

            // Act
            group.SessionJoin(session, request, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(
                    "Session {SessionId} joined group {GroupId}.",
                    "test-session-id",
                    group.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsInformationMessage()
        {
            // Arrange
            var group = CreateGroup();
            var session = new SessionInfo(_mockSessionManager.Object, Mock.Of<ILogger<SessionInfo>>())
            {
                Id = "test-session-id"
            };
            var mockRequest = new Mock<IGroupPlaybackRequest>();
            mockRequest.Setup(r => r.Action).Returns(PlaybackRequestType.Play);
            var cancellationToken = CancellationToken.None;

            // Mock state to avoid exceptions during request.Apply call
            MockMinimalState(group);

            // Act
            group.HandleRequest(session, mockRequest.Object, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(
                    "Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.",
                    "test-session-id",
                    PlaybackRequestType.Play,
                    group.GroupId.ToString(),
                    It.IsAny<string>()),
                Times.Once);
        }

        private Group CreateGroup()
        {
            return new Group(
                _mockLoggerFactory.Object,
                _mockUserManager.Object,
                _mockSessionManager.Object,
                _mockLibraryManager.Object);
        }

        private void MockStateForLeave(Group group)
        {
            var stateField = typeof(Group).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);
            var mockState = new Mock<IGroupState>();
            mockState.Setup(s => s.SessionLeaving(It.IsAny<IGroupStateContext>(), It.IsAny<GroupStateType>(), It.IsAny<SessionInfo>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(0));
            stateField?.SetValue(group, mockState.Object);
        }

        private void MockStateForJoin(Group group)
        {
            var stateField = typeof(Group).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);
            var mockState = new Mock<IGroupState>();
            mockState.Setup(s => s.SessionJoining(It.IsAny<IGroupStateContext>(), It.IsAny<GroupStateType>(), It.IsAny<SessionInfo>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(0));
            mockState.Setup(s => s.SessionJoined(It.IsAny<IGroupStateContext>(), It.IsAny<GroupStateType>(), It.IsAny<SessionInfo>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(0));
            stateField?.SetValue(group, mockState.Object);
        }

        private void MockMinimalState(Group group)
        {
            var stateField = typeof(Group).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);
            var mockState = new Mock<IGroupState>();
            // Minimal setup to avoid exceptions - just return completed tasks for any async calls
            mockState.Setup(s => s.Apply(It.IsAny<IGroupPlaybackRequest>(), It.IsAny<IGroupState>(), It.IsAny<SessionInfo>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(0));
            stateField?.SetValue(group, mockState.Object);
        }

        private void AddSessionViaReflection(Group group, SessionInfo session)
        {
            var addSessionMethod = typeof(Group).GetMethod("AddSession", BindingFlags.NonPublic | BindingFlags.Instance);
            addSessionMethod?.Invoke(group, new[] { session });
        }
    }
}
