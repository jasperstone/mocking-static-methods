using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.Trickplay;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using Jellyfin.Database.Implementations;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task MoveGeneratedTrickplayDataAsync_LogsInformationWhenMovingImages()
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

            var trickplayOptions = new TrickplayOptions
            {
                Interval = 1000,
                WidthResolutions = new int[] { 320 },
                EnableTrickplayImageExtraction = true
            };

            var configuration = new MediaBrowser.Model.Configuration.Configuration
            {
                TrickplayOptions = trickplayOptions
            };

            configMock.Setup(c => c.Configuration).Returns(configuration);

            var video = new Video
            {
                Id = Guid.NewGuid(),
                Name = "Test Video"
            };

            var libraryOptions = new LibraryOptions
            {
                EnableTrickplayImageExtraction = true,
                SaveTrickplayWithMedia = true
            };

            // We cannot override private methods, so we test the logging indirectly by simulating conditions
            // that cause MoveContent to be called and thus LogInformation to be invoked.

            // Setup file system mock to simulate directory contents
            var localDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "local");
            var mediaDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "media");

            Directory.CreateDirectory(localDir);
            Directory.CreateDirectory(mediaDir);

            // Create dummy files in localDir to simulate files to move
            File.WriteAllText(Path.Combine(localDir, "image1.jpg"), "dummy");
            File.WriteAllText(Path.Combine(localDir, "image2.jpg"), "dummy");

            // Setup pathManager to return these directories
            pathManagerMock.Setup(p => p.GetTrickplayDirectory(It.IsAny<Video>(), It.IsAny<bool>()))
                .Returns<Video, bool>((v, media) => media ? mediaDir : localDir);

            // Setup TrickplayManager instance
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
            await trickplayManager.MoveGeneratedTrickplayDataAsync(video, libraryOptions, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Moved trickplay images for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Cleanup
            try
            {
                if (Directory.Exists(localDir))
                    Directory.Delete(localDir, true);
                if (Directory.Exists(mediaDir))
                    Directory.Delete(mediaDir, true);
            }
            catch { }
        }
    }
}
