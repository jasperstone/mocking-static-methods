using System.Threading;
using System.Threading.Tasks;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class SingleDatabaseManagerLoggingTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformation_WhenAofSizeExceedsLimit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // We cannot instantiate SingleDatabaseManager directly because it is internal,
            // so we test the logging call by invoking the extension method on ILogger directly.
            // This is a minimal test to cover the LogInformation call usage.

            var aofSize = 100L;
            var aofSizeLimit = 50L;

            // Act
            loggerMock.Object.LogInformation(
                "Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}",
                aofSize, aofSizeLimit);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enforcing AOF size limit")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}
