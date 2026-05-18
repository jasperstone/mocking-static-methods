using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Common.Configuration;
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
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        private readonly Mock<ILogger<TrickplayManager>> _mockLogger;
        private readonly Mock<IMediaEncoder> _mockMediaEncoder;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<EncodingHelper> _mockEncodingHelper;
        private readonly Mock<IServerConfigurationManager> _mockConfig;
        private readonly Mock<IImageEncoder> _mockImageEncoder;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockDbProvider;
        private readonly Mock<IApplicationPaths> _mockAppPaths;
        private readonly Mock<IPathManager> _mockPathManager;
        private readonly TrickplayManager _trickplayManager;

        public TrickplayManagerTests()
        {
            _mockLogger = new Mock<ILogger<TrickplayManager>>();
            _mockMediaEncoder = new Mock<IMediaEncoder>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockEncodingHelper = new Mock<EncodingHelper>();
            _mockConfig = new Mock<IServerConfigurationManager>();
            _mockImageEncoder = new Mock<IImageEncoder>();
            _mockDbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _mockAppPaths = new Mock<IApplicationPaths>();
            _mockPathManager = new Mock<IPathManager>();

            _trickplayManager = new TrickplayManager(
                _mockLogger.Object,
                _mockMediaEncoder.Object,
                _mockFileSystem.Object,
                _mockEncodingHelper.Object,
                _mockConfig.Object,
                _mockImageEncoder.Object,
                _mockDbProvider.Object,
                _mockAppPaths.Object,
                _mockPathManager.Object);
        }

        [Fact]
        public async Task RefreshTrickplayDataAsync_LogsInformation_WhenTrickplayFilesAreCreated()
        {
            // Arrange
            var video = new Video();
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true };
            var trickplayOptions = new TrickplayOptions { Interval = 1000, WidthResolutions = new int[] { 1920 } };
            var trickplayInfo = new TrickplayInfo();
            var mediaPath = "path/to/media";
            var imgTempDir = "path/to/temp";
            var outputDir = new DirectoryInfo("path/to/output");

            _mockConfig.Setup(c => c.Configuration.TrickplayOptions).Returns(trickplayOptions);
            _mockFileSystem.Setup(fs => fs.GetFiles(imgTempDir, It.IsAny<string[]>(), false, false))
                .Returns(new List<FileSystemMetadata> { new FileSystemMetadata { FullName = "image1.jpg" } });
            _mockPathManager.Setup(pm => pm.GetTrickplayDirectory(video, It.IsAny<bool>())).Returns(outputDir.FullName);

            // Act
            await _trickplayManager.RefreshTrickplayDataAsync(video, false, libraryOptions, CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogInformation("Finished creation of trickplay files for {0}", mediaPath),
                Times.Once);
        }
    }
}
