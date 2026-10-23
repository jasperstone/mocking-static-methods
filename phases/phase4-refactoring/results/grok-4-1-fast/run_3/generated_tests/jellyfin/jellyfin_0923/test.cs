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
using MediaBrowser.Providers.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Tests.MediaInfo
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly ProbeProvider _probeProvider;

        public ProbeProviderTests()
        {
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _directoryServiceMock = new Mock<IDirectoryService>();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ProbeProvider>()).Returns(_loggerMock.Object);

            _probeProvider = new TestableProbeProvider(loggerFactoryMock.Object);
        }

        [Fact]
        public void HasChanged_AudioWithLyricMismatch_LogsDebugAndReturnsTrue()
        {
            // Arrange
            var audio = new Audio
            {
                Path = "/music/song.mp3",
                SupportsLocalMetadata = true,
                LyricFiles = Array.Empty<string>()
            };

            var testProvider = (TestableProbeProvider)_probeProvider;
            testProvider.MockLyricResolver.Setup(r => r.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(new[] { new ExternalFileInfo { Path = "/music/song.lrc" } });

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                x => x.LogDebug("Refreshing {ItemPath} due to external lyrics change.", "/music/song.mp3"),
                Times.Once);
        }

        [Fact]
        public void HasChanged_AudioWithMatchingLyrics_DoesNotLogAndReturnsFalse()
        {
            // Arrange
            var audio = new Audio
            {
                Path = "/music/song.mp3",
                SupportsLocalMetadata = true,
                LyricFiles = new[] { "/music/song.lrc" }
            };

            var testProvider = (TestableProbeProvider)_probeProvider;
            testProvider.MockLyricResolver.Setup(r => r.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(new[] { new ExternalFileInfo { Path = "/music/song.lrc" } });

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogDebug("Refreshing {ItemPath} due to external lyrics change.", It.IsAny<string>()),
                Times.Never);
        }

        private class TestableProbeProvider : ProbeProvider
        {
            public Mock<LyricResolver> MockLyricResolver { get; }

            public TestableProbeProvider(ILoggerFactory loggerFactory)
                : base(
                    Mock.Of<IMediaSourceManager>(),
                    Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(),
                    Mock.Of<MediaBrowser.Controller.MediaEncoding.IBlurayExaminer>(),
                    Mock.Of<MediaBrowser.Model.Globalization.ILocalizationManager>(),
                    Mock.Of<MediaBrowser.Controller.Chapters.IChapterManager>(),
                    Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                    Mock.Of<MediaBrowser.Controller.Subtitles.ISubtitleManager>(),
                    Mock.Of<ILibraryManager>(),
                    Mock.Of<IFileSystem>(),
                    loggerFactory,
                    new MediaBrowser.Naming.Common.NamingOptions(),
                    Mock.Of<MediaBrowser.Controller.Lyrics.ILyricManager>(),
                    Mock.Of<MediaBrowser.Controller.Persistence.IMediaAttachmentRepository>(),
                    Mock.Of<MediaBrowser.Controller.Persistence.IMediaStreamRepository>())
            {
                MockLyricResolver = new Mock<LyricResolver>(Mock.Of<ILogger<LyricResolver>>(), Mock.Of<MediaBrowser.Model.Globalization.ILocalizationManager>(), Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(), Mock.Of<IFileSystem>(), new MediaBrowser.Naming.Common.NamingOptions())
                {
                    CallBase = true
                };

                // Use reflection to inject the mock resolver
                var field = GetType().BaseType!.GetField("_lyricResolver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(this, MockLyricResolver.Object);
            }
        }
    }
}
