using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

public class EncoderValidatorTests
{
    [Fact]
    public void CheckFilterWithOption_LogsWarning_WhenFilterAndOptionNotAvailable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var encoderValidator = new EncoderValidator(loggerMock.Object, "fakeEncoderPath");

        var encoderValidatorMock = new Mock<EncoderValidator>(loggerMock.Object, "fakeEncoderPath");
        encoderValidatorMock.Setup(validator => validator.GetProcessOutput(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Action<string>>()))
            .Returns("");

        // Act
        var result = encoderValidator.CheckFilterWithOption("nonExistentFilter", "nonExistentOption");

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
        Assert.False(result);
    }
}
