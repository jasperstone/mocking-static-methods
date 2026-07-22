using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

public class EncoderValidatorTests
{
    private class TestEncoderValidator : EncoderValidator
    {
        public TestEncoderValidator(ILogger logger, string encoderPath) : base(logger, encoderPath)
        {
        }

        protected override string GetProcessOutput(string encoderPath, string arguments, bool throwOnError, string workingDirectory)
        {
            return "Filter nonExistentFilter with option nonExistentOption is not available";
        }
    }

    [Fact]
    public void CheckFilterWithOption_LogsWarning_WhenFilterAndOptionAreNotAvailable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<EncoderValidator>>();
        var encoderValidator = new TestEncoderValidator(loggerMock.Object, "fakeEncoderPath");

        // Act
        var result = encoderValidator.CheckFilterWithOption("nonExistentFilter", "nonExistentOption");

        // Assert
        loggerMock.Verify(
            logger => logger.LogWarning(
                "Filter: {Name} with option {Option} is not available",
                "nonExistentFilter",
                "nonExistentOption"),
            Times.Once);
        Assert.False(result);
    }
}
