using System;
using System.Threading;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.Requests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
        var session = new SessionInfo { Id = "session1", UserName = "user1" };

        // Act
        group.SessionJoin(session, new JoinGroupRequest(), CancellationToken.None);

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
        var session = new SessionInfo { Id = "session1", UserName = "user1" };

        // Act
        group.SessionLeave(session, new LeaveGroupRequest(), CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                "Session {SessionId} left group {GroupId}.",
                It.IsAny<object[]>()),
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
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger<Group>()).Returns(loggerMock.Object);

        var group = new Group(loggerFactoryMock.Object, userManagerMock.Object, sessionManagerMock.Object, libraryManagerMock.Object);
        var session = new SessionInfo { Id = "session1", UserName = "user1" };
        var requestMock = new Mock<IGroupPlaybackRequest>();

        // Act
        group.HandleRequest(session, requestMock.Object, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                "Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.",
                It.IsAny<object[]>()),
            Times.Once);
    }
}
