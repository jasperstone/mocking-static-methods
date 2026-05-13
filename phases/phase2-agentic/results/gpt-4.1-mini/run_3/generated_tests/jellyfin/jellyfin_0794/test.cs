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
        // We want to test the code path that calls _logger.LogError("ffmpeg subtitle conversion failed for {Path}", inputPath);
        // This happens when the subtitle conversion fails and the output file is missing or empty, causing a FfmpegException to be thrown.

        [Fact]
        public async Task GetSubtitles_ConversionFails_LogsErrorAndThrowsFfmpegException()
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

            var baseItem = new Mock<BaseItem>().Object;
            var mediaSourceId = "mediaSourceId";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var startTimeTicks = 0L;
            var endTimeTicks = 0L;
            var preserveOriginalTimestamps = false;
            var cancellationToken = CancellationToken.None;

            // Setup media source and streams
            var mediaStream = new MediaStream
            {
                Type = MediaStreamType.Subtitle,
                Index = subtitleStreamIndex,
                Codec = "srt"
            };

            var mediaSource = new MediaSourceInfo
            {
                Id = mediaSourceId,
                MediaStreams = new[] { mediaStream }
            };

            mediaSourceManagerMock
                .Setup(m => m.GetPlaybackMediaSources(baseItem, null, true, false, cancellationToken))
                .ReturnsAsync(new[] { mediaSource });

            // Setup GetReadableFile to return a SubtitleInfo with a path
            var subtitleInfo = new SubtitleInfo
            {
                Path = "inputPath",
                Format = "srt",
                Protocol = MediaProtocol.File
            };

            // We need to mock GetReadableFile and GetSubtitleStream to simulate the subtitle stream retrieval
            // Unfortunately, these are private methods, so we cannot mock them directly.
            // Instead, we will mock the subtitleParser.Parse to throw an exception to simulate conversion failure.

            // Setup subtitleParser.Parse to throw to simulate failure in ConvertSubtitles
            subtitleParserMock
                .Setup(p => p.Parse(It.IsAny<Stream>(), It.IsAny<string>()))
                .Throws(new Exception("Parse failure"));

            // Setup file system to simulate output file existence and length
            var outputPath = "outputPath";

            // We need to simulate the file system calls that happen in the conversion failure path:
            // The code tries to delete the output file and logs errors if deletion fails.

            // Setup file system to say output file exists and length is 0 to trigger failure
            fileSystemMock.Setup(fs => fs.FileExists(outputPath)).Returns(false);
            fileSystemMock.Setup(fs => fs.GetFileInfo(outputPath)).Returns(new FileInfoStub(0));

            // Setup DeleteFile to throw IOException to test LogError call
            fileSystemMock.Setup(fs => fs.DeleteFile(outputPath)).Throws(new IOException("Delete failed"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<FfmpegException>(async () =>
            {
                // We call the interface method explicitly
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

            // Verify that LogError was called with the expected message containing "ffmpeg subtitle conversion failed for"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ffmpeg subtitle conversion failed for")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        // Helper stub for IFileInfo
        private class FileInfoStub : IFileInfo
        {
            public FileInfoStub(long length)
            {
                Length = length;
            }

            public bool Exists => true;
            public long Length { get; }
            public string Name => "stub";
            public string FullName => "stub";
            public DateTime LastWriteTimeUtc => DateTime.UtcNow;
            public DateTime LastAccessTimeUtc => DateTime.UtcNow;
            public DateTime CreationTimeUtc => DateTime.UtcNow;
            public bool IsDirectory => false;
        }
    }
}
