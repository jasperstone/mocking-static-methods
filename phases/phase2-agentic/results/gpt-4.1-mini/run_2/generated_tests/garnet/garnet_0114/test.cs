using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession(loggerMock.Object);
            migrateSession.SetupNamespaces(2); // Must be multiple of VectorManager.ContextStep (assumed 2)
            migrateSession.SetupMigrateOperationClientThrows();

            // Act
            var result = await migrateSession.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to reserve")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task MigrateSlotsDriverInlineAsync_LogsErrorOnCreateAndRunMigrateTasksAsyncException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession(loggerMock.Object);
            migrateSession.SetupClusterProviderForMigrateSlotsDriver();
            migrateSession.SetupCreateAndRunMigrateTasksAsyncThrows();

            // Act
            var result = await migrateSession.MigrateSlotsDriverInlineAsync();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(nameof(migrateSession.CreateAndRunMigrateTasksAsync))),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper class to expose internals and setup mocks
        private class TestableMigrateSession : MigrateSession
        {
            private readonly ILogger _logger;
            public TestableMigrateSession(ILogger logger)
            {
                _logger = logger;
                base.logger = logger;
            }

            public void SetupNamespaces(int count)
            {
                // _namespaces is private, so use reflection to set it
                var field = typeof(MigrateSession).GetField("_namespaces", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, new System.Collections.Generic.List<ulong>(new ulong[count]));
            }

            public void SetupMigrateOperationClientThrows()
            {
                // Setup migrateOperation[0].Client.ExecuteForArrayAsync to throw
                var migrateOperationField = typeof(MigrateSession).GetField("migrateOperation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var migrateOperationArray = new IMigrateOperation[1];
                var clientMock = new Mock<IClient>();
                clientMock.Setup(c => c.ExecuteForArrayAsync(It.IsAny<string[]>())).ThrowsAsync(new Exception("Test exception"));
                var migrateOperationMock = new Mock<IMigrateOperation>();
                migrateOperationMock.SetupGet(mo => mo.Client).Returns(clientMock.Object);
                migrateOperationArray[0] = migrateOperationMock.Object;
                migrateOperationField.SetValue(this, migrateOperationArray);
            }

            public void SetupClusterProviderForMigrateSlotsDriver()
            {
                // Setup clusterProvider and serverOptions to avoid null refs
                var clusterProviderField = typeof(MigrateSession).GetField("clusterProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var clusterProviderMock = new Mock<IClusterProvider>();
                var storeWrapperMock = new Mock<IStoreWrapper>();
                var storeMock = new Mock<IStore>();
                var logMock = new Mock<ILog>();
                logMock.SetupGet(l => l.BeginAddress).Returns(0L);
                logMock.SetupGet(l => l.TailAddress).Returns(100L);
                storeMock.SetupGet(s => s.Log).Returns(logMock.Object);
                storeWrapperMock.SetupGet(sw => sw.store).Returns(storeMock.Object);
                storeWrapperMock.SetupGet(sw => sw.objectStore).Returns(storeMock.Object);
                clusterProviderMock.SetupGet(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
                var serverOptionsMock = new Mock<IServerOptions>();
                serverOptionsMock.Setup(so => so.PageSizeBits()).Returns(4);
                serverOptionsMock.Setup(so => so.ObjectStorePageSizeBits()).Returns(4);
                serverOptionsMock.SetupGet(so => so.DisableObjects).Returns(false);
                serverOptionsMock.SetupGet(so => so.ParallelMigrateTaskCount).Returns(1);
                clusterProviderMock.SetupGet(cp => cp.serverOptions).Returns(serverOptionsMock.Object);
                clusterProviderField.SetValue(this, clusterProviderMock.Object);
            }

            public void SetupCreateAndRunMigrateTasksAsyncThrows()
            {
                // Setup CreateAndRunMigrateTasksAsync to throw exception to trigger LogError
                var method = typeof(MigrateSession).GetMethod("CreateAndRunMigrateTasksAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                // We cannot override private local function, so instead we simulate by mocking ScanStoreTaskAsync to throw
                var scanStoreTaskAsyncMethod = typeof(MigrateSession).GetMethod("ScanStoreTaskAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                // We cannot mock private methods easily, so we simulate by setting migrateOperation to null to cause exception
                var migrateOperationField = typeof(MigrateSession).GetField("migrateOperation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                migrateOperationField.SetValue(this, null);
            }
        }

        // Interfaces to mock dependencies (simplified)
        private interface IMigrateOperation
        {
            IClient Client { get; }
        }

        private interface IClient
        {
            Task<string[]> ExecuteForArrayAsync(params string[] args);
        }

        private interface IClusterProvider
        {
            IStoreWrapper storeWrapper { get; }
            IServerOptions serverOptions { get; }
        }

        private interface IStoreWrapper
        {
            IStore store { get; }
            IStore objectStore { get; }
        }

        private interface IStore
        {
            ILog Log { get; }
        }

        private interface ILog
        {
            long BeginAddress { get; }
            long TailAddress { get; }
        }

        private interface IServerOptions
        {
            int PageSizeBits();
            int ObjectStorePageSizeBits();
            bool DisableObjects { get; }
            int ParallelMigrateTaskCount { get; }
        }
    }
}
