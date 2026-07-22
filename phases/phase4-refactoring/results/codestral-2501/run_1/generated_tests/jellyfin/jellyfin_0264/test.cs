using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Controller.SyncPlay.Requests;
using MediaBrowser.Model.SyncPlay;
using System;
using System.Threading;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.SyncPlay.Queue;

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
            var cancellationToken = new CancellationToken();

            // Act
            group.SessionJoin(session, new JoinGroupRequest(Guid.NewGuid()), cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    "Session {SessionId} joined group {GroupId}.",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void SessionLeave_LogsInformation()
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
            var cancellationToken = new CancellationToken();

            // Act
            group.SessionLeave(session, new LeaveGroupRequest(), cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
