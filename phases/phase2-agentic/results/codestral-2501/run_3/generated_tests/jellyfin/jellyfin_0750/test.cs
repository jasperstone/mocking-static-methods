using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

public class EncoderValidatorTests
{
    private readonly Mock<ILogger<EncoderValidator>> _mockLogger;
    private readonly EncoderValidator _encoderValidator;

    public EncoderValidatorTests()
    {
        _mockLogger = new Mock<ILogger<EncoderValidator>>();
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Filter: nonexistent_filter with option nonexistent_option is not available")),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Bit stream filter: nonexistent_filter with option nonexistent_option is not available")),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Once);

        Assert.False(result);
    }
}
