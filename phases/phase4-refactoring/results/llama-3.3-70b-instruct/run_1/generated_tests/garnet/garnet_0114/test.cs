using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {        [Fact(Skip = "MigrateSession is internal sealed")]
        public void LogError_CallsLoggerLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);

            // Act
            migrateSession.LogError(new Exception("Test exception"), "Test message");

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}
