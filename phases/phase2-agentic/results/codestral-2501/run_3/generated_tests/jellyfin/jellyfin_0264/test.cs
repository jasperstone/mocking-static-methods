using System;
using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.Requests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Group _group;

        public GroupTests()
        {
            _loggerMock = new Mock<ILogger<Group>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _userManagerMock = new Mock<IUserManager>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();

            _loggerFactoryMock.Setup(factory => factory.CreateLogger<Group>()).Returns(_loggerMock.Object);

            _group = new Group(
                _loggerFactoryMock.Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);
        }

        [Fact]
        public void SessionJoin_LogsInformation()
        {
            // Arrange
            var session = new SessionInfo
            {
                Id = "session1",
                UserName = "user1"
            };

            // Act
            _group.SessionJoin(session, new JoinGroupRequest(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
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
            var session = new SessionInfo
            {
                Id = "session1",
                UserName = "user1"
            };

            // Act
            _group.SessionLeave(session, new LeaveGroupRequest(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
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
            var session = new SessionInfo
            {
                Id = "session1",
                UserName = "user1"
            };
            var request = new Mock<IGroupPlaybackRequest>();
            request.Setup(r => r.Action).Returns("Play");

            // Act
            _group.HandleRequest(session, request.Object, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session1 requested Play in group")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
