using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    // Minimal stubs for missing types
    public class SessionInfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }

    public class LeaveGroupRequest { }

    public interface IUserManager { }
    public interface ISessionManager { }
    public interface ILibraryManager { }

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

            _loggerFactoryMock.Setup(x => x.CreateLogger<Group>()).Returns(_loggerMock.Object);
        }

        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var group = new Emby.Server.Implementations.SyncPlay.Group(
                _loggerFactoryMock.Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);

            var session = new SessionInfo
            {
                Id = "session1",
                UserName = "user1"
            };

            var leaveRequest = new LeaveGroupRequest();

            // Act
            group.SessionLeave(session, leaveRequest, CancellationToken.None);

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
