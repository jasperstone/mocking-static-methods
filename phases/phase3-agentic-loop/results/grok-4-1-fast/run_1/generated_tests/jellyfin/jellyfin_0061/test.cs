using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorLoggerTests
    {
        [Fact]
        public void LogErrorExtension_CalledWithExceptionAndPath_LogsErrorMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LibraryMonitor>>();
            var exception = new IOException("Test IO exception");
            var path = "/test/path";
            
            // Act
            mockLogger.Object.LogError(exception, "Error watching path: {Path}", path);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error watching path: ") && v.ToString()!.Contains(path)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
