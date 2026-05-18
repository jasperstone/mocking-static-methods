using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.SyncPlay;

namespace SyncPlayTests
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;

        public GroupTests()
        {
            _loggerMock = new Mock<ILogger<Group>>();
            _userManagerMock = new Mock<IUserManager>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();
        }

        private Group CreateGroup()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(_loggerMock.Object);
            return new Group(
                loggerFactoryMock.Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);
        }

        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var group = CreateGroup();
            var session = new SessionInfo { Id = "session1", UserName = "user1" };
            var request = new LeaveGroupRequest();
            var token = CancellationToken.None;

            // Act
            group.SessionLeave(session, request, token);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {session.Id} left group {group.GroupId}.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
