using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

public class EncoderValidatorTests
{
    [Fact]
    public void ValidateVersion_ExceptionLogged()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<EncoderValidator>>();
        var encoderValidator = new EncoderValidator(loggerMock.Object, "fakeEncoderPath");

        // Act
        var result = encoderValidator.ValidateVersion();

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
