using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

public class VideoTests
{
    [Fact]
    public async Task RefreshMetadataForOwnedVideo_FileNoLongerExists_LogsInformationAndDeletesItem()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<Video>>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var fileSystemMock = new Mock<IFileSystem>();
        var metadataRefreshOptionsMock = new Mock<MetadataRefreshOptions>();

        var video = new Video();
        var orphanedVideo = new Video { OwnerId = video.Id };

        string path = "path/to/video";
        var id = Guid.NewGuid();

        libraryManagerMock.Setup(lm => lm.GetNewItemId(path, typeof(Video))).Returns(id);
        libraryManagerMock.Setup(lm => lm.GetItemById(id)).Returns(orphanedVideo);
        fileSystemMock.Setup(fs => fs.FileExists(path)).Returns(false);

        video.LibraryManager = libraryManagerMock.Object;
        video.FileSystem = fileSystemMock.Object;
        video.Logger = loggerMock.Object;

        // Act
        await video.RefreshMetadataForOwnedVideo(metadataRefreshOptionsMock.Object, false, path, CancellationToken.None);

        // Assert
        loggerMock.Verify(logger => logger.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", path), Times.Once);
        libraryManagerMock.Verify(lm => lm.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>()), Times.Once);
    }
}
