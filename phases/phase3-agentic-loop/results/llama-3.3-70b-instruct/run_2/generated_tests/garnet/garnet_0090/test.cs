using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Garnet.cluster
{
    public class TestMigrateOperation : MigrateOperation
    {
        public TestMigrateOperation(MigrateSession session, Sketch sketch = null, int batchSize = 1 << 18) 
            : base(session, sketch, batchSize)
        {
        }

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }
    }

    public class MigrateOperationTests
    {
        [Fact]
        public async Task LogWarning_IsCalled_WhenTransmitSlotsFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession();
            var sketch = new Sketch();
            var migrateOperation = new TestMigrateOperation(migrateSession, sketch);
            migrateOperation.SetLogger(loggerMock.Object);

            // Act
            var result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
