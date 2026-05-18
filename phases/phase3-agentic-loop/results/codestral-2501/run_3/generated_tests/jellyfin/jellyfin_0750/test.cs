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
        var loggerMock = new Mock<ILogger<EncoderValidator>>();
        var encoderValidator = new EncoderValidator(loggerMock.Object, "fakeEncoderPath");

        // Act
        var result = encoderValidator.CheckFilterWithOption("nonExistentFilter", "nonExistentOption");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
        Assert.False(result);
    }

    [Fact]
    public void CheckBitStreamFilterWithOption_LogsWarning_WhenFilterAndOptionNotAvailable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<EncoderValidator>>();
        var encoderValidator = new EncoderValidator(loggerMock.Object, "fakeEncoderPath");

        // Act
        var result = encoderValidator.CheckBitStreamFilterWithOption("nonExistentFilter", "nonExistentOption");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
        Assert.False(result);
    }
}
