using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using Jellyfin.Server.Implementations.Trickplay;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Drawing;
using Jellyfin.Database.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task RefreshTrickplayDataAsync_LogsInformation_WhenTrickplayFilesCreated()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TrickplayManager>>();
            var mockMediaEncoder = new Mock<IMediaEncoder>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockEncodingHelper = new Mock<EncodingHelper>();
            var mockConfig = new Mock<IServerConfigurationManager>();
            var mockImageEncoder = new Mock<IImageEncoder>();
            var mockDbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            var mockPathManager = new Mock<IPathManager>();

            var trickplayManager = new TrickplayManager(
                mockLogger.Object,
                mockMediaEncoder.Object,
                mockFileSystem.Object,
                mockEncodingHelper.Object,
                mockConfig.Object,
                mockImageEncoder.Object,
                mockDbProvider.Object,
                mockAppPaths.Object,
                mockPathManager.Object);

            var video = new Video();
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true };
            var cancellationToken = CancellationToken.None;

            var trickplayOptions = new TrickplayOptions { Interval = 1000, WidthResolutions = new int[] { 1920 } };
            mockConfig.Setup(c => c.Configuration.TrickplayOptions).Returns(trickplayOptions);

            var trickplayDirectory = "path/to/trickplay";
            mockPathManager.Setup(p => p.GetTrickplayDirectory(video, It.IsAny<bool>())).Returns(trickplayDirectory);

            var imgTempDir = "path/to/temp";
            var outputDir = new DirectoryInfo("path/to/output");
            var mediaPath = "path/to/media";

            mockFileSystem.Setup(fs => fs.GetFiles(imgTempDir, It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<FileSystemMetadata> { new FileInfo("path/to/image.jpg") }.Select(f => new FileSystemMetadata { FullName = f.FullName }));

            // Act
            await trickplayManager.RefreshTrickplayDataAsync(video, false, libraryOptions, cancellationToken);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finished creation of trickplay files for")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
