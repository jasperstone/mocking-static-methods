using System;
using System.Threading;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(loggerMock.Object);

            var userManagerMock = new Mock<IUserManager>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var sessionLoggerMock = new Mock<ILogger<SessionInfo>>();

            var group = new Group(loggerFactoryMock.Object, userManagerMock.Object, sessionManagerMock.Object, libraryManagerMock.Object);

            var session = new SessionInfo(sessionManagerMock.Object, sessionLoggerMock.Object)
            {
                Id = "sessionId",
                UserName = "userName"
            };

            var cancellationToken = new CancellationToken();

            // Act
            group.SessionJoin(session, new JoinGroupRequest(group.GroupId), cancellationToken);

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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(loggerMock.Object);

            var userManagerMock = new Mock<IUserManager>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var sessionLoggerMock = new Mock<ILogger<SessionInfo>>();

            var group = new Group(loggerFactoryMock.Object, userManagerMock.Object, sessionManagerMock.Object, libraryManagerMock.Object);

            var session = new SessionInfo(sessionManagerMock.Object, sessionLoggerMock.Object)
            {
                Id = "sessionId",
                UserName = "userName"
            };

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
