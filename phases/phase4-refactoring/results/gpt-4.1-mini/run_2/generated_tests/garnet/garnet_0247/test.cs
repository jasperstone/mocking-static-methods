using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_IsCalledWithExpectedMessageAndException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var testException = new Exception("Test exception");

            // Act
            LoggerTestHelper.LogWarningWithException(loggerMock.Object, testException);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(
                    testException,
                    "An exception occurred at ReplicationManager.ProcessPrimaryStream"),
                Times.Once);
        }

        private static class LoggerTestHelper
        {
            public static void LogWarningWithException(ILogger logger, Exception ex)
            {
                logger?.LogWarning(ex, "An exception occurred at ReplicationManager.ProcessPrimaryStream");
            }
        }
    }
}
