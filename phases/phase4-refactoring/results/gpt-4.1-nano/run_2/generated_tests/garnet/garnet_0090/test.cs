using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrateOperationLoggingTests
    {
        [Fact]
        public async Task MigrateOperation_Should_LogWarning_ForScanRange()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession.MigrateOperation>>();
            var sessionMock = new Mock<MigrateSession>();
            var gcsMock = new Mock<GarnetClientSession>();
            var localSessionMock = new Mock<LocalServerSession>();

            // Setup session mock
            sessionMock.Setup(s => s.GetGarnetClient()).Returns(gcsMock.Object);
            sessionMock.Setup(s => s.GetLocalSession()).Returns(localSessionMock.Object);
            sessionMock.Setup(s => s.CheckConnectionAsync(It.IsAny<GarnetClientSession>())).ReturnsAsync(true);

            // Setup gcs mock
            gcsMock.Setup(g => g.InitializeIterationBuffer(It.IsAny<int>()));
            gcsMock.Setup(g => g.SendAndResetIterationBuffer()).ReturnsAsync(true);
            gcsMock.Setup(g => g.Dispose());

            // Setup local session mock
            localSessionMock.Setup(l => l.BasicGarnetApi.IterateMainStore(
                It.IsAny<MainStoreScan>(), It.IsAny<ref long>(), It.IsAny<long>(), It.IsAny<long>(), true))
                .ReturnsAsync();

            localSessionMock.Setup(l => l.BasicGarnetApi.IterateObjectStore(
                It.IsAny<ObjectStoreScan>(), It.IsAny<ref long>(), It.IsAny<long>(), It.IsAny<long>(), true))
                .ReturnsAsync();

            // Instantiate MigrateOperation
            var migrateOp = new MigrateSession.MigrateOperation(sessionMock.Object);
            // Inject the logger
            var migrateType = typeof(MigrateSession.MigrateOperation);
            var loggerField = migrateType.GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(migrateOp, loggerMock.Object);

            // Act
            // Call the method that contains the LogWarning call
            // For demonstration, assume it's called 'RunMigrationAsync' and is public
            var methodInfo = migrateType.GetMethod("RunMigrationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (methodInfo != null)
            {
                await (Task)methodInfo.Invoke(migrateOp, null);
            }

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("<MainStore> migrate keys (namespaces) scan range")), 
                It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }
    }
}
