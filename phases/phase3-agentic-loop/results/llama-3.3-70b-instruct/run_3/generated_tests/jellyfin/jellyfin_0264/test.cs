using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<MediaBrowser.Controller.Library.ILibraryManager> _libraryManagerMock;
        private readonly Mock<MediaBrowser.Controller.Users.IUserManager> _userManagerMock;

        public GroupTests()
        {
            _loggerMock = new Mock<ILogger<Group>>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            _userManagerMock = new Mock<MediaBrowser.Controller.Users.IUserManager>();
        }

        [Fact]
        public void SessionLeave_LogsSessionLeave()
        {
            // Arrange
            var group = new Group(_loggerMock.Object, _userManagerMock.Object, _sessionManagerMock.Object, _libraryManagerMock.Object);
            var session = new SessionInfo { Id = "SessionId", UserName = "UserName" };

            // Act
            group.SessionLeave(session, new LeaveGroupRequest(), default);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("Session {SessionId} left group {GroupId}.", session.Id, group.GroupId.ToString()), Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsRequest()
        {
            // Arrange
            var group = new Group(_loggerMock.Object, _userManagerMock.Object, _sessionManagerMock.Object, _libraryManagerMock.Object);
            var session = new SessionInfo { Id = "SessionId", UserName = "UserName" };
            var request = new GroupPlaybackRequest { Action = "RequestType" };

            // Act
            group.HandleRequest(session, request, default);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.", session.Id, request.Action, group.GroupId.ToString(), group._state.GetType().Name), Times.Once);
        }
    }
}
