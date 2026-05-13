using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.SyncPlay;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;

        public GroupTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<Group>>();
            _userManagerMock = new Mock<IUserManager>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();

            _loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(_loggerMock.Object);
        }

        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var group = new Group(
                _loggerFactoryMock.Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);

            var session = new SessionInfo
            {
                Id = "session1",
                UserName = "user1"
            };

            var request = new LeaveGroupRequest();

            // Add session to group to avoid errors in RemoveSession
            var addSessionMethod = typeof(Group).GetMethod("AddSession", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            addSessionMethod.Invoke(group, new object[] { session });

            // Act
            group.SessionLeave(session, request, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session1 left group")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void SessionJoin_LogsInformation()
        {
            // Arrange
            var group = new Group(
                _loggerFactoryMock.Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);

            var session = new SessionInfo
            {
                Id = "session2",
                UserName = "user2"
            };

            // Act
            group.SessionJoin(session, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session2 joined group")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsInformation()
        {
            // Arrange
            var group = new Group(
                _loggerFactoryMock.Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);

            var session = new SessionInfo
            {
                Id = "session3",
                UserName = "user3"
            };

            var requestMock = new Mock<IGroupPlaybackRequest>();
            requestMock.Setup(r => r.Action).Returns("Play");
            requestMock.Setup(r => r.Apply(It.IsAny<Group>(), It.IsAny<IGroupState>(), session, It.IsAny<CancellationToken>()));

            // Act
            group.HandleRequest(session, requestMock.Object, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session3 requested Play")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            requestMock.Verify(r => r.Apply(group, It.IsAny<IGroupState>(), session, CancellationToken.None), Times.Once);
        }
    }
}
