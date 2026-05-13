using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Group _group;

        public GroupTests()
        {
            _loggerMock = new Mock<ILogger<Group>>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _userManagerMock = new Mock<IUserManager>();

            _group = new Group(
                new LoggerFactory(),
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);
        }

        [Fact]
        public void SessionLeave_LogsSessionLeave()
        {
            // Arrange
            var session = new SessionInfo
            {
                Id = "SessionId",
                UserName = "UserName"
            };

            // Act
            _group.SessionLeave(session, new LeaveGroupRequest(), default);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    session.Id,
                    _group.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsSessionRequest()
        {
            // Arrange
            var session = new SessionInfo
            {
                Id = "SessionId",
                UserName = "UserName"
            };

            var request = new PlayGroupRequest();

            // Act
            _group.HandleRequest(session, request, default);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.",
                    session.Id,
                    request.Action,
                    _group.GroupId.ToString(),
                    _group._state.GetType().Name),
                Times.Once);
        }
    }
}
