using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_FileDoesNotExist_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var video = new Video
            {
                Path = "somepath",
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid()
            };
            var options = new MetadataRefreshOptions();
            var cancellationToken = CancellationToken.None;

            // Setup LibraryManager to return false for FileExists
            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((Video)null);
            // Setup FileSystem to return false for FileExists
            fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            // Inject dependencies
            video.LibraryManager = libraryManagerMock.Object;
            video.Logger = loggerMock.Object;
            video.FileSystem = fileSystemMock.Object;

            // Act
            await video.RefreshMetadataForOwnedVideo(options, false, "somepath", cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", "somepath"),
                Times.Once);
        }
    }
}
