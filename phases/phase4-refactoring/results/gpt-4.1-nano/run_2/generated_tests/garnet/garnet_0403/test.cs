using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using Garnet;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerTests
    {
        private class TestDatabase : GarnetDatabase
        {
            public override void Initialize() { }
            public override object ObjectStore => null;
            public override VectorManager VectorManager => new VectorManager();
        }

        private class TestMultiDatabaseManager : MultiDatabaseManager
        {
            public Mock<ILogger<MultiDatabaseManager>> LoggerMock { get; }
            public override bool TryGetOrAddDatabase(int dbId, out GarnetDatabase db, out bool success)
            {
                db = new TestDatabase();
                success = true;
                return true;
            }
            public override void RecoverDatabaseCheckpoint(GarnetDatabase db, out long storeVersion, out long objectStoreVersion)
            {
                storeVersion = 1;
                objectStoreVersion = 2;
                throw new Exception("Simulated exception");
            }

            public TestMultiDatabaseManager(ILogger<MultiDatabaseManager> logger) : base((id) => new TestDatabase(), new StoreWrapper())
            {
                LoggerMock = new Mock<ILogger<MultiDatabaseManager>>();
                Logger = LoggerMock.Object;
            }
        }

        [Fact]
        public async Task RecoverCheckpoint_LogsException_WhenRecoverDatabaseCheckpointThrows()
        {
            var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
            var manager = new TestMultiDatabaseManager(loggerMock.Object);

            // Setup to trigger the exception branch
            manager.TryGetSavedDatabaseIds = (dir, name, out int[] ids) =>
            {
                ids = new[] { 0 };
                return true;
            };

            // Call RecoverCheckpoint, which should log the exception
            await manager.RecoverCheckpoint();

            // Verify that LogInformation was called with the exception
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during recovery of store")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
