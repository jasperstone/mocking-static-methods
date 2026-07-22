using System;
using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.SyncPlay.Requests;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        [Fact]
        public void SessionLeave_ValidSession_LogsInformationMessage()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<Group>>();
            mockLoggerFactory.Setup(f => f.CreateLogger<Group>()).Returns(mockLogger.Object);
            var mockUserManager = new Mock<IUserManager>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            
            var group = new Group(
                mockLoggerFactory.Object,
                mockUserManager.Object,
                mockSessionManager.Object,
                mockLibraryManager.Object);
            
            var session = new SessionInfo(mockSessionManager.Object, mockLogger.Object)
            {
                Id = "test-session-id",
                UserName = "test-user"
            };

            // Act
            group.SessionLeave(session, new LeaveGroupRequest(), CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    "test-session-id",
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void SessionJoin_ValidSession_LogsInformationMessage()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<Group>>();
            mockLoggerFactory.Setup(f => f.CreateLogger<Group>()).Returns(mockLogger.Object);
            var mockUserManager = new Mock<IUserManager>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            
            var group = new Group(
                mockLoggerFactory.Object,
                mockUserManager.Object,
                mockSessionManager.Object,
                mockLibraryManager.Object);
            
            var session = new SessionInfo(mockSessionManager.Object, mockLogger.Object)
            {
                Id = "test-session-id",
                UserName = "test-user"
            };

            // Act
            group.SessionJoin(session, new JoinGroupRequest(Guid.NewGuid()), CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} joined group {GroupId}.",
                    "test-session-id",
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_ValidRequest_LogsInformationMessage()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<Group>>();
            mockLoggerFactory.Setup(f => f.CreateLogger<Group>()).Returns(mockLogger.Object);
            var mockUserManager = new Mock<IUserManager>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            
            var group = new Group(
                mockLoggerFactory.Object,
                mockUserManager.Object,
                mockSessionManager.Object,
                mockLibraryManager.Object);
            
            var session = new SessionInfo(mockSessionManager.Object, mockLogger.Object)
            {
                Id = "test-session-id"
            };

            // Mock state to return a string
            var mockState = new Mock<IGroupState>();
            mockState.Setup(s => s.Type).Returns("Idle");
            typeof(Group).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(group, mockState.Object);

            var mockRequest = new Mock<IGroupPlaybackRequest>();
            mockRequest.Setup(r => r.Action).Returns(PlaybackRequestType.Play);

            // Act
            group.HandleRequest(session, mockRequest.Object, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.",
                    "test-session-id",
                    PlaybackRequestType.Play,
                    It.IsAny<string>(),
                    "Idle"),
                Times.Once);
        }
    }
}
