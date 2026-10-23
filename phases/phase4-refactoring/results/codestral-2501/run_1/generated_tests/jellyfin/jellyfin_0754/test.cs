using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.MediaEncoding.Encoder;

public class EncoderValidatorTests
{
    [Fact]
    public void GetCodecs_LogsErrorAndReturnsEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<EncoderValidator>>();
        var encoderValidator = new EncoderValidator(loggerMock.Object, "fakeEncoderPath");

        var exception = new InvalidOperationException("Test exception");

        // Mock the GetProcessOutput method to throw an exception
        var encoderValidatorMock = new Mock<EncoderValidator>(loggerMock.Object, "fakeEncoderPath");
        encoderValidatorMock.Setup(x => x.GetProcessOutput(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Throws(exception);

        // Act
        var result = encoderValidatorMock.Object.GetCodecs(Codec.Encoder);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);

        Assert.Empty(result);
    }
}
