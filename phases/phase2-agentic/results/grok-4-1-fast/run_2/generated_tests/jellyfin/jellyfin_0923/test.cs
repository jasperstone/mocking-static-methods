#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<LyricResolver> _lyricResolverMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly ProbeProvider _probeProvider;

        public ProbeProviderTests()
        {
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _lyricResolverMock = new Mock<LyricResolver>();
            _directoryServiceMock = new Mock<IDirectoryService>();

            // Simplified constructor - only mocking dependencies we need for the test
            _probeProvider = new ProbeProviderTestDouble(
                _loggerMock.Object,
                _lyricResolverMock.Object,
                _directoryServiceMock.Object);
        }

        [Fact]
        public void HasChanged_AudioItem_LyricFilesMismatch_LogsDebugMessage()
        {
            // Arrange
            var audioItem = new Audio
            {
                Path = "/music/song.mp3",
                SupportsLocalMetadata = true,
                LyricFiles = new[] { "/music/song.lrc" }
            };

            var externalFiles = new[] { new LyricFileInfo { Path = "/music/song_new.lrc" } };
            _lyricResolverMock
                .Setup(r => r.GetExternalFiles(audioItem, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            // Act
            var result = _probeProvider.HasChanged(audioItem, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    "Refreshing {ItemPath} due to external lyrics change.",
                    audioItem.Path),
                Times.Once);
        }

        [Fact]
        public void HasChanged_AudioItem_LyricFilesMatch_DoesNotLogDebugMessage()
        {
            // Arrange
            var audioItem = new Audio
            {
                Path = "/music/song.mp3",
                SupportsLocalMetadata = true,
                LyricFiles = new[] { "/music/song.lrc" }
            };

            var externalFiles = new[] { new LyricFileInfo { Path = "/music/song.lrc" } };
            _lyricResolverMock
                .Setup(r => r.GetExternalFiles(audioItem, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            // Act
            var result = _probeProvider.HasChanged(audioItem, _directoryServiceMock.Object);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        // Test double for simplified constructor
        private class ProbeProviderTestDouble : ProbeProvider
        {
            public ProbeProviderTestDouble(
                ILogger<ProbeProvider> logger,
                LyricResolver lyricResolver,
                IDirectoryService directoryService)
                : base(
                    Mock.Of<IMediaSourceManager>(),
                    Mock.Of<IMediaEncoder>(),
                    Mock.Of<IBlurayExaminer>(),
                    Mock.Of<ILocalizationManager>(),
                    Mock.Of<IChapterManager>(),
                    Mock.Of<IServerConfigurationManager>(),
                    Mock.Of<ISubtitleManager>(),
                    Mock.Of<ILibraryManager>(),
                    Mock.Of<IFileSystem>(),
                    Mock.Of<ILoggerFactory>(),
                    new NamingOptions(),
                    Mock.Of<ILyricManager>(),
                    Mock.Of<IMediaAttachmentRepository>(),
                    Mock.Of<IMediaStreamRepository>())
            {
                // Use injected dependencies via reflection or protected access
                typeof(ProbeProvider).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, logger);
                typeof(ProbeProvider).GetField("_lyricResolver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, lyricResolver);
            }
        }
    }

    // Mock types for dependencies
    public class LyricFileInfo
    {
        public string Path { get; set; } = string.Empty;
    }

    public class LyricResolver
    {
        public virtual List<LyricFileInfo> GetExternalFiles(Audio audio, IDirectoryService directoryService, bool allowMisnamed) => [];
    }
}
