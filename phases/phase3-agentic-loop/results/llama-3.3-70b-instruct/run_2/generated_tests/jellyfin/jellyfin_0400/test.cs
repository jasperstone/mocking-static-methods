using Xunit;
using Moq;
using System.Threading;
using System.IO;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.Trickplay;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.EntityFrameworkCore;
using Jellyfin.Database;

namespace Jellyfin.Server.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task LogInformation_Called_When_Trickplay_Creation_Completes()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var fileSystemMock = new Mock<IFileSystem>();
            var encodingHelperMock = new Mock<EncodingHelper>();
            var configMock = new Mock<IServerConfigurationManager>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var dbProviderMock = new Mock<IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>();
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

            var video = new Video { Id = Guid.NewGuid() };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true };
            var cancellationToken = new CancellationToken();

            // Act
            await trickplayManager.RefreshTrickplayDataAsync(video, false, libraryOptions, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Finished creation of trickplay files for {0}", It.IsAny<string>()), Times.Once);
        }
    }
}
