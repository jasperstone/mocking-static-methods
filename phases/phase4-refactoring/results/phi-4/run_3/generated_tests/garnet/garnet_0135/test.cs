using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        public class TestableMigrationDriver : MigrationDriver
        {
            public TestableMigrationDriver(ILogger logger) : base(logger) { }

            public override bool RelinquishOwnership()
            {
                return base.RelinquishOwnership();
            }
        }

        [Fact]
        public async Task LogErrorCalledWhenRelinquishOwnershipFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new TestableMigrationDriver(loggerMock.Object);

            // Override RelinquishOwnership to simulate failure
            var relinquishOwnershipMethod = typeof(TestableMigrationDriver).GetMethod("RelinquishOwnership", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            relinquishOwnershipMethod?.SetValue(migrationDriver, () => false);

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Failed to relinquish ownership from source node")),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }
    }
}
