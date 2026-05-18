using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.Trickplay;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task MoveGeneratedTrickplayDataAsync_LogsInformationOnMove()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var fileSystemMock = new Mock<IFileSystem>();
            var encodingHelperMock = new Mock<EncodingHelper>();
            var configMock = new Mock<IServerConfigurationManager>();
            var imageEncoderMock = new Mock<MediaBrowser.Controller.Drawing.IImageEncoder>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var appPathsMock = new Mock<MediaBrowser.Common.Configuration.IApplicationPaths>();
            var pathManagerMock = new Mock<IPathManager>();

            var trickplayOptions = new TrickplayOptions
            {
                Interval = 1000,
                WidthResolutions = new List<int> { 320 },
                EnableHwAcceleration = false,
                EnableHwEncoding = false,
                ProcessThreads = 1,
                Qscale = 1,
                ProcessPriority = 0,
                EnableKeyFrameOnlyExtraction = false
            };

            var configurationMock = new Mock<IApplicationConfiguration>();
            configurationMock.SetupGet(c => c.TrickplayOptions).Returns(trickplayOptions);
            configMock.SetupGet(c => c.Configuration).Returns(configurationMock.Object);

            var video = new Video { Id = Guid.NewGuid(), Name = "TestVideo" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };

            var trickplayManager = new TestTrickplayManager(
                loggerMock.Object,
                mediaEncoderMock.Object,
                fileSystemMock.Object,
                encodingHelperMock.Object,
                configMock.Object,
                imageEncoderMock.Object,
                dbContextFactoryMock.Object,
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
        }

        private class TestTrickplayManager : TrickplayManager
        {
            public TestTrickplayManager(
                ILogger<TrickplayManager> logger,
                MediaBrowser.Controller.MediaEncoding.IMediaEncoder mediaEncoder,
                IFileSystem fileSystem,
                EncodingHelper encodingHelper,
                IServerConfigurationManager config,
                MediaBrowser.Controller.Drawing.IImageEncoder imageEncoder,
                IDbContextFactory<JellyfinDbContext> dbProvider,
                MediaBrowser.Common.Configuration.IApplicationPaths appPaths,
                IPathManager pathManager)
                : base(logger, mediaEncoder, fileSystem, encodingHelper, config, imageEncoder, dbProvider, appPaths, pathManager)
            {
            }

            // We cannot override private methods, so we simulate the environment by mocking dependencies and file system

            protected override Task<Dictionary<string, TrickplayResolution>> GetTrickplayResolutions(Guid videoId)
            {
                return Task.FromResult(new Dictionary<string, TrickplayResolution>
                {
                    { "320x180", new TrickplayResolution { TileWidth = 320, TileHeight = 180 } }
                });
            }

            protected override string GetTrickplayDirectory(Video video, int tileWidth, int tileHeight, string resolution, bool saveWithMedia)
            {
                // Return a temp directory path for testing
                return Path.GetTempPath();
            }
        }

        // Dummy class to satisfy references
        private class TrickplayResolution
        {
            public int TileWidth { get; set; }
            public int TileHeight { get; set; }
        }
    }
}
