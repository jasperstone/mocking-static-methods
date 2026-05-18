using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Configuration;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task LogInformation_Called_When_Trickplay_Files_Created()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            var encodingHelperMock = new Mock<MediaBrowser.Controller.Drawing.EncodingHelper>();
            var configMock = new Mock<MediaBrowser.Common.Configuration.IServerConfigurationManager>();
            var imageEncoderMock = new Mock<MediaBrowser.Controller.Drawing.IImageEncoder>();
            var dbProviderMock = new Mock<Jellyfin.Database.Implementations.IDbContextFactory<Jellyfin.Database.Implementations.Entities.JellyfinDbContext>>();
            var appPathsMock = new Mock<MediaBrowser.Common.Configuration.IApplicationPaths>();
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

            // Act
            await trickplayManager.RefreshTrickplayDataAsync(new Video(), true, new LibraryOptions(), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
