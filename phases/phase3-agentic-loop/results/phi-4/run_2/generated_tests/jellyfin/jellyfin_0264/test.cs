using System;
using System.Threading;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Emby.Server.Implementations.SyncPlay;
using Emby.Server.Implementations.Controller.Session;
using Emby.Server.Implementations.Controller.Users;
using Emby.Server.Implementations.Database.Entities;
using Emby.Server.Implementations.Library;

public class GroupTests
{
    [Fact]
    public void SessionLeave_LogsInformationCorrectly()
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

        var session = new SessionInfo
        {
            Id = Guid.NewGuid(),
            UserName = "TestUser"
        };

        // Act
        group.SessionLeave(session, null, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation(
                "Session {SessionId} left group {GroupId}.",
                session.Id,
                group.GroupId.ToString()),
            Times.Once);
    }
}
