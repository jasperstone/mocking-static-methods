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
            var mockLogger = new Mock<ILogger>();
            var mockSession = new Mock<MigrateSession>();
            var migrateOperation = new MigrateOperation(mockSession.Object)
            {
                sketch = new Sketch { argSliceVector = new ArgSliceVector { Count = 5 } }
            };

            // Act
            bool result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TransmitSlots failed for")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result);
        }
    }
}
