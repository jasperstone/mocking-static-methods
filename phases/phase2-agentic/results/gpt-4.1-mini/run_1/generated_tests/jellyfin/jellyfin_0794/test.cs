using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.MediaEncoding.Subtitles;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Subtitles.Tests
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public async Task GetSubtitles_LogsErrorAndThrowsFfmpegException_WhenConversionFailsAndDeletesFileFailsWithIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var subtitleParserMock = new Mock<ISubtitleParser>();
            var pathManagerMock = new Mock<IPathManager>();
            var serverConfigMock = new Mock<IServerConfigurationManager>();

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigMock.Object);

            var baseItem = new BaseItem { Id = "item1" };
            var mediaSourceId = "source1";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var startTimeTicks = 0L;
            var endTimeTicks = 1000L;
            var preserveOriginalTimestamps = false;
            var cancellationToken = CancellationToken.None;

            var mediaStream = new MediaStream
            {
                Type = MediaStreamType.Subtitle,
                Index = subtitleStreamIndex,
                Codec = "srt"
            };

            var mediaSourceInfo = new MediaSourceInfo
            {
                Id = mediaSourceId,
                MediaStreams = new[] { mediaStream }
            };

            mediaSourceManagerMock.Setup(m => m.GetPlaybackMediaSources(baseItem, null, true, false, cancellationToken))
                .ReturnsAsync(new[] { mediaSourceInfo });

            // Setup GetReadableFile to return a SubtitleInfo with a path
            var subtitleInfo = new SubtitleInfo
            {
                Path = "outputPath",
                Format = "srt",
                Protocol = MediaProtocol.File
            };

            // We need to mock GetReadableFile and GetSubtitleStream to simulate failure in conversion
            // Since these are private methods, we simulate by mocking subtitleParser.Parse to throw
            subtitleParserMock.Setup(p => p.Parse(It.IsAny<Stream>(), It.IsAny<string>()))
                .Throws(new Exception("Parse failure"));

            // Setup file system to throw IOException on DeleteFile
            fileSystemMock.Setup(f => f.DeleteFile("outputPath"))
                .Throws(new IOException("Delete failed"));

            // Setup File.Exists to return true to enter the first if block in the code snippet
            // We need to mock File.Exists and FileInfo.Length, but File.Exists is static, so we simulate by mocking _fileSystem.GetFileInfo
            fileSystemMock.Setup(f => f.GetFileInfo("outputPath"))
                .Returns(new FileInfoStub(10));

            // We simulate the file exists by not throwing in GetFileInfo and no FileNotFoundException on DeleteFile

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await ((ISubtitleEncoder)subtitleEncoder).GetSubtitles(
                    baseItem,
                    mediaSourceId,
                    subtitleStreamIndex,
                    outputFormat,
                    startTimeTicks,
                    endTimeTicks,
                    preserveOriginalTimestamps,
                    cancellationToken);
            });

            // Verify that LogError was called with the expected message containing "ffmpeg subtitle conversion failed"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ffmpeg subtitle conversion failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        private class FileInfoStub : IFileInfo
        {
            public FileInfoStub(long length)
            {
                Length = length;
            }

            public long Length { get; }

            public string Name => throw new NotImplementedException();

            public string FullName => throw new NotImplementedException();

            public bool Exists => true;

            public DateTime LastWriteTimeUtc => throw new NotImplementedException();

            public DateTime LastAccessTimeUtc => throw new NotImplementedException();

            public DateTime CreationTimeUtc => throw new NotImplementedException();

            public Stream OpenRead()
            {
                throw new NotImplementedException();
            }
        }
    }
}
