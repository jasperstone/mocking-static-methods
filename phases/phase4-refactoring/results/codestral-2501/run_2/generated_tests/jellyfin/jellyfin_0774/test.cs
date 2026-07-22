using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Dlna;

namespace MediaBrowser.MediaEncoding.Tests
{
    public class MediaEncoderTests
    {
        [Fact]
        public async Task ExtractVideoImagesOnInterval_LogsWarning_WhenFfmpegExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var blurayExaminerMock = new Mock<IBlurayExaminer>();
            var localizationMock = new Mock<ILocalizationManager>();
            var configMock = new Mock<IConfiguration>();
            var serverConfigMock = new Mock<IServerConfigurationManager>();

            var mediaEncoder = new MediaEncoder(
                loggerMock.Object,
                configurationManagerMock.Object,
                fileSystemMock.Object,
                blurayExaminerMock.Object,
                localizationMock.Object,
                configMock.Object,
                serverConfigMock.Object);

            var jobState = new JobState();
            var options = new EncodingOptions();
            var vidEncoder = "vidEncoder";
            var inputFile = "inputFile";
            var threads = 1;
            var qualityScale = 1;
            var priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            // Act
            await mediaEncoder.ExtractVideoImagesOnInterval(jobState, options, vidEncoder, inputFile, threads, qualityScale, priority, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<FfmpegException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
