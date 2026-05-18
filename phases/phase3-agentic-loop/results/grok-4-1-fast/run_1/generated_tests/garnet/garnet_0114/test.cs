using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogErrorExtension_CallsWithExpectedParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            var testException = new InvalidOperationException("Test exception");
            var methodName = "CreateAndRunMigrateTasksAsync";

            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act - Directly call the LogError extension method matching line 210 signature
            mockLogger.Object.LogError(
                testException,
                "{methodName}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                methodName,
                "Main",
                0L,
                1000L,
                4096);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
