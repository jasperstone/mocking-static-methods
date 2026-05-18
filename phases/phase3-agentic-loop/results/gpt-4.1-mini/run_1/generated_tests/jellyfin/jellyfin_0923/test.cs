using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Providers.MediaInfo;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        private class TestDirectoryService : MediaBrowser.Model.IO.IDirectoryService
        {
            private readonly MediaBrowser.Model.IO.IFile _file;

            public TestDirectoryService(MediaBrowser.Model.IO.IFile file)
            {
                _file = file;
            }

            public MediaBrowser.Model.IO.IFile GetFile(string path) => _file;
        }

        private class TestFile : MediaBrowser.Model.IO.IFile
        {
            public DateTime LastWriteTimeUtc { get; set; }
        }

        private class FileInfoStub : MediaBrowser.Model.IO.IFileInfo
        {
            public string Path { get; set; }
        }

        [Fact]
        public void HasChanged_LogsDebug_WhenExternalLyricsChange()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ProbeProvider>()).Returns(loggerMock.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<AudioResolver>()).Returns(Mock.Of<ILogger<AudioResolver>>());
            loggerFactoryMock.Setup(f => f.CreateLogger<SubtitleResolver>()).Returns(Mock.Of<ILogger<SubtitleResolver>>());
            loggerFactoryMock.Setup(f => f.CreateLogger<LyricResolver>()).Returns(Mock.Of<ILogger<LyricResolver>>());
            loggerFactoryMock.Setup(f => f.CreateLogger<FFProbeVideoInfo>()).Returns(Mock.Of<ILogger<FFProbeVideoInfo>>());
            loggerFactoryMock.Setup(f => f.CreateLogger<AudioFileProber>()).Returns(Mock.Of<ILogger<AudioFileProber>>());

            var probeProvider = new ProbeProvider(
                null, null, null, null, null, null, null, null, null,
                loggerFactoryMock.Object, null, null, null, null);

            // Setup Audio item with lyric files
            var audio = new Audio
            {
                LyricFiles = new List<string> { "lyric2.lrc" },
                SupportsLocalMetadata = true,
                Path = "audioPath"
            };

            // Setup lyric resolver to return different files than audio.LyricFiles
            var lyricFiles = new List<MediaBrowser.Model.IO.IFileInfo> { new FileInfoStub { Path = "lyric1.lrc" } };

            // Use reflection to set private _lyricResolver field to a test resolver returning lyricFiles
            var lyricResolverField = typeof(ProbeProvider).GetField("_lyricResolver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            lyricResolverField.SetValue(probeProvider, new TestLyricResolver(lyricFiles));

            // Act
            var changed = probeProvider.HasChanged(audio, new TestDirectoryService(null));

            // Assert
            Assert.True(changed);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing") && v.ToString().Contains(audio.Path) && v.ToString().Contains("lyrics")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestLyricResolver : LyricResolver
        {
            private readonly IEnumerable<MediaBrowser.Model.IO.IFileInfo> _files;

            public TestLyricResolver(IEnumerable<MediaBrowser.Model.IO.IFileInfo> files) : base(null, null, null, null, null)
            {
                _files = files;
            }

            public override IEnumerable<MediaBrowser.Model.IO.IFileInfo> GetExternalFiles(Audio audio, MediaBrowser.Model.IO.IDirectoryService directoryService, bool includeNested)
            {
                return _files;
            }
        }
    }
}
