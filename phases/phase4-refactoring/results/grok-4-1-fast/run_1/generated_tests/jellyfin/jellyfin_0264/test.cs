#nullable enable

using System;
using System.Threading;
using Emby.Server.Implementations.SyncPlay;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.Requests;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.SyncPlay
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Group _group;

        public GroupTests()
        {
            _loggerMock = new Mock<ILogger<Group>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(_loggerMock.Object);

            // Create minimal mocks with required namespaces
            var userManagerMock = new Mock<MediaBrowser.Controller.Net.IUserManager>();
            var sessionManagerMock = new Mock<MediaBrowser.Controller.ISessionManager>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();

            _group = new Group(
                _loggerFactoryMock.Object,
                userManagerMock.Object,
                sessionManagerMock.Object,
                libraryManagerMock.Object);
        }

        [Fact]
        public void SessionLeave_ValidSession_LogsInformationMessage()
        {
            // Arrange
            var sessionId = "test-session-id";
            var session = new SessionInfo
            {
                Id = sessionId,
                UserName = "TestUser"
            };
            var request = new LeaveGroupRequest();
            var cancellationToken = new CancellationToken();

            // Add session first using reflection (private method)
            var addSessionMethod = typeof(Group).GetMethod("AddSession", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            addSessionMethod!.Invoke(_group, [session]);

            // Mock state to prevent exceptions
            var stateMock = new Mock<IGroupState>();
            var stateField = typeof(Group).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stateField!.SetValue(_group, stateMock.Object);
            stateMock.Setup(s => s.SessionLeaving(It.IsAny<Group>(), It.IsAny<GroupStateType>(), It.IsAny<SessionInfo>(), It.IsAny<CancellationToken>()));

            // Act
            _group.SessionLeave(session, request, cancellationToken);

            // Assert - specifically tests line 323 LogInformation call
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    session.Id,
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_ValidRequest_LogsInformationMessage()
        {
            // Arrange
            var sessionId = "test-session-id";
            var session = new SessionInfo { Id = sessionId };
            var requestMock = new Mock<IGroupPlaybackRequest>();
            requestMock.Setup(r => r.Action).Returns("TestAction");
            var cancellationToken = new CancellationToken();

            // Mock state
            var stateMock = new Mock<IGroupState>();
            var stateField = typeof(Group).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stateField!.SetValue(_group, stateMock.Object);
            requestMock.Setup(r => r.Apply(It.IsAny<Group>(), It.IsAny<IGroupState>(), It.IsAny<SessionInfo>(), It.IsAny<CancellationToken>()));

            // Act
            _group.HandleRequest(session, requestMock.Object, cancellationToken);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.",
                    session.Id,
                    "TestAction",
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
