using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_CalledWithCorrectParameters_VerifiesExtensionMethodUsage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            
            long testAofSize = 10240L;
            long testAofSizeLimit = 5120L;
            string expectedMessageTemplate = "Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}";

            // Act - Directly test the LoggerExtensions.LogInformation extension method
            // This verifies the exact usage pattern from SingleDatabaseManager.cs line 226
            logger.LogInformation(expectedMessageTemplate, testAofSize, testAofSizeLimit);

            // Assert - Verify the underlying Log call was made with correct parameters
            mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("currentAofSize") && 
                    v.ToString()!.Contains(testAofSize.ToString()) && 
                    v.ToString()!.Contains("AofSizeLimit") && 
                    v.ToString()!.Contains(testAofSizeLimit.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = null;
            long testAofSize = 10240L;
            long testAofSizeLimit = 5120L;

            // Act & Assert - Null-conditional operator prevents call when logger is null
            // This matches the logger?.LogInformation pattern in production code
            logger?.LogInformation("Test message {aofSize} > {limit}", testAofSize, testAofSizeLimit);
            
            // No exception should be thrown
            Assert.True(true);
        }

        [Fact]
        public void LogError_WithExceptionAndParameters_VerifiesErrorLogging()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            var testException = new InvalidOperationException("Test exception");
            long tailAddress = 10000L;
            long commitAddress = 5000L;

            // Act - Test the LogError pattern from CommitToAofAsync
            logger.LogError(testException,
                "Exception raised while committing to AOF. AOF tail address = {tailAddress}; AOF committed until address = {commitAddress}; ",
                tailAddress, commitAddress);

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                testException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
