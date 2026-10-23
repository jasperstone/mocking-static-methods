using System;
using System.Threading;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.SyncPlay;
using Emby.Server.Implementations.SyncPlay;

namespace SyncPlayTests
{
    public class GroupLoggingTests
    {
        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<Group>()).Returns(loggerMock.Object);

            var userManagerMock = new Mock<IUserManager>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var group = new Group(loggerFactoryMock.Object, userManagerMock.Object, sessionManagerMock.Object, libraryManagerMock.Object);

            var session = new SessionInfo { Id = "session1", UserName = "user1" };
            var request = new LeaveGroupRequest();

            // Act
            group.SessionLeave(session, request, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    session.Id,
                    group.GroupId.ToString()),
                Times.Once);
        }
    }
}
