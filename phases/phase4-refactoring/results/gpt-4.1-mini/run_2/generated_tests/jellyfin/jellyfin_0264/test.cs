using System;
using System.Threading;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.Requests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.SyncPlay;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<Group>>();
            loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(loggerMock.Object);

            var userManagerMock = new Mock<MediaBrowser.Controller.Library.IUserManager>();
            var sessionManagerMock = new Mock<MediaBrowser.Controller.Session.ISessionManager>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();

            var group = new Group(loggerFactoryMock.Object, userManagerMock.Object, sessionManagerMock.Object, libraryManagerMock.Object);

            var sessionLoggerMock = new Mock<ILogger>();
            var session = new MediaBrowser.Controller.Session.SessionInfo(sessionManagerMock.Object, sessionLoggerMock.Object)
            {
                Id = "session1",
                UserName = "user1"
            };

            var leaveRequest = new MediaBrowser.Controller.SyncPlay.Requests.LeaveGroupRequest();

            // Act
            group.SessionLeave(session, leaveRequest, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session session1 left group")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
