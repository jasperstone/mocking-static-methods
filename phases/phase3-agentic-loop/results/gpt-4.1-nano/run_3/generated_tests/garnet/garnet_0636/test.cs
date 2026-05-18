using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class DeltaLogTests
    {
        [Fact]
        public async Task AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockDevice = new Mock<IDevice>();
            var deltaLog = new DeltaLog(mockDevice.Object, 10, 0, mockLogger.Object);

            // Use reflection to access the private method
            var methodInfo = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Prepare context object
            var context = new PageAsyncFlushResult<Empty>
            {
                count = 1,
                Free = () => { },
                handle = new CountdownEvent(1)
            };

            // Act
            // Call with errorCode != 0 to trigger LogError
            methodInfo.Invoke(deltaLog, new object[] { 1u, 0u, context });

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.Is<string>(s => s.Contains("AsyncFlushPageToDeviceCallback error")), It.IsAny<uint>()),
                Times.Once);
        }
    }
}
