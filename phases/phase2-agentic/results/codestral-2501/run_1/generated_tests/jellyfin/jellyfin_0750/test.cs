using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

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
    public void CheckFilterWithOption_LogsWarning_WhenFilterAndOptionNotAvailable()
    {
        // Arrange
        var filter = "nonexistent_filter";
        var option = "nonexistent_option";

        // Act
        var result = _encoderValidator.CheckFilterWithOption(filter, option);

        // Assert
        _mockLogger.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        Assert.False(result);
    }

    [Fact]
    public void CheckBitStreamFilterWithOption_LogsWarning_WhenFilterAndOptionNotAvailable()
    {
        // Arrange
        var filter = "nonexistent_filter";
        var option = "nonexistent_option";

        // Act
        var result = _encoderValidator.CheckBitStreamFilterWithOption(filter, option);

        // Assert
        _mockLogger.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        Assert.False(result);
    }
}
