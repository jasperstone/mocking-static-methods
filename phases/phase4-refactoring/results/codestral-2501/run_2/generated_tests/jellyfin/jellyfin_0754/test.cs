using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;
using System;

public class EncoderValidatorTests
{
    [Fact]
    public void GetCodecs_ThrowsException_LogsError()
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
                It.IsAny<Exception>(),
                It.IsAny<Func<Exception, string>>(),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
