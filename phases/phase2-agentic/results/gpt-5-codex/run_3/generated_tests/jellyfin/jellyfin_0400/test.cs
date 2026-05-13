using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.Trickplay;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Trickplay
{
    public class TrickplayManagerTests
    {
        private class TestTrickplayManager : TrickplayManager
        {
            public Func<Task<string?>>? GetOrCreateTempDirectoryFunc { get; set; }
            public Func<IEnumerable<string>, int, TrickplayOptions, string, object?>? CreateTilesFunc { get; set; }
            public Func<object, Task>? SaveTrickplayInfoFunc { get; set; }

            public TestTrickplayManager(
                ILogger<TrickplayManager> logger,
                IMediaEncoder mediaEncoder,
                IFileSystem fileSystem,
                EncodingHelper encodingHelper,
                IServerConfigurationManager config,
                IImageEncoder imageEncoder,
                IDbContextFactory<JellyfinDbContext> dbProvider,
                IApplicationPaths appPaths,
                IPathManager pathManager)
                : base(logger, mediaEncoder, fileSystem, encodingHelper, config, imageEncoder, dbProvider, appPaths, pathManager)
            {
            }

            protected override Task<string?> GetOrCreateTempDirectory(Video video, TrickplayOptions options, CancellationToken cancellationToken)
            {
                if (GetOrCreateTempDirectoryFunc is null)
                {
                    return Task.FromResult<string?>(null);
                }

                return GetOrCreateTempDirectoryFunc();
            }

            protected override object? CreateTiles(IEnumerable<string> paths, int width, TrickplayOptions options, string outputDirectory)
            {
                return CreateTilesFunc is null
                    ? base.CreateTiles(paths, width, options, outputDirectory)
                    : CreateTilesFunc(paths, width, options, outputDirectory);
            }

            protected override Task SaveTrickplayInfo(object trickplayInfo)
            {
                if (SaveTrickplayInfoFunc is null)
                {
                    return base.SaveTrickplayInfo(trickplayInfo);
                }

                return SaveTrickplayInfoFunc(trickplayInfo);
            }

            public Task InvokeTryCreateTrickplayAsync(Video video, TrickplayOptions options, string mediaPath, CancellationToken cancellationToken)
            {
                return TryCreateTrickplayAsync(video, options, mediaPath, cancellationToken);
            }
        }

        [Fact]
        public async Task TryCreateTrickplayAsync_LogsInformationOnSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();

            var configMock = new Mock<IServerConfigurationManager>();
            configMock.SetupGet(c => c.Configuration).Returns(new ServerConfiguration
            {
                TrickplayOptions = new TrickplayOptions
                {
                    Interval = 2_000,
                    WidthResolutions = new List<int> { 200 },
                    TileHeight = 200,
                    TileWidth = 200,
                    ExtractKeyFramesOnly = false,
                    Qscale = 3
                }
            });

            var dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<JellyfinDbContext>());

            var fileSystemMock = new Mock<IFileSystem>();
            var pathManagerMock = new Mock<IPathManager>();
            pathManagerMock.Setup(p => p.GetTrickplayDirectory(It.IsAny<BaseItem>(), It.IsAny<bool>()))
                .Returns(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            var mediaEncoderMock = new Mock<IMediaEncoder>();
            mediaEncoderMock.Setup(e => e.CreateTrickplayFiles(It.IsAny<string>(), It.IsAny<TrickplayOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), 200));

            var imageEncoderMock = new Mock<IImageEncoder>();
            var encodingHelper = Mock.Of<EncodingHelper>();
            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.Setup(p => p.GetTrickplayDirectory(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            fileSystemMock.Setup(fs => fs.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<FileSystemMetadata>
                {
                    new FileSystemMetadata { FullName = "image1.jpg" },
                    new FileSystemMetadata { FullName = "image2.jpg" }
                });

            var video = new Video { Id = Guid.NewGuid(), Path = "mediaPath" };
            var trickplayOptions = new TrickplayOptions
            {
                Interval = 2_000,
                WidthResolutions = new List<int> { 200 },
                TileHeight = 200,
                TileWidth = 200,
                ExtractKeyFramesOnly = false,
                Qscale = 3
            };

            var manager = new TestTrickplayManager(
                loggerMock.Object,
                mediaEncoderMock.Object,
                fileSystemMock.Object,
                encodingHelper,
                configMock.Object,
                imageEncoderMock.Object,
                dbFactoryMock.Object,
                appPathsMock.Object,
                pathManagerMock.Object)
            {
                GetOrCreateTempDirectoryFunc = () => Task.FromResult<string?>(Path.GetTempPath()),
                CreateTilesFunc = (_, _, _, _) => new { ItemId = Guid.NewGuid() },
                SaveTrickplayInfoFunc = _ => Task.CompletedTask
            };

            // Act
            await manager.InvokeTryCreateTrickplayAsync(video, trickplayOptions, video.Path, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Finished creation of trickplay files for {0}", video.Path),
                Times.Once);
        }

        [Fact]
        public async Task TryCreateTrickplayAsync_DoesNotLogInformationWhenCreateTilesReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();

            var configMock = new Mock<IServerConfigurationManager>();
            configMock.SetupGet(c => c.Configuration).Returns(new ServerConfiguration
            {
                TrickplayOptions = new TrickplayOptions
                {
                    Interval = 2_000,
                    WidthResolutions = new List<int> { 200 },
                    TileHeight = 200,
                    TileWidth = 200,
                    ExtractKeyFramesOnly = false,
                    Qscale = 3
                }
            });

            var dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<JellyfinDbContext>());

            var fileSystemMock = new Mock<IFileSystem>();
            var pathManagerMock = new Mock<IPathManager>();
            pathManagerMock.Setup(p => p.GetTrickplayDirectory(It.IsAny<BaseItem>(), It.IsAny<bool>()))
                .Returns(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            var mediaEncoderMock = new Mock<IMediaEncoder>();
            mediaEncoderMock.Setup(e => e.CreateTrickplayFiles(It.IsAny<string>(), It.IsAny<TrickplayOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), 200));

            var imageEncoderMock = new Mock<IImageEncoder>();
            var encodingHelper = Mock.Of<EncodingHelper>();
            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.Setup(p => p.GetTrickplayDirectory(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            fileSystemMock.Setup(fs => fs.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<FileSystemMetadata>
                {
                    new FileSystemMetadata { FullName = "image1.jpg" },
                    new FileSystemMetadata { FullName = "image2.jpg" }
                });

            var video = new Video { Id = Guid.NewGuid(), Path = "mediaPath" };
            var trickplayOptions = new TrickplayOptions
            {
                Interval = 2_000,
                WidthResolutions = new List<int> { 200 },
                TileHeight = 200,
                TileWidth = 200,
                ExtractKeyFramesOnly = false,
                Qscale = 3
            };

            var manager = new TestTrickplayManager(
                loggerMock.Object,
                mediaEncoderMock.Object,
                fileSystemMock.Object,
                encodingHelper,
                configMock.Object,
                imageEncoderMock.Object,
                dbFactoryMock.Object,
                appPathsMock.Object,
                pathManagerMock.Object)
            {
                GetOrCreateTempDirectoryFunc = () => Task.FromResult<string?>(Path.GetTempPath()),
                CreateTilesFunc = (_, _, _, _) => null,
                SaveTrickplayInfoFunc = _ => Task.CompletedTask
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                manager.InvokeTryCreateTrickplayAsync(video, trickplayOptions, video.Path, CancellationToken.None));

            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Never);
        }
    }
}
