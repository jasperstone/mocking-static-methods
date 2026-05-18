using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task LogWarning_ShouldBeCalled_WhenTransmitSlotsFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var sketchMock = new Mock<Sketch>();
            var migrateOperation = new Mock<MigrateOperation>(null, sketchMock.Object);

            // Set up the sketch to have some keys
            var argSliceVector = new List<ArgSlice>
            {
                new ArgSlice(new SpanByte(new byte[1], IntPtr.Zero), false),
                new ArgSlice(new SpanByte(new byte[1], IntPtr.Zero), false)
            };
            sketchMock.Setup(s => s.argSliceVector).Returns(argSliceVector);

            // Mock the TransmitSlotsAsync method to return false
            migrateOperation.Setup(m => m.TransmitSlotsAsync(It.IsAny<StoreType>()))
                .ReturnsAsync(false);

            // Act
            bool result = await migrateOperation.Object.RunMigrationAsync(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("TransmitSlots failed")),
                    It.IsAny<long>(), // cursor
                    It.IsAny<long>(), // current
                    It.Is<int>(count => count == argSliceVector.Count)
                ),
                Times.Once
            );

            Assert.False(result);
        }
    }
}
