using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using Xunit;
using Emby.Server.Implementations.SyncPlay;

namespace Emby.Server.Tests.SyncPlay
{
    public class GroupTests
    {
        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Group>>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockUserManager = new Mock<IUserManager>();
            var mockLibraryManager = new Mock<ILibraryManager>();

            var group = new Group(
                mockLogger.Object.CreateLogger,
                mockUserManager.Object,
                mockSessionManager.Object,
                mockLibraryManager.Object);

            var session = new SessionInfo { Id = "session1", UserName = "user1" };
            var request = new LeaveGroupRequest();

            // Act
            group.SessionLeave(session, request, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session {SessionId} left group {GroupId}.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
