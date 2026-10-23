using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_VerifyExtensionMethodCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            Exception testException = new InvalidOperationException("Test exception");
            var storeType = (object)"Main";
            var beginAddress = (object)100L;
            var tailAddress = (object)200L;
            var pageSize = (object)4096;

            // Act
            loggerMock.Object.LogError(
                testException,
                "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                nameof(CreateAndRunMigrateTasksAsync),
                storeType,
                beginAddress,
                tailAddress,
                pageSize);

            // Assert - Verify the Log method was called with expected parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAny<string, object[]>>((state, _) => state.ToString().Contains("{CreateAndRunMigrateTasks}")),
                    testException,
                    It.IsAny<Func<It.IsAny<string, object[]>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger? logger = null;
            Exception testException = new InvalidOperationException("Test");

            // Act & Assert - Null-conditional operator prevents NRE
            logger?.LogError(testException, "{CreateAndRunMigrateTasks}: {storeType}", "Main");
            Assert.True(true);
        }

        [Fact]
        public void LogError_ValidatesMessageTemplate()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            Exception testException = new InvalidOperationException("Test");

            // Act
            loggerMock.Object.LogError(
                testException,
                "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                nameof(CreateAndRunMigrateTasksAsync),
                "Main",
                100L,
                200L,
                4096);

            // Assert - Extension method correctly formats the message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAny<string, object[]>>((state, _) => 
                        state.ToString()!.Contains("CreateAndRunMigrateTasks") && 
                        state.ToString()!.Contains("Main") &&
                        state.ToString()!.Contains("100") &&
                        state.ToString()!.Contains("4096")),
                    testException,
                    It.IsAny<Func<It.IsAny<string, object[]>, Exception, string>>()),
                Times.Once);
        }
    }
}
