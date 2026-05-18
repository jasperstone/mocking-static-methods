using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSessionType = Assembly.Load("Garnet").GetType("Garnet.cluster.MigrateSession");
            var migrateSession = Activator.CreateInstance(migrateSessionType, loggerMock.Object);

            // Act
            var method = migrateSessionType.GetMethod("TrySetSlotRangesAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            await (Task<bool>)method.Invoke(migrateSession, new object[] { "nodeid", 3 });

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTraceOnCompletion()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSessionType = Assembly.Load("Garnet").GetType("Garnet.cluster.MigrateSession");
            var migrateSession = Activator.CreateInstance(migrateSessionType, loggerMock.Object);

            // Act
            var method = migrateSessionType.GetMethod("TrySetSlotRangesAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            await (Task<bool>)method.Invoke(migrateSession, new object[] { "nodeid", 3 });

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(2));
        }
    }
}
