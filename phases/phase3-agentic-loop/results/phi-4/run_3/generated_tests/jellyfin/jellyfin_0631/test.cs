using System;
using System.Threading;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Xunit;

public class VideoTests
{
    [Fact]
    public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileDoesNotExist()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<Video>>();
        var libraryManagerMock = new Mock<LibraryManager>();
        var fileSystemMock = new Mock<IFileSystem>();

        var video = new Video
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid()
        };

        libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>()))
            .Returns(video);

        fileSystemMock.Setup(m => m.FileExists(It.IsAny<string>()))
            .Returns(false);

        var options = new MetadataRefreshOptions();
        var cancellationToken = new CancellationToken();

        // Act
        await ((Video)video).RefreshMetadataForOwnedVideo(options, false, "fakePath", typeof(Video), cancellationToken);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation(
                It.IsAny<EventId>(),
                It.Is<object>(o => o.ToString() == "Owned video file no longer exists, removing orphaned item: {Path}"),
                null,
                It.IsAny<Func<string, Exception, string>>(),
                It.Is<object[]>(a => a.Length == 1 && a[0].ToString() == "fakePath")),
            Times.Once);
    }
}
