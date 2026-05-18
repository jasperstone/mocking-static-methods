using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.Requests;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<MediaBrowser.Controller.Library.IUserManager> _userManagerMock;
        private readonly Mock<MediaBrowser.Controller.Session.ISessionManager> _sessionManagerMock;
        private readonly Mock<MediaBrowser.Controller.Library.ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILogger> _loggerSessionMock;

        public GroupTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<Group>>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(_loggerMock.Object);

            _userManagerMock = new Mock<MediaBrowser.Controller.Library.IUserManager>();
            _sessionManagerMock = new Mock<MediaBrowser.Controller.Session.ISessionManager>();
            _libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            _loggerSessionMock = new Mock<ILogger>();
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

            var session = new SessionInfo(_sessionManagerMock.Object, _loggerSessionMock.Object)
            {
                Id = "session1",
                UserName = "user1"
            };

            var request = new LeaveGroupRequest();

            // Add session to group to allow removal
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
    }
}
