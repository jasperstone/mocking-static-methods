using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Configuration;
using Microsoft.EntityFrameworkCore;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;

namespace Jellyfin.Server.Implementations.Trickplay
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task RefreshTrickplayDataAsync_LogsInformation_WhenTrickplayFilesAreCreated()
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

            // Act
            await trickplayManager.RefreshTrickplayDataAsync(
                new Video { Id = "videoId", Name = "videoName" },
                true,
                new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true },
                CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
