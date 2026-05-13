using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.Trickplay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Trickplay
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task RefreshTrickplayDataAsync_LogsInformationOnSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var fileSystemMock = new Mock<IFileSystem>();
            var encodingHelperMock = new Mock<EncodingHelper>();
            var configMock = new Mock<IServerConfigurationManager>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var appPathsMock = new Mock<IApplicationPaths>();
            var pathManagerMock = new Mock<IPathManager>();

            var trickplayManager = new TrickplayManager(
                loggerMock.Object,
                mediaEncoderMock.Object,
                fileSystemMock.Object,
                encodingHelperMock.Object,
                configMock.Object,
                imageEncoderMock.Object,
                dbProviderMock.Object,
                appPathsMock.Object,
                pathManagerMock.Object);

            var video = new Video { Id = Guid.NewGuid(), Name = "Test Video" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };
            var cancellationToken = CancellationToken.None;

            // Setup mocks
            fileSystemMock.Setup(fs => fs.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), false, false))
                .Returns(new[] { new FileInfo("image1.jpg"), new FileInfo("image2.jpg") });

            var dbContextMock = new Mock<JellyfinDbContext>();
            dbProviderMock.Setup(db => db.CreateDbContextAsync(cancellationToken))
                .ReturnsAsync(dbContextMock.Object);

            // Act
            await trickplayManager.RefreshTrickplayDataAsync(video, false, libraryOptions, cancellationToken);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("Finished creation of trickplay files for {0}", It.Is<string>(s => s == "Test Video")),
                Times.Once);
        }
    }
}
