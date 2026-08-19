using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Garnet.server;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerTests
    {
        private class TestGarnetDatabase : GarnetDatabase
        {
            public TestGarnetDatabase(int id) : base(id, null, null, null, null, null, null, null, null, null, false, false, null)
            {
            }

            public override void Dispose()
            {
                // No-op for testing
            }
        }

        private class TestMultiDatabaseManager : MultiDatabaseManager
        {
            private readonly TestGarnetDatabase _testDb;
            public ILogger Logger { get; set; }

            public TestMultiDatabaseManager(Func<int, GarnetDatabase> createDatabaseDelegate, StoreWrapper storeWrapper)
                : base(createDatabaseDelegate, storeWrapper)
            {
                _testDb = new TestGarnetDatabase(0);
            }

            protected override GarnetDatabase TryGetOrAddDatabase(int dbId, out bool success, out string error)
            {
                success = true;
                error = null;
                return _testDb;
            }

            public override void RecoverDatabaseCheckpoint(GarnetDatabase db, out long storeVersion, out long objectStoreVersion)
            {
                throw new Exception("Recovery failure");
            }
        }

        [Fact]
        public async Task RecoverCheckpoint_LogsInformationOnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new StoreWrapper
            {
                loggerFactory = new LoggerFactory(),
                serverOptions = new ServerOptions
                {
                    MaxDatabases = 10,
                    FailOnRecoveryError = false,
                    MainStoreCheckpointBaseDirectory = "/tmp",
                    GetCheckpointDirectoryName = (id) => $"checkpoint_{id}"
                }
            };

            var manager = new TestMultiDatabaseManager((id) => null, storeWrapper);
            manager.Logger = mockLogger.Object;

            // Act
            await manager.RecoverCheckpointAsync();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during recovery of store")),
                    It.Is<Exception>(ex => ex.Message == "Recovery failure"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
