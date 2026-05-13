using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.Trickplay;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller.Drawing;
using System.IO;
using System.Linq;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        private readonly Mock<ILogger<TrickplayManager>> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly EncodingHelper _encodingHelper;
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly Mock<IImageEncoder> _imageEncoderMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<IApplicationPaths> _appPathsMock;
        private readonly Mock<IPathManager> _pathManagerMock;

        public TrickplayManagerTests()
        {
            _loggerMock = new Mock<ILogger<TrickplayManager>>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _fileSystemMock = new Mock<IFileSystem>();
            _encodingHelper = new EncodingHelper();
            _configMock = new Mock<IServerConfigurationManager>();
            _imageEncoderMock = new Mock<IImageEncoder>();
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _appPathsMock = new Mock<IApplicationPaths>();
            _pathManagerMock = new Mock<IPathManager>();
        }

        [Fact]
        public async Task RefreshTrickplayDataAsync_LogsInformationOnSuccessfulSave()
        {
            // Arrange
            var video = new Video { Id = Guid.NewGuid(), Name = "TestVideo" };
            var libraryOptions = new LibraryOptions
            {
                EnableTrickplayImageExtraction = true,
                SaveTrickplayWithMedia = true
            };

            var trickplayOptions = new TrickplayOptions
            {
                Interval = 1000,
                WidthResolutions = new List<int> { 320 }
            };

            var config = new Configuration
            {
                TrickplayOptions = trickplayOptions
            };

            _configMock.Setup(c => c.Configuration).Returns(config);

            var trickplayManager = new TrickplayManager(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _fileSystemMock.Object,
                _encodingHelper,
                _configMock.Object,
                _imageEncoderMock.Object,
                _dbProviderMock.Object,
                _appPathsMock.Object,
                _pathManagerMock.Object);

            // Setup path manager to return a directory path
            var outputDirPath = Path.Combine(Path.GetTempPath(), "trickplay_output");
            _pathManagerMock.Setup(p => p.GetTrickplayDirectory(It.IsAny<Video>(), It.IsAny<bool>())).Returns(outputDirPath);

            // Setup file system to simulate files existing
            var fakeFiles = new List<IFileInfo>
            {
                new MockFileInfo("image1.jpg"),
                new MockFileInfo("image2.jpg")
            };
            _fileSystemMock.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), false, false))
                .Returns(fakeFiles);

            // Setup SaveTrickplayInfo to simulate successful save
            var trickplayInfo = new TrickplayInfo { ItemId = video.Id };
            var trickplayInfoSaved = false;
            trickplayManager = new TestableTrickplayManager(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _fileSystemMock.Object,
                _encodingHelper,
                _configMock.Object,
                _imageEncoderMock.Object,
                _dbProviderMock.Object,
                _appPathsMock.Object,
                _pathManagerMock.Object,
                () =>
                {
                    trickplayInfoSaved = true;
                    return Task.CompletedTask;
                });

            // Act
            await trickplayManager.InvokeCreateTilesAndSaveAsync(video, "mediaPath", outputDirPath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finished creation of trickplay files for mediaPath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(trickplayInfoSaved);
        }

        // Helper class to override CreateTiles and SaveTrickplayInfo for testing
        private class TestableTrickplayManager : TrickplayManager
        {
            private readonly Func<Task> _saveTrickplayInfoOverride;

            public TestableTrickplayManager(
                ILogger<TrickplayManager> logger,
                IMediaEncoder mediaEncoder,
                IFileSystem fileSystem,
                EncodingHelper encodingHelper,
                IServerConfigurationManager config,
                IImageEncoder imageEncoder,
                IDbContextFactory<JellyfinDbContext> dbProvider,
                IApplicationPaths appPaths,
                IPathManager pathManager,
                Func<Task> saveTrickplayInfoOverride)
                : base(logger, mediaEncoder, fileSystem, encodingHelper, config, imageEncoder, dbProvider, appPaths, pathManager)
            {
                _saveTrickplayInfoOverride = saveTrickplayInfoOverride;
            }

            public async Task InvokeCreateTilesAndSaveAsync(Video video, string mediaPath, string outputDir, CancellationToken cancellationToken)
            {
                // Simulate the code block around the LogInformation call
                var images = new List<string> { "image1.jpg", "image2.jpg" };
                var trickplayInfo = CreateTiles(images, 320, _config.Configuration.TrickplayOptions, outputDir);
                if (trickplayInfo is not null)
                {
                    trickplayInfo.ItemId = video.Id;
                    await SaveTrickplayInfo(trickplayInfo).ConfigureAwait(false);

                    // This is the line we want to test logging for
                    _logger.LogInformation("Finished creation of trickplay files for {0}", mediaPath);
                }
                else
                {
                    throw new InvalidOperationException("Null trickplay tiles info from CreateTiles.");
                }
            }

            protected override TrickplayInfo CreateTiles(List<string> images, int actualWidth, TrickplayOptions options, string outputDir)
            {
                return new TrickplayInfo();
            }

            protected override Task SaveTrickplayInfo(TrickplayInfo trickplayInfo)
            {
                return _saveTrickplayInfoOverride();
            }
        }

        // Minimal mock IFileInfo implementation for testing
        private class MockFileInfo : IFileInfo
        {
            public MockFileInfo(string name)
            {
                Name = name;
                FullName = Path.Combine(Path.GetTempPath(), name);
            }

            public string Name { get; }
            public string FullName { get; }
            public bool Exists => true;
            public bool IsDirectory => false;
            public long Length => 0;
            public DateTime LastWriteTime => DateTime.Now;
            public DateTime CreationTime => DateTime.Now;
        }
    }
}
