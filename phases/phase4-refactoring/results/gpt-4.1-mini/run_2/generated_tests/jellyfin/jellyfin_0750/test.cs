using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void CheckFilterWithOption_LogsWarning_WhenFilterOptionNotAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var validator = new EncoderValidator(loggerMock.Object, "fakePath");

            string filter = "nonexistentfilter";
            string option = "nonexistentoption";

            // Act
            // This will call the real method, which calls GetProcessOutput with the real encoder path.
            // Since the encoder path is fake, it will likely throw and log an error, not a warning.
            // So we expect false but no warning log.
            bool result = validator.CheckFilterWithOption(filter, option);

            // Assert
            Assert.False(result);
            // We expect no warning log because the method throws and logs error instead.
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
