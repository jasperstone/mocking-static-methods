using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.Requests;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Logging;
using Moq;
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
        public void SessionLeave_LogsInformationMessage()
        {
            // Arrange
            var group = CreateGroup();
            var session = new SessionInfo
            {
                Id = "test-session-id",
                UserName = "test-user"
            };
            var request = new LeaveGroupRequest();
            var cancellationToken = new CancellationToken();

            // Act
            group.SessionLeave(session, request, cancellationToken);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    "test-session-id",
                    It.IsAny<Guid>().ToString()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsInformationMessage()
        {
            // Arrange
            var group = CreateGroup();
            var session = new SessionInfo
            {
                Id = "test-session-id"
            };
            var request = new Mock<IGroupPlaybackRequest>();
            request.Setup(r => r.Action).Returns("TestAction");
            var cancellationToken = new CancellationToken();

            // Act
            group.HandleRequest(session, request.Object, cancellationToken);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.",
                    "test-session-id",
                    "TestAction",
                    It.IsAny<Guid>().ToString(),
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
    }
}
