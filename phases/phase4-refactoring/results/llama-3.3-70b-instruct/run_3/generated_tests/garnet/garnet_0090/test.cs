using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class MigrateOperationTests
    {
        [Fact]
        public void LogWarning_CalledWithCorrectArguments()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(
                new ClusterSession(),
                new ClusterProvider(),
                "targetAddress",
                1234,
                "targetNodeId",
                "username",
                "password",
                "sourceNodeId",
                true,
                false,
                1000,
                new HashSet<int> { 1, 2, 3 },
                new Sketch(),
                TransferOption.SLOTS
            );
            var migrateOperation = new MigrateOperation(migrateSession);
            migrateOperation.logger = loggerMock.Object;

            // Act
            migrateOperation.TransmitSlotsAsync(StoreType.Main).Wait();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
