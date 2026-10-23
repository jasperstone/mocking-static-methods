using System;
using System.Threading;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.Requests;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.SyncPlay
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _mockLogger;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<ISessionManager> _mockSessionManager;
        private readonly Group _group;

        public GroupTests()
        {
            _mockLogger = new Mock<ILogger<Group>>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory.Setup(f => f.CreateLogger<Group>()).Returns(_mockLogger.Object);
            _mockSessionManager = new Mock<ISessionManager>();

            // Create minimal mocks for constructor
            var nullLoggerFactory = _mockLoggerFactory.Object;
            var nullUserManager = Mock.Of<IUserManager>();
            var nullLibraryManager = Mock.Of<ILibraryManager>();

            _group = new Group(nullLoggerFactory, nullUserManager, _mockSessionManager.Object, nullLibraryManager);
        }

        [Fact]
        public void SessionLeave_ValidSession_LogsInformationMessage()
        {
            // Arrange
            var sessionId = "test-session-id";
            var session = new SessionInfo(_mockSessionManager.Object, Mock.Of<ILogger<SessionInfo>>())
            {
                Id = sessionId,
                UserName = "TestUser"
            };
            var request = new LeaveGroupRequest();
            var cancellationToken = new CancellationToken();

            // Mock the state field to prevent exceptions during SessionLeaving call
            var mockState = new Mock<IGroupState>();
            mockState.Setup(s => s.SessionLeaving(It.IsAny<Group>(), It.IsAny<GroupStateType>(), It.IsAny<SessionInfo>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            typeof(Group).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(_group, mockState.Object);

            // Act
            _group.SessionLeave(session, request, cancellationToken);

            // Assert - verify the LogInformation call on line 323
            _mockLogger.Verify(
                x => x.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    sessionId,
                    _group.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_ValidRequest_LogsInformationMessage()
        {
            // Arrange
            var sessionId = "test-session-id";
            var session = new SessionInfo(_mockSessionManager.Object, Mock.Of<ILogger<SessionInfo>>())
            {
                Id = sessionId,
                UserName = "TestUser"
            };

            // Create a simple request implementation
            var request = new Mock<IGroupPlaybackRequest>();
            request.Setup(r => r.Action).Returns("TestAction");

            var cancellationToken = new CancellationToken();

            // Mock state to handle Apply call
            var mockState = new Mock<IGroupState>();
            mockState.Setup(s => s.Type).Returns(GroupStateType.Idle);
            mockState.Setup(s => s.Apply(It.IsAny<Group>(), It.IsAny<IGroupState>(), It.IsAny<SessionInfo>(), It.IsAny<IGroupPlaybackRequest>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            typeof(Group).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(_group, mockState.Object);

            // Act
            _group.HandleRequest(session, request.Object, cancellationToken);

            // Assert - verify the LogInformation call
            _mockLogger.Verify(
                x => x.LogInformation(
                    "Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.",
                    sessionId,
                    "TestAction",
                    _group.GroupId.ToString(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
