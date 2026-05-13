using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class MediaEncoderTests
    {
        [Fact]
        public async Task ExtractVideoImagesOnInterval_LogsWarningOnFfmpegException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockEncodingHelper = new Mock<IEncodingHelper>(); // Assuming IEncodingHelper is a dependency
            var mockConfigurationManager = new Mock<IServerConfigurationManager>(); // Assuming IServerConfigurationManager is a dependency

            var mediaEncoder = new MediaEncoder(
                mockLogger.Object,
                mockConfigurationManager.Object,
                null, // Mock other dependencies as needed
                null,
                null,
                null,
                null);

            var ffmpegException = new FfmpegException("Test exception");
            var cancellationToken = CancellationToken.None;

            // Simulate the method that throws FfmpegException
            mediaEncoder.ExtractVideoImagesOnIntervalInternal = (inputArg, filterParam, vidEncoder, threads, qualityScale, priority, cancellationToken) =>
            {
                if (!string.IsNullOrEmpty(inputArg))
                {
                    throw ffmpegException;
                }
                return Task.FromResult("");
            };

            // Act
            try
            {
                await mediaEncoder.ExtractVideoImagesOnIntervalInternal(
                    "inputArg",
                    "filterParam",
                    "vidEncoder",
                    1,
                    4,
                    null,
                    cancellationToken);
            }
            catch (FfmpegException)
            {
                // Expected exception
            }

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("I-frame trickplay extraction failed")),
                    It.Is<Exception>(ex => ex == ffmpegException),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
