using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace Microsoft.Extensions.Logging.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogErrorExtension_CalledWithCorrectParameters_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ));

            ILogger logger = mockLogger.Object;
            string method = "TryAddReplicationTasks";
            long startAddress = 500;
            long truncatedUntil = 1000;

            // Act - Directly test the extension method pattern used on line 271
            logger.LogError("{method} failed to add tasks for AOF sync {startAddress} {truncatedUntil}", 
                           method, startAddress, truncatedUntil);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString().Contains("TryAddReplicationTasks") &&
                    v.ToString().Contains("failed to add tasks") &&
                    v.ToString().Contains("500") &&
                    v.ToString().Contains("1000")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }

        [Fact]
        public void LogErrorExtension_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = null;
            string method = "TryAddReplicationTasks";
            long startAddress = 500;
            long truncatedUntil = 1000;

            // Act & Assert - Uses null-conditional operator logger?.LogError
            Assert.DoesNotThrow(() => logger?.LogError("{method} failed to add tasks for AOF sync {startAddress} {truncatedUntil}", 
                                                       method, startAddress, truncatedUntil));
        }
    }
}
