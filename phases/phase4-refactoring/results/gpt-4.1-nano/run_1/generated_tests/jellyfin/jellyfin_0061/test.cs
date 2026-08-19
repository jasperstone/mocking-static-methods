using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Jellyfin.Server.Implementations.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_Should_Log_Exception_With_Message()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new InvalidOperationException("Test exception");
            var path = "some/path";

            // Act
            // Simulate calling the extension method that logs error
            loggerMock.Object.LogError(exception, "Error watching path: {Path}", path);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error watching path")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
