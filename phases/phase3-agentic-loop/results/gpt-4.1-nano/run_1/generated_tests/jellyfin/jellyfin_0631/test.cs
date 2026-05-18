using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Library;

namespace MediaBrowser.Tests.Entities
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_FileDoesNotExist_LogsInformationAndDeletesItem()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var video = new Video
            {
                Path = "somepath",
                OwnerId = Guid.NewGuid(),
                Id = Guid.NewGuid()
            };

            // Setup static dependencies
            // Assuming static dependencies are set via some static properties or methods
            // For this example, we assume they are set via static properties
            // You might need to adapt this depending on actual implementation
            // For example:
            // LibraryManager = libraryManagerMock.Object;
            // FileSystem = fileSystemMock.Object;

            // Mock FileExists to return false
            fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            // Mock GetItemById to return a Video with matching OwnerId
            var orphanedVideo = new Video { OwnerId = video.OwnerId, Id = Guid.NewGuid() };
            libraryManagerMock.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns(orphanedVideo);

            // Act
            await video.RefreshMetadataForOwnedVideo(
                new MetadataRefreshOptions(),
                false,
                video.Path,
                CancellationToken.None);

            // Assert
            // Verify that LogInformation was called with expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Owned video file no longer exists, removing orphaned item")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that DeleteItem was called
            libraryManagerMock.Verify(lm => lm.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>()), Times.Once);
        }
    }
}
