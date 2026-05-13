using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
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
            var loggerMock = new Mock<ILogger<Video>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();

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

            fileSystemMock.Setup(fs => fs.FileExists("testPath")).Returns(false);
            libraryManagerMock.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns(orphanedVideo);

            var videoInstance = new Video
            {
                Logger = loggerMock.Object,
                LibraryManager = libraryManagerMock.Object,
                FileSystem = fileSystemMock.Object
            };

            // Act
            await videoInstance.RefreshMetadataForOwnedVideo(
                new MetadataRefreshOptions(),
                false,
                "testPath",
                CancellationToken.None);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    "Owned video file no longer exists, removing orphaned item: {Path}",
                    "testPath"),
                Times.Once);

            libraryManagerMock.Verify(
                lm => lm.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>()),
                Times.Once);
        }
    }
}
