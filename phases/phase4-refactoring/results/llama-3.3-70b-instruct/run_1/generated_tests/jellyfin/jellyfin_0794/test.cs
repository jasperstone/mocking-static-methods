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
            var fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var httpClientFactoryMock = new Mock<Microsoft.Extensions.Http.IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<MediaBrowser.Controller.Library.IMediaSourceManager>();
            var subtitleParserMock = new Mock<MediaBrowser.MediaEncoding.Subtitles.ISubtitleParser>();
            var pathManagerMock = new Mock<MediaBrowser.Controller.IO.IPathManager>();
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

            var exception = new Exception("Test exception");
            var message = "Test message {Path}";
            var path = "test_path";

            // Act
            ((ILogger<SubtitleEncoder>)subtitleEncoder._logger).LogError(exception, message, path);

            // Assert
            loggerMock.Verify(l => l.LogError(exception, message, path), Times.Once);
        }

        [Fact]
        public void LogError_CalledWithMessage_LoggerErrorCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var httpClientFactoryMock = new Mock<Microsoft.Extensions.Http.IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<MediaBrowser.Controller.Library.IMediaSourceManager>();
            var subtitleParserMock = new Mock<MediaBrowser.MediaEncoding.Subtitles.ISubtitleParser>();
            var pathManagerMock = new Mock<MediaBrowser.Controller.IO.IPathManager>();
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

            var message = "Test message {Path}";
            var path = "test_path";

            // Act
            ((ILogger<SubtitleEncoder>)subtitleEncoder._logger).LogError(message, path);

            // Assert
            loggerMock.Verify(l => l.LogError(message, path), Times.Once);
        }
    }
}
