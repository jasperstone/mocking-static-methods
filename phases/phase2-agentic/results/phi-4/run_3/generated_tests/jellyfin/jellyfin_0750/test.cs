using System;
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
        var mockLogger = new Mock<ILogger>();
        var encoderValidator = new EncoderValidator(mockLogger.Object, "dummyPath");

        // Act
        encoderValidator.CheckFilterWithOption("nonexistentFilter", "nonexistentOption");

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Filter: nonexistentFilter with option nonexistentOption is not available")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
