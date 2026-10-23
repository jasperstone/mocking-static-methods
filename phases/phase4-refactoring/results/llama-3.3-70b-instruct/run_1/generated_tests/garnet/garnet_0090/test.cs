using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster.Server.Migration; // Assuming the namespace is correct, adjust as needed for your project structure and namespace usage in the code you're testing against. If the namespace is different, adjust accordingly to match your project structure and namespace usage in the code you're testing against. If the namespace is not needed, remove it.

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
                true,
                1000,
                new HashSet<int>(),
                new Sketch(),
                TransferOption.SLOTS
            );
            var migrateOperation = new MigrateOperation(migrateSession, new Sketch());
            migrateOperation.logger = loggerMock.Object;

            // Act
            migrateOperation.TransmitSlotsAsync(StoreType.Main).Wait();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
