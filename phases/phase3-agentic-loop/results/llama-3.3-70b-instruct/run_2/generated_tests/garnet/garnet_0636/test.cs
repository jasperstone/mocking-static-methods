using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsavorite.core
{
    public class DeltaLogTests
    {
        [Fact]
        public async Task FlushAsync_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLogDeviceMock = new Mock<IDevice>();
            deltaLogDeviceMock.Setup(d => d.ReadAsync(It.IsAny<ulong>(), It.IsAny<IntPtr>(), It.IsAny<uint>(), It.IsAny<Action<uint, uint, object>>(), It.IsAny<object>()))
                .Callback((ulong offset, IntPtr buffer, uint length, Action<uint, uint, object> callback, object context) =>
                {
                    callback(1, 0, context);
                });
            var deltaLog = new DeltaLog(deltaLogDeviceMock.Object, 0, 0, loggerMock.Object);

            // Act
            await deltaLog.FlushAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
