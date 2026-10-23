using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_FormatsMessageWithException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new InvalidOperationException("Test exception");
            var methodName = "CreateAndRunMigrateTasksAsync";
            var storeType = "MainStore";
            long beginAddress = 123;
            long tailAddress = 456;
            int pageSize = 789;

            // Act
            loggerMock.Object.LogError(exception, "{method}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                methodName, storeType, beginAddress, tailAddress, pageSize);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains(methodName) &&
                        v.ToString().Contains(storeType) &&
                        v.ToString().Contains(beginAddress.ToString()) &&
                        v.ToString().Contains(tailAddress.ToString()) &&
                        v.ToString().Contains(pageSize.ToString())),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
