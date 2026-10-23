using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsWhenFileDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var video = new Video
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid()
            };

            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>()))
                .Returns(video);

            fileSystemMock.Setup(m => m.FileExists(It.IsAny<string>()))
                .Returns(false);

            var videoInstance = new Video
            {
                Logger = loggerMock.Object,
                FileSystem = fileSystemMock.Object,
                LibraryManager = libraryManagerMock.Object
            };

            // Act
            await videoInstance.RefreshMetadataForOwnedVideo(
                new MetadataRefreshOptions(),
                false,
                "path/to/nonexistent/file",
                CancellationToken.None);

            // Assert
            loggerMock.Verify(
                m => m.LogInformation(
                    It.Is<string>(s => s.Contains("Owned video file no longer exists, removing orphaned item: {Path}")),
                    It.Is<string>(s => s == "path/to/nonexistent/file")),
                Times.Once);
        }
    }
}
