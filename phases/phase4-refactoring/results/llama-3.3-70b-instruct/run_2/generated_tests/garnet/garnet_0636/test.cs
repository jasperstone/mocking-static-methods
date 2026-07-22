using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsavorite.core
{
    public class DeltaLogTests
    {
        [Fact]
        public async Task FlushAsync_CompletesSuccessfully()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(Mock.Of<IDevice>(), 10, 100, loggerMock.Object);

            // Act
            await deltaLog.FlushAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_LogsErrorOnError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(Mock.Of<IDevice>(), 10, 100, loggerMock.Object);
            var context = new object();

            // Act
            var privateType = deltaLog.GetType();
            var privateMethod = privateType.GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            privateMethod.Invoke(deltaLog, new object[] { 1, 0, context });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
