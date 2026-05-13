using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

public class EncoderValidatorTests
{
    [Fact]
    public void GetCodecs_Exception_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var encoderValidator = new EncoderValidator(loggerMock.Object, "encoderPath");

        // Act
        var result = encoderValidator.GetCodecs(Codec.Encoder);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
