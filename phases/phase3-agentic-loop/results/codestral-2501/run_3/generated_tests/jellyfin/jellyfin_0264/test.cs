using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.Requests;
using MediaBrowser.Model.SyncPlay;
using System.Threading;
using System;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Library;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _mockLogger;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<IUserManager> _mockUserManager;
        private readonly Mock<ISessionManager> _mockSessionManager;
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Group _group;

        public GroupTests()
        {
            _mockLogger = new Mock<ILogger<Group>>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockUserManager = new Mock<IUserManager>();
            _mockSessionManager = new Mock<ISessionManager>();
            _mockLibraryManager = new Mock<ILibraryManager>();

            _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);

            _group = new Group(_mockLoggerFactory.Object, _mockUserManager.Object, _mockSessionManager.Object, _mockLibraryManager.Object);
        }

        [Fact]
        public void SessionJoin_LogsInformation()
        {
            // Arrange
            var session = new SessionInfo { Id = "session1", UserName = "user1" };
            var request = new JoinGroupRequest { GroupId = Guid.NewGuid() };
            var cancellationToken = CancellationToken.None;

            // Act
            _group.SessionJoin(session, request, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session1 joined group")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var session = new SessionInfo { Id = "session1", UserName = "user1" };
            var request = new LeaveGroupRequest();
            var cancellationToken = CancellationToken.None;

            // Act
            _group.SessionLeave(session, request, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session1 left group")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsInformation()
        {
            // Arrange
            var session = new SessionInfo { Id = "session1", UserName = "user1" };
            var request = new Mock<IGroupPlaybackRequest>();
            request.Setup(x => x.Action).Returns("Play");
            var cancellationToken = CancellationToken.None;

            // Act
            _group.HandleRequest(session, request.Object, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session1 requested Play in group")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
