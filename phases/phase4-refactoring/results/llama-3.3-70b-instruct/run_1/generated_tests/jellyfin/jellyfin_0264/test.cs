using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.SyncPlay.Queue;
using MediaBrowser.Controller.SyncPlay.Requests;
using MediaBrowser.Model.SyncPlay;

namespace Emby.Server.Implementations.SyncPlay
{
    public class GroupTests
    {
        [Fact]
        public void SessionJoin_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var userManagerMock = new Mock<IUserManager>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var group = new Group(
                loggerFactoryMock.Object,
                userManagerMock.Object,
                sessionManagerMock.Object,
                libraryManagerMock.Object
            );

            var session = new SessionInfo(sessionManagerMock.Object, loggerFactoryMock.Object.CreateLogger<SessionInfo>(), "SessionId", "UserName");

            // Act
            group.SessionJoin(session, new JoinGroupRequest(group.GroupId), default);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} joined group {GroupId}.",
                    session.Id,
                    group.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var userManagerMock = new Mock<IUserManager>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var group = new Group(
                loggerFactoryMock.Object,
                userManagerMock.Object,
                sessionManagerMock.Object,
                libraryManagerMock.Object
            );

            var session = new SessionInfo(sessionManagerMock.Object, loggerFactoryMock.Object.CreateLogger<SessionInfo>(), "SessionId", "UserName");

            // Act
            group.SessionLeave(session, new LeaveGroupRequest(), default);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    session.Id,
                    group.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var userManagerMock = new Mock<IUserManager>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var group = new Group(
                loggerFactoryMock.Object,
                userManagerMock.Object,
                sessionManagerMock.Object,
                libraryManagerMock.Object
            );

            var session = new SessionInfo(sessionManagerMock.Object, loggerFactoryMock.Object.CreateLogger<SessionInfo>(), "SessionId", "UserName");
            var request = new PlayGroupRequest();

            // Act
            group.HandleRequest(session, request, default);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.",
                    session.Id,
                    request.Action,
                    group.GroupId.ToString(),
                    group._state.GetType().Name),
                Times.Once);
        }
    }
}
