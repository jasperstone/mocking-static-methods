using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests.MediaEncoding.Encoder
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void GetCodecs_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(mockLogger.Object, "dummyPath");

            // Act
            var result = encoderValidator.GetCodecs(Codec.Encoder);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Error detecting available {Codec}",
                    It.Is<string>(codec => codec == "encoders")
                ),
                Times.Once
            );

            Assert.Empty(result);
        }
    }
}
