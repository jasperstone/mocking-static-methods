using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Tests.Entities
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_FileDoesNotExist_ShouldLogInformationAndReturn()
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

            // Setup LibraryManager to return a Video with matching OwnerId
            var ownerId = Guid.NewGuid();
            var ownedVideo = new Video { OwnerId = ownerId, Id = Guid.NewGuid() };
            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns(ownedVideo);
            // Setup FileExists to return false
            fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            // Inject dependencies
            var refreshMethod = typeof(Video).GetMethod("RefreshMetadataForOwnedVideo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var instance = video;

            // Act
            var task = (Task)refreshMethod.Invoke(instance, new object[]
            {
                new MetadataRefreshOptions(),
                false,
                "somepath",
                CancellationToken.None
            });
            await task;

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", "somepath"),
                Times.Once);
        }
    }
}
