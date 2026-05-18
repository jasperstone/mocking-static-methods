using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.Requests;
using System.Threading;

namespace Jellyfin.Tests.SyncPlay
{
    public class GroupTests
    {
        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger<Group>()).Returns(loggerMock.Object);

            var userManagerMock = new Mock<IUserManager>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var group = new Group(loggerFactoryMock.Object, userManagerMock.Object, sessionManagerMock.Object, libraryManagerMock.Object);

            var session = new SessionInfo(sessionManagerMock.Object, loggerMock.Object)
            {
                Id = "session1",
                UserName = "user1"
            };

            var request = new LeaveGroupRequest();

            // Act
            group.SessionLeave(session, request, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s == "Session {SessionId} left group {GroupId}."),
                    It.Is<object[]>(o => o[0].ToString() == "session1" && o[1].ToString() == group.GroupId.ToString())),
                Times.Once);
        }
    }
}
