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
        public async Task LogError_Called_When_ErrorCode_Is_Not_Zero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(Mock.Of<IDevice>(), 10, 0, loggerMock.Object);

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(1, 0, null);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task LogError_Not_Called_When_ErrorCode_Is_Zero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(Mock.Of<IDevice>(), 10, 0, loggerMock.Object);

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(0, 0, null);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
