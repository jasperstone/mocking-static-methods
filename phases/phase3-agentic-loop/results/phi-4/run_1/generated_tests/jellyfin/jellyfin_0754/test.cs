using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

public class EncoderValidatorTests
{
    private enum Codec
    {
        Encoder,
        Decoder
    }

    [Fact]
    public void GetCodecs_LogsError_WhenExceptionOccurs()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var encoderPath = "path/to/encoder";
        var encoderValidator = new EncoderValidator(mockLogger.Object, encoderPath);

        // Act
        var result = encoderValidator.GetCodecs(Codec.Encoder);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<Exception>(),
                "Error detecting available {Codec}",
                It.Is<string>(s => s == "encoders")
            ),
            Times.Once
        );
    }
}
