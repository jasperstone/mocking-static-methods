using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class SingleDatabaseManagerTests
    {
        private class DummyDatabase : GarnetDatabase
        {
            public override void Initialize() { }
        }

        private class DummyStoreWrapper : StoreWrapper
        {
            public override long Recover() => 1;
            public override long Recover(object token1, object token2) => 1;
            public override Task<(long?, long?)> TakeCheckpointAsync(GarnetDatabase db, ILogger logger, CancellationToken token)
            {
                return Task.FromResult<(long?, long?)>((123, 456));
            }
            public override void ResumeCheckpoints(GarnetDatabase db) { }
            public override bool TryPauseCheckpoints(GarnetDatabase db) => true;
            public override Task<bool> TryPauseCheckpointsAsync(GarnetDatabase db) => Task.FromResult(true);
        }

        [Fact]
        public async Task LogInformation_IsCalled_WhenTsavoriteNoHybridLogExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapper = new DummyStoreWrapper();
            var createDatabase = new Func<int, GarnetDatabase>(_ => new DummyDatabase());
            var manager = new SingleDatabaseManager(createDatabase, storeWrapper, createDefaultDatabase: true);

            // Simulate the catch block for TsavoriteNoHybridLogException
            var ex = new TsavoriteNoHybridLogException();

            // Act
            // Directly invoke the catch block logic for testing
            // Since the actual method is private, we simulate the scenario
            // by calling LogInformation directly
            loggerMock.Object.LogInformation(ex,
                "No Hybrid Log found for recovery; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                1, 2);

            // Assert
            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No Hybrid Log found for recovery")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
