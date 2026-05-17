using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.Trickplay;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;

namespace Jellyfin.Server.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task RefreshTrickplayDataAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            var encodingHelperMock = new Mock<MediaBrowser.Controller.Drawing.EncodingHelper>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var imageEncoderMock = new Mock<MediaBrowser.Controller.Drawing.IImageEncoder>();
            var dbProviderMock = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>();
            var appPathsMock = new Mock<MediaBrowser.Controller.IO.IApplicationPaths>();
            var pathManagerMock = new Mock<MediaBrowser.Controller.IO.IPathManager>();

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

            var video = new MediaBrowser.Controller.Entities.Video { Id = Guid.NewGuid(), Name = "videoName" };
            var libraryOptions = new MediaBrowser.Model.Configuration.LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };
            var cancellationToken = new CancellationToken();

            // Act
            await trickplayManager.RefreshTrickplayDataAsync(video, true, libraryOptions, cancellationToken);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
