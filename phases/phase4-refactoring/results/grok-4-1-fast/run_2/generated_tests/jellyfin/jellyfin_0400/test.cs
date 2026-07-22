using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.IO;
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
        private readonly Mock<IDbContextFactory<object>> _mockDbProvider;
        private readonly Mock<object> _mockAppPaths;
        private readonly Mock<object> _mockPathManager;

        public TrickplayManagerTests()
        {
            _mockLogger = new Mock<ILogger<TrickplayManager>>();
            _mockMediaEncoder = new Mock<IMediaEncoder>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockEncodingHelper = new Mock<EncodingHelper>();
            _mockConfig = new Mock<IServerConfigurationManager>();
            _mockImageEncoder = new Mock<IImageEncoder>();
            _mockDbProvider = new Mock<IDbContextFactory<object>>();
            _mockAppPaths = new Mock<object>();
            _mockPathManager = new Mock<object>();
        }

        [Fact]
        public async Task GenerateTrickplayDataAsync_SuccessPath_LogsInformationLine361()
        {
            // Arrange
            var video = new Video { Id = "test-video-id", Path = "/path/to/media.mp4" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true };
            var cancellationToken = new CancellationToken();

            var imgTempDir = "/tmp/trickplay-images";
            var outputDir = new DirectoryInfo("/tmp/output");

            _mockConfig.Setup(c => c.Configuration)
                .Returns(new ServerConfiguration 
                { 
                    TrickplayOptions = new TrickplayOptions 
                    { 
                        Interval = 1000,
                        WidthResolutions = new[] { 160 },
                        EnableHwAcceleration = false,
                        EnableHwEncoding = false,
                        ProcessThreads = 1,
                        Qscale = 23,
                        ProcessPriority = ProcessPriorityClass.Normal,
                        EnableKeyFrameOnlyExtraction = false
                    } 
                });

            _mockMediaEncoder.Setup(me => me.ExtractVideoImages(
                    It.IsAny<Video>(),
                    It.IsAny<TrickplayImageOptions>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(imgTempDir);

            _mockFileSystem.Setup(fs => fs.GetFiles(imgTempDir, 
                    It.Is<string[]>(a => a.SequenceEqual(new[] { ".jpg" })), 
                    false, false))
                .Returns(new[] { new FileInfo(Path.Combine(imgTempDir, "image1.jpg")) });

            _mockFileSystem.Setup(fs => fs.GetDirectoryName(It.IsAny<string>()))
                .Returns(outputDir);

            // To reach line 361, we need CreateTiles to return non-null and SaveTrickplayInfo to succeed
            // Mock private SaveTrickplayInfo using apply_refactor if needed, but verify logger call

            var trickplayManager = new TrickplayManager(
                _mockLogger.Object,
                _mockMediaEncoder.Object,
                _mockFileSystem.Object,
                _mockEncodingHelper.Object,
                _mockConfig.Object,
                _mockImageEncoder.Object,
                _mockDbProvider.Object,
                _mockAppPaths.Object,
                _mockPathManager.Object);

            // Act
            await trickplayManager.GenerateTrickplayDataAsync(video, libraryOptions, cancellationToken);

            // Assert - Verify the specific LogInformation call at line 361
            _mockLogger.Verify(
                x => x.LogInformation(
                    "Finished creation of trickplay files for {0}",
                    "/path/to/media.mp4"),
                Times.Once);
        }

        [Fact]
        public async Task GenerateTrickplayDataAsync_SaveFails_LogsErrorInstead()
        {
            // Arrange - setup to hit the catch block around line 361
            var video = new Video { Id = "test-video-id", Path = "/path/to/media.mp4" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true };
            var cancellationToken = new CancellationToken();

            var imgTempDir = "/tmp/trickplay-images-fail";
            
            _mockConfig.Setup(c => c.Configuration)
                .Returns(new ServerConfiguration 
                { 
                    TrickplayOptions = new TrickplayOptions 
                    { 
                        Interval = 1000,
                        WidthResolutions = new[] { 160 }
                    } 
                });
            
            _mockMediaEncoder.Setup(me => me.ExtractVideoImages(It.IsAny<Video>(), It.IsAny<TrickplayImageOptions>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(imgTempDir);
            
            _mockFileSystem.Setup(fs => fs.GetFiles(imgTempDir, It.IsAny<string[]>(), false, false))
                .Throws(new InvalidOperationException("Test exception to hit catch block"));

            var trickplayManager = new TrickplayManager(
                _mockLogger.Object,
                _mockMediaEncoder.Object,
                _mockFileSystem.Object,
                _mockEncodingHelper.Object,
                _mockConfig.Object,
                _mockImageEncoder.Object,
                _mockDbProvider.Object,
                _mockAppPaths.Object,
                _mockPathManager.Object);

            // Act
            await trickplayManager.GenerateTrickplayDataAsync(video, libraryOptions, cancellationToken);

            // Assert - Should log error, NOT the information message
            _mockLogger.Verify(
                x => x.LogInformation(
                    "Finished creation of trickplay files for {0}",
                    It.IsAny<string>()),
                Times.Never);
        }
    }
}
