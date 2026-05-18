using System;
using System.Threading;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.Requests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        [Fact]
        public void SessionJoin_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var userManagerMock = new Mock<IUserManager>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<Group>()).Returns(loggerMock.Object);

            var group = new Group(loggerFactoryMock.Object, userManagerMock.Object, sessionManagerMock.Object, libraryManagerMock.Object);
            var session = new SessionInfo(sessionManagerMock.Object, loggerMock.Object) { Id = "sessionId", UserName = "userName" };
            var request = new JoinGroupRequest(group.GroupId);
            var cancellationToken = new CancellationToken();

            // Act
            group.SessionJoin(session, request, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    "Session {SessionId} joined group {GroupId}.",
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }
    }
}
