using Xunit;
using Moq;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class TestableVideo : Video
    {
        public ILibraryManager LibraryManager { get; set; }
        public ILogger Logger { get; set; }
        public IFileSystem FileSystem { get; set; }

        protected override Task RefreshMetadataForOwnedVideo(
            MetadataRefreshOptions options,
            bool copyTitleMetadata,
            string path,
            Type itemType,
            CancellationToken cancellationToken)
        {
            return base.RefreshMetadataForOwnedVideo(options, copyTitleMetadata, path, itemType, cancellationToken);
        }
    }

    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_FileNoLongerExists_LogsInformationAndDeletesItem()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLogger = new Mock<ILogger<Video>>();
            var mockFileSystem = new Mock<IFileSystem>();

            var video = new Video
            {
                Id = Guid.NewGuid(),
                Path = "path/to/video"
            };

            mockLibraryManager.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns(video);
            mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            var videoInstance = new TestableVideo
            {
                LibraryManager = mockLibraryManager.Object,
                Logger = mockLogger.Object,
                FileSystem = mockFileSystem.Object
            };

            // Act
            await videoInstance.RefreshMetadataForOwnedVideo(
                new MetadataRefreshOptions(),
                false,
                "path/to/video",
                typeof(Video),
                CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Owned video file no longer exists, removing orphaned item: {Path}",
                    It.IsAny<string>()),
                Times.Once);

            mockLibraryManager.Verify(
                lm => lm.DeleteItem(
                    It.IsAny<Video>(),
                    It.IsAny<DeleteOptions>()),
                Times.Once);
        }
    }
}
