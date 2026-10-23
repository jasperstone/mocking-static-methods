using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;
using System;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void GetCodecs_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<EncoderValidator>>();
            var encoderValidator = new EncoderValidator(mockLogger.Object, "fakeEncoderPath");

            // Act
            var result = encoderValidator.GetCodecs(Codec.Encoder);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
