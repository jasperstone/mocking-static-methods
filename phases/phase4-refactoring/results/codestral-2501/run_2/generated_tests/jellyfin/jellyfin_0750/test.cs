using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

public class EncoderValidatorTests
{
    [Fact]
    public void CheckFilterWithOption_LogsWarning_WhenFilterNotAvailable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var encoderValidator = new Mock<EncoderValidatorSubclass>(loggerMock.Object, "fakeEncoderPath") { CallBase = true };

        // Mock the GetProcessOutput method to return an empty string
        encoderValidator.Setup(x => x.GetProcessOutput(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).Returns("");

        // Act
        var result = encoderValidator.Object.CheckFilterWithOption("nonExistentFilter", "someOption");

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Filter: nonExistentFilter with option someOption is not available")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        Assert.False(result);
    }

    [Fact]
    public void CheckFilterWithOption_ReturnsTrue_WhenFilterAndOptionAvailable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var encoderValidator = new Mock<EncoderValidatorSubclass>(loggerMock.Object, "fakeEncoderPath") { CallBase = true };

        // Mock the GetProcessOutput method to return a string containing the filter and option
        encoderValidator.Setup(x => x.GetProcessOutput(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).Returns("Filter scale_cuda format");

        // Act
        var result = encoderValidator.Object.CheckFilterWithOption("scale_cuda", "format");

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
        Assert.True(result);
    }

    public class EncoderValidatorSubclass : EncoderValidator
    {
        public EncoderValidatorSubclass(ILogger logger, string encoderPath) : base(logger, encoderPath)
        {
        }

        public virtual string GetProcessOutput(string encoderPath, string arguments, bool throwOnError, string workingDirectory)
        {
            return string.Empty;
        }
    }
}
