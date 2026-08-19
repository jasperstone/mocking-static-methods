using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
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
        [Fact]
        public async Task SessionJoin_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var group = new Group(
                new LoggerFactory(),
                Mock.Of<IUserManager>(),
                Mock.Of<ISessionManager>(),
                Mock.Of<ILibraryManager>()
            );
            group._logger = loggerMock.Object;
            var session = new SessionInfo { Id = "SessionId", UserName = "UserName" };

            // Act
            group.SessionJoin(session, new JoinGroupRequest(), CancellationToken.None);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} joined group {GroupId}.",
                    session.Id,
                    group.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public async Task SessionLeave_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var group = new Group(
                new LoggerFactory(),
                Mock.Of<IUserManager>(),
                Mock.Of<ISessionManager>(),
                Mock.Of<ILibraryManager>()
            );
            group._logger = loggerMock.Object;
            var session = new SessionInfo { Id = "SessionId", UserName = "UserName" };

            // Act
            group.SessionLeave(session, new LeaveGroupRequest(), CancellationToken.None);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    session.Id,
                    group.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public async Task HandleRequest_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var group = new Group(
                new LoggerFactory(),
                Mock.Of<IUserManager>(),
                Mock.Of<ISessionManager>(),
                Mock.Of<ILibraryManager>()
            );
            group._logger = loggerMock.Object;
            var session = new SessionInfo { Id = "SessionId", UserName = "UserName" };
            var request = new PlayGroupRequest();

            // Act
            group.HandleRequest(session, request, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    "Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.",
                    session.Id,
                    request.Action,
                    group.GroupId.ToString(),
                    group._state.Type),
                Times.Once);
        }
    }
}
