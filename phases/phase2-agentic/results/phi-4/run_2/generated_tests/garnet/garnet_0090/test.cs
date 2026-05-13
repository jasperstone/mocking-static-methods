using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task TransmitSlotsAsync_LogsWarningOnFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateOperation = new MigrateOperation(null)
            {
                sketch = new Sketch { argSliceVector = new ArgSliceVector { Count = 5 } }
            };

            // Simulate the failure of TransmitSlotsAsync
            migrateOperation.TransmitSlotsAsync = (storeType) => Task.FromResult(false);

            // Act
            bool result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("TransmitSlots failed")),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.Is<int>(count => count == 5)),
                Times.Once);
        }
    }
}
