using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public void LogError_CalledWithExceptionAndMessage_LoggerLogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            var message = "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}";
            var storeType = "Main";
            var beginAddress = 0L;
            var tailAddress = 10L;
            var pageSize = 1024;

            // Act
            loggerMock.Object.LogError(exception, message, "CreateAndRunMigrateTasks", storeType, beginAddress, tailAddress, pageSize);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
