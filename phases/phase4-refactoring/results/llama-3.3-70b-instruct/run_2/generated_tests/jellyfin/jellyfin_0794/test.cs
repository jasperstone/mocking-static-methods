using MediaBrowser.MediaEncoding.Subtitles;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public void LogError_CalledWithExceptionAndMessage_LoggerErrorCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<MediaBrowser.Common.IO.IFileSystem>();
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var httpClientFactoryMock = new Mock<Microsoft.Extensions.Http.IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<MediaBrowser.Controller.Library.IMediaSourceManager>();
            var subtitleParserMock = new Mock<MediaBrowser.MediaEncoding.Subtitles.ISubtitleParser>();
            var pathManagerMock = new Mock<MediaBrowser.Common.IO.IPathManager>();
            var serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigurationManagerMock.Object);

            var exception = new IOException();
            var message = "Error deleting converted subtitle {Path}";
            var outputPath = "outputPath";

            // Act
            try
            {
                fileSystemMock.Setup(f => f.DeleteFile(outputPath)).Throws(exception);
                loggerMock.Object.LogError(exception, message, outputPath);
            }
            catch
            {
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void LogError_CalledWithMessage_LoggerErrorCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<MediaBrowser.Common.IO.IFileSystem>();
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var httpClientFactoryMock = new Mock<Microsoft.Extensions.Http.IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<MediaBrowser.Controller.Library.IMediaSourceManager>();
            var subtitleParserMock = new Mock<MediaBrowser.MediaEncoding.Subtitles.ISubtitleParser>();
            var pathManagerMock = new Mock<MediaBrowser.Common.IO.IPathManager>();
            var serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigurationManagerMock.Object);

            var message = "ffmpeg subtitle conversion failed for {Path}";
            var inputPath = "inputPath";

            // Act
            loggerMock.Object.LogError(message, inputPath);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
