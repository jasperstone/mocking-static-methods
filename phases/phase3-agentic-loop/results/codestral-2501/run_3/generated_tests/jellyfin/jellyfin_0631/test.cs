using Xunit;
using Moq;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;
using System;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_FileDoesNotExist_LogsInformationAndDeletesItem()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger<Video>>();

            var video = new Video
            {
                Id = Guid.NewGuid(),
                Path = "testPath"
            };

            var orphanedVideo = new Video
            {
                Id = Guid.NewGuid(),
                OwnerId = video.Id
            };

            mockLibraryManager.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns(orphanedVideo);
            mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            var cancellationToken = new CancellationToken();

            // Act
            await video.RefreshMetadataForOwnedVideo(
                new MetadataRefreshOptions(),
                false,
                "testPath",
                typeof(Video),
                cancellationToken);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Owned video file no longer exists, removing orphaned item: {Path}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            mockLibraryManager.Verify(lm => lm.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>()), Times.Once);
        }
    }
}
