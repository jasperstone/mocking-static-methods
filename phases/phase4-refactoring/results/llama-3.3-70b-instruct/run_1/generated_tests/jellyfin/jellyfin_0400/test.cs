using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.Trickplay;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;

namespace Jellyfin.Server.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task LogInformation_Called_When_Trickplay_Files_Created()
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
                new Video { Id = "123", Name = "Test Video" },
                true,
                new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true },
                CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
