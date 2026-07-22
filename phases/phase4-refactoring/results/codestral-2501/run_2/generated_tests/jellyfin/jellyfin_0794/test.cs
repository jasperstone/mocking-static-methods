using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.MediaEncoding.Subtitles;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using System.Net.Http;

namespace MediaBrowser.MediaEncoding.Tests.Subtitles
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public async Task ConvertSubtitles_Failure_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var subtitleParserMock = new Mock<ISubtitleParser>();
            var pathManagerMock = new Mock<IPathManager>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigurationManagerMock.Object);

            var inputPath = "inputPath";
            var outputPath = "outputPath";

            fileSystemMock.Setup(fs => fs.GetFileInfo(outputPath)).Returns(new FileInfoWrapper(new FileInfo(outputPath)));
            fileSystemMock.Setup(fs => fs.DeleteFile(outputPath)).Throws(new IOException());

            // Act
            await Assert.ThrowsAsync<FfmpegException>(() => subtitleEncoder.ConvertSubtitles(
                new MemoryStream(),
                "inputFormat",
                "outputFormat",
                0,
                0,
                false,
                CancellationToken.None));

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Once);
        }
    }

    public class FileInfoWrapper : IFileInfo
    {
        private readonly FileInfo _fileInfo;

        public FileInfoWrapper(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        public long Length => _fileInfo.Length;
    }

    public interface IFileInfo
    {
        long Length { get; }
    }

    public class FfmpegException : Exception
    {
        public FfmpegException(string message) : base(message)
        {
        }
    }
}
