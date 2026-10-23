using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Model.Dto;
using System;
using System.Collections.Generic;
using Emby.Server.Implementations.Session;

public class SessionManagerTests
{
    [Fact]
    public void TranslateItemForInstantMix_LogsError_WhenItemDoesNotExist()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SessionManager>>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var musicManagerMock = new Mock<IMusicManager>();

        var sessionManager = new SessionManager(
            loggerMock.Object,
            null,
            null,
            null,
            libraryManagerMock.Object,
            null,
            musicManagerMock.Object,
            null,
            null,
            null,
            null,
            null,
            null);

        var nonExistentItemId = Guid.NewGuid();
        var user = new User();

        // Act
        var result = sessionManager.TranslateItemForInstantMix(nonExistentItemId, user);

        // Assert
        loggerMock.Verify(
            x => x.LogError("A nonexistent item Id {0} was passed into TranslateItemForInstantMix", nonExistentItemId),
            Times.Once);

        Assert.Empty(result);
    }
}
