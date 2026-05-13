using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Subtitles;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Providers.MediaInfo;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly ProbeProvider _probeProvider;

        public ProbeProviderTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _directoryServiceMock = new Mock<IDirectoryService>();

            _loggerFactoryMock.Setup(x => x.CreateLogger<ProbeProvider>()).Returns(_loggerMock.Object);

            // For dependencies of ProbeProvider constructor, we can pass null or mocks as they are not used in HasChanged
            _probeProvider = new ProbeProvider(
                mediaSourceManager: null!,
                mediaEncoder: null!,
                blurayExaminer: null!,
                localization: null!,
                chapterManager: null!,
                config: null!,
                subtitleManager: null!,
                libraryManager: null!,
                fileSystem: null!,
                loggerFactory: _loggerFactoryMock.Object,
                namingOptions: null!,
                lyricManager: null!,
                mediaAttachmentRepository: null!,
                mediaStreamRepository: null!);
        }

        [Fact]
        public void HasChanged_FileProtocolFileChanged_LogsDebugAndReturnsTrue()
        {
            var path = "file://test/path/file.mkv";
            var lastWriteTime = DateTime.UtcNow;

            var baseItemMock = new Mock<BaseItem>();
            baseItemMock.SetupGet(i => i.Path).Returns(path);
            baseItemMock.SetupGet(i => i.IsFileProtocol).Returns(true);
            baseItemMock.Setup(i => i.HasChanged(It.IsAny<DateTime>())).Returns(true);

            var fileMock = new Mock<IFile>();
            fileMock.SetupGet(f => f.LastWriteTimeUtc).Returns(lastWriteTime);

            _directoryServiceMock.Setup(d => d.GetFile(path)).Returns(fileMock.Object);

            var result = _probeProvider.HasChanged(baseItemMock.Object, _directoryServiceMock.Object);

            Assert.True(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Refreshing") && v.ToString()!.Contains(path)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void HasChanged_VideoWithSubtitleFilesChanged_LogsDebugAndReturnsTrue()
        {
            var video = new Mock<Video>();
            video.SetupGet(v => v.VideoType).Returns(VideoType.Movie);
            video.SetupGet(v => v.IsPlaceHolder).Returns(false);
            video.SetupGet(v => v.SupportsLocalMetadata).Returns(true);
            video.SetupGet(v => v.Path).Returns("videoPath");
            video.SetupGet(v => v.SubtitleFiles).Returns(new List<string> { "sub1.srt" });
            video.SetupGet(v => v.AudioFiles).Returns(new List<string> { "audio1.mp3" });

            var baseItem = video.Object;

            // Setup subtitle resolver to return different subtitle files to trigger change
            var subtitleResolverMock = new Mock<SubtitleResolver>(
                Mock.Of<ILogger<SubtitleResolver>>(),
                null!, null!, null!, null!);
            subtitleResolverMock.Setup(s => s.GetExternalFiles(video.Object, _directoryServiceMock.Object, false))
                .Returns(new List<IFileInfo> { new FakeFileInfo("different.srt") });

            // Replace private field _subtitleResolver using reflection
            var subtitleResolverField = typeof(ProbeProvider).GetField("_subtitleResolver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            subtitleResolverField!.SetValue(_probeProvider, subtitleResolverMock.Object);

            // Setup audio resolver to return same audio files to avoid triggering audio change
            var audioResolverMock = new Mock<AudioResolver>(
                Mock.Of<ILogger<AudioResolver>>(),
                null!, null!, null!, null!);
            audioResolverMock.Setup(a => a.GetExternalFiles(video.Object, _directoryServiceMock.Object, false))
                .Returns(new List<IFileInfo> { new FakeFileInfo("audio1.mp3") });
            var audioResolverField = typeof(ProbeProvider).GetField("_audioResolver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            audioResolverField!.SetValue(_probeProvider, audioResolverMock.Object);

            var result = _probeProvider.HasChanged(baseItem, _directoryServiceMock.Object);

            Assert.True(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Refreshing") && v.ToString()!.Contains("external subtitles change")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void HasChanged_AudioWithLyricFilesChanged_LogsDebugAndReturnsTrue()
        {
            var audio = new Mock<Audio>();
            audio.SetupGet(a => a.SupportsLocalMetadata).Returns(true);
            audio.SetupGet(a => a.Path).Returns("audioPath");
            audio.SetupGet(a => a.LyricFiles).Returns(new List<string> { "lyric1.lrc" });

            var baseItem = audio.Object;

            // Setup lyric resolver to return different lyric files to trigger change
            var lyricResolverMock = new Mock<LyricResolver>(
                Mock.Of<ILogger<LyricResolver>>(),
                null!, null!, null!, null!);
            lyricResolverMock.Setup(l => l.GetExternalFiles(audio.Object, _directoryServiceMock.Object, false))
                .Returns(new List<IFileInfo> { new FakeFileInfo("different.lrc") });

            var lyricResolverField = typeof(ProbeProvider).GetField("_lyricResolver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            lyricResolverField!.SetValue(_probeProvider, lyricResolverMock.Object);

            var result = _probeProvider.HasChanged(baseItem, _directoryServiceMock.Object);

            Assert.True(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Refreshing") && v.ToString()!.Contains("external lyrics change")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class FakeFileInfo : IFileInfo
        {
            public FakeFileInfo(string path)
            {
                Path = path;
            }

            public string Path { get; }

            public DateTime LastWriteTimeUtc => DateTime.UtcNow;

            public long Length => 0;
        }
    }
}
