using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Trickplay;

namespace Jellyfin.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task RefreshTrickplayDataAsync_LogsInformation_WhenFinishedCreatingTrickplayFiles()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var fileSystemMock = new Mock<IFileSystem>();
            var encodingHelper = new EncodingHelper();
            var configMock = new Mock<IServerConfigurationManager>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var appPathsMock = new Mock<IApplicationPaths>();
            var pathManagerMock = new Mock<IPathManager>();

            // Setup configuration to return trickplay options with interval >= 1000
            var trickplayOptions = new MediaBrowser.Model.Configuration.TrickplayOptions
            {
                Interval = 1500,
                WidthResolutions = new[] { 320, 640 }
            };
            var config = new MediaBrowser.Model.Configuration.ServerConfiguration
            {
                TrickplayOptions = trickplayOptions
            };
            configMock.Setup(c => c.Configuration).Returns(config);

            var manager = new TestTrickplayManager(
                loggerMock.Object,
                mediaEncoderMock.Object,
                fileSystemMock.Object,
                encodingHelper,
                configMock.Object,
                imageEncoderMock.Object,
                dbProviderMock.Object,
                appPathsMock.Object,
                pathManagerMock.Object,
                new Dictionary<string, (int, int)> { { "720p", (1280, 720) } });

            var video = new Video { Id = Guid.NewGuid().ToString(), Name = "Test Video" };
            var libraryOptions = new LibraryOptions
            {
                EnableTrickplayImageExtraction = true,
                SaveTrickplayWithMedia = true
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await manager.RefreshTrickplayDataAsync(video, false, libraryOptions, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finished creation of trickplay files for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        private class TestTrickplayManager : TrickplayManager
        {
            private readonly Dictionary<string, (int, int)> _resolutions;

            public TestTrickplayManager(
                ILogger<TrickplayManager> logger,
                IMediaEncoder mediaEncoder,
                IFileSystem fileSystem,
                EncodingHelper encodingHelper,
                IServerConfigurationManager config,
                IImageEncoder imageEncoder,
                IDbContextFactory<JellyfinDbContext> dbProvider,
                IApplicationPaths appPaths,
                IPathManager pathManager,
                Dictionary<string, (int, int)> resolutions)
                : base(logger, mediaEncoder, fileSystem, encodingHelper, config, imageEncoder, dbProvider, appPaths, pathManager)
            {
                _resolutions = resolutions;
            }

            protected override async Task<Dictionary<string, (int TileWidth, int TileHeight)>> GetTrickplayResolutions(string itemId)
            {
                return await Task.FromResult(_resolutions);
            }
        }
    }
}
