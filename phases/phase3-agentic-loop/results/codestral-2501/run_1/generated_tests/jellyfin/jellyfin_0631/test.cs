using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Providers;

public class VideoTests
{
    [Fact]
    public async Task RefreshMetadataForOwnedVideo_FileNoLongerExists_LogsInformationAndDeletesItem()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Video>>();
        var mockLibraryManager = new Mock<ILibraryManager>();
        var mockFileSystem = new Mock<IFileSystem>();

        var video = new Video { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        var path = "path/to/video";

        mockFileSystem.Setup(fs => fs.FileExists(path)).Returns(false);
        mockLibraryManager.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns(video);
        mockLibraryManager.Setup(lm => lm.DeleteItem(video, It.IsAny<DeleteOptions>()));

        var videoInstance = new VideoDerived
        {
            Logger = mockLogger.Object,
            LibraryManager = mockLibraryManager.Object,
            FileSystem = mockFileSystem.Object
        };

        // Act
        await videoInstance.RefreshMetadataForOwnedVideo(
            new MetadataRefreshOptions(mockFileSystem.Object),
            false,
            path,
            CancellationToken.None);

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Owned video file no longer exists, removing orphaned item: {Path}")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        mockLibraryManager.Verify(lm => lm.DeleteItem(video, It.IsAny<DeleteOptions>()), Times.Once);
    }

    private class VideoDerived : Video
    {
        public new ILogger<Video> Logger { get; set; }
        public new ILibraryManager LibraryManager { get; set; }
        public new IFileSystem FileSystem { get; set; }

        public new Task RefreshMetadataForOwnedVideo(
            MetadataRefreshOptions options,
            bool copyTitleMetadata,
            string path,
            CancellationToken cancellationToken)
        {
            return base.RefreshMetadataForOwnedVideo(options, copyTitleMetadata, path, cancellationToken);
        }
    }
}
