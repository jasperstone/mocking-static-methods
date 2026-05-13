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
        public async Task RefreshTrickplayDataAsync_LogsInformation_WhenTrickplayInfoIsSaved()
        {
            // Arrange
            var video = new Video();
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true };
            var trickplayOptions = new TrickplayOptions { Interval = 1000 };
            var trickplayInfo = new TrickplayInfo();
            var cancellationToken = CancellationToken.None;

            _mockConfig.Setup(c => c.Configuration.TrickplayOptions).Returns(trickplayOptions);
            _mockDbProvider.Setup(d => d.CreateDbContextAsync(cancellationToken)).ReturnsAsync(new Mock<JellyfinDbContext>().Object);
            _mockPathManager.Setup(p => p.GetTrickplayDirectory(video, It.IsAny<bool>())).Returns("tempDir");
            _mockFileSystem.Setup(f => f.GetFiles("tempDir", It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<FileSystemMetadata> { new FileInfo("tempDir/image.jpg") }.Cast<FileSystemMetadata>().ToList());

            // Act
            await _trickplayManager.RefreshTrickplayDataAsync(video, true, libraryOptions, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finished creation of trickplay files for")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
