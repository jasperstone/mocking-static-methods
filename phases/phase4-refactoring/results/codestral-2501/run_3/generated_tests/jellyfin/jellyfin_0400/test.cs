using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;
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
        public async Task MoveGeneratedTrickplayDataAsync_ShouldLogInformation_WhenMovingTrickplayImages()
        {
            // Arrange
            var video = new Video { Id = Guid.NewGuid(), Name = "Test Video" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };
            var cancellationToken = new CancellationToken();
            var existingTrickplayResolutions = new Dictionary<string, TrickplayResolution>
            {
                { "720p", new TrickplayResolution { TileWidth = 1280, TileHeight = 720 } }
            };

            _mockConfig.Setup(c => c.Configuration.TrickplayOptions).Returns(new TrickplayOptions { Interval = 1000 });
            _mockDbProvider.Setup(d => d.CreateDbContextAsync(cancellationToken)).ReturnsAsync(new JellyfinDbContext(new DbContextOptions<JellyfinDbContext>()));
            _mockPathManager.Setup(p => p.GetTrickplayDirectory(video, true)).Returns("TestDirectory");
            _mockFileSystem.Setup(f => f.MoveDirectory(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

            // Act
            await _trickplayManager.MoveGeneratedTrickplayDataAsync(video, libraryOptions, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Moved trickplay images for Test Video to TestDirectory")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
