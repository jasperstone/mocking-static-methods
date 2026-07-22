using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Tests
{
    public class GroupTests
    {
        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var userManagerMock = new Mock<IUserManager>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var group = new Group(
                new LoggerFactory(),
                userManagerMock.Object,
                sessionManagerMock.Object,
                libraryManagerMock.Object
            );
            group._logger = loggerMock.Object;
            var session = new SessionInfo { Id = "SessionId", UserName = "UserName" };

            // Act
            group.SessionLeave(session, new LeaveGroupRequest { GroupId = group.GroupId, SessionId = session.Id }, default);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    It.IsAny<object>(),
                    It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var userManagerMock = new Mock<IUserManager>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var group = new Group(
                new LoggerFactory(),
                userManagerMock.Object,
                sessionManagerMock.Object,
                libraryManagerMock.Object
            );
            group._logger = loggerMock.Object;
            var session = new SessionInfo { Id = "SessionId" };
            var request = new GroupPlaybackRequest { Action = "Action" };

            // Act
            group.HandleRequest(session, request, default);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.",
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>()),
                Times.Once);
        }
    }
}
