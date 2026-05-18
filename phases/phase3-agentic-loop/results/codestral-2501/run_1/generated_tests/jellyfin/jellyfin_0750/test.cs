using Xunit;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class EncoderValidatorTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly EncoderValidator _encoderValidator;

        public EncoderValidatorTests()
        {
            _mockLogger = new Mock<ILogger>();
            _encoderValidator = new EncoderValidator(_mockLogger.Object, "encoderPath");
        }

        [Fact]
        public void CheckFilterWithOption_ValidFilterAndOption_LogsWarning()
        {
            // Arrange
            var filter = "scale_cuda";
            var option = "format";

            // Act
            var result = _encoderValidator.CheckFilterWithOption(filter, option);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Filter: scale_cuda with option format is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_ValidFilterAndOption_LogsWarning()
        {
            // Arrange
            var filter = "hevc_metadata";
            var option = "remove_dovi";

            // Act
            var result = _encoderValidator.CheckBitStreamFilterWithOption(filter, option);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Bit stream filter: hevc_metadata with option remove_dovi is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
