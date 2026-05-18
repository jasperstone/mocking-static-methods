using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<IGroupState> _stateMock;
        private readonly Group _group;

        public GroupTests()
        {
            _loggerMock = new Mock<ILogger<Group>>();
            _stateMock = new Mock<IGroupState>();
            _group = new Group(Mock.Of<ILoggerFactory>(), Mock.Of<IUserManager>(), Mock.Of<ISessionManager>(), Mock.Of<ILibraryManager>());
            _group._logger = _loggerMock.Object;
            _group._state = _stateMock.Object;
        }

        [Fact]
        public async Task SessionJoin_LogsInformation()
        {
            // Arrange
            var session = new SessionInfo { Id = "SessionId", UserName = "UserName" };
            var groupId = Guid.NewGuid();

            // Act
            _group.SessionJoin(session, new JoinGroupRequest { GroupId = groupId }, CancellationToken.None);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.Is<string>(s => s.Contains("Session SessionId joined group"))), Times.Once);
        }

        [Fact]
        public async Task SessionLeave_LogsInformation()
        {
            // Arrange
            var session = new SessionInfo { Id = "SessionId", UserName = "UserName" };
            var groupId = Guid.NewGuid();

            // Act
            _group.SessionLeave(session, new LeaveGroupRequest { GroupId = groupId }, CancellationToken.None);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.Is<string>(s => s.Contains("Session SessionId left group"))), Times.Once);
        }

        [Fact]
        public async Task HandleRequest_LogsInformation()
        {
            // Arrange
            var session = new SessionInfo { Id = "SessionId", UserName = "UserName" };
            var request = new PlayGroupRequest { Action = "Play" };
            var groupId = Guid.NewGuid();

            // Act
            _group.HandleRequest(session, request, CancellationToken.None);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.Is<string>(s => s.Contains("Session SessionId requested Play in group"))), Times.Once);
        }
    }
}
