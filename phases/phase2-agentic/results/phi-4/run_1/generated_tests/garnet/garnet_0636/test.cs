using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tsavorite.core.Tests
{
    public class DeltaLogTests
    {
        [Fact]
        public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(
                new Mock<IDevice>().Object,
                12, // Example log page size bits
                0,
                loggerMock.Object
            );

            var context = new PageAsyncFlushResult<Empty>();
            context.count = 1; // Ensure the count is decremented to zero

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(1, 0, context); // Non-zero error code

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("AsyncFlushPageToDeviceCallback error:")),
                    It.Is<uint>(errorCode => errorCode == 1)
                ),
                Times.Once
            );
        }
    }
}
