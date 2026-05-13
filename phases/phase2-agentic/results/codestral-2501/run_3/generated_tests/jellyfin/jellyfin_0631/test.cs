using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_FileDoesNotExist_LogsInformationAndDeletesItem()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Video>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockFileSystem = new Mock<IFileSystem>();
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

            mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);
            mockLibraryManager.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns(orphanedVideo);
            mockLibraryManager.Setup(lm => lm.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>()));

            // Act
            await video.RefreshMetadataForOwnedVideo(
                new MetadataRefreshOptions(),
                false,
                "testPath",
                typeof(Video),
                CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Owned video file no longer exists, removing orphaned item: {Path}",
                    "testPath"),
                Times.Once);
            mockLibraryManager.Verify(lm => lm.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>()), Times.Once);
        }
    }
}
