using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster
{
    internal sealed partial class MigrateSession
    {
        // Test helper constructor to inject dependencies for testing
        internal MigrateSession(ILogger logger, IMigrateOperation[] migrateOperation, List<ulong> namespaces, ulong targetNodeId)
        {
            this.logger = logger;
            this.migrateOperation = migrateOperation;
            this._namespaces = namespaces;
            this._targetNodeId = targetNodeId;
        }
    }
}

namespace Garnet.Tests.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<IMigrateOperationClient>();
            clientMock.Setup(c => c.ExecuteForArrayAsync(It.IsAny<string[]>())).ThrowsAsync(new InvalidOperationException("Test exception"));

            var migrateOperationMock = new Mock<IMigrateOperation>();
            migrateOperationMock.SetupGet(mo => mo.Client).Returns(clientMock.Object);

            var namespaces = new List<ulong> { 0, 1, 2, 3 }; // count divisible by VectorManager.ContextStep (2)
            ulong targetNodeId = 1234;

            var migrateSession = new MigrateSession(loggerMock.Object, new IMigrateOperation[] { migrateOperationMock.Object }, namespaces, targetNodeId);

            // Act
            var result = await migrateSession.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to reserve")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Interfaces to match internal dependencies for mocking
    internal interface IMigrateOperationClient
    {
        Task<string[]> ExecuteForArrayAsync(params string[] args);
    }

    internal interface IMigrateOperation
    {
        IMigrateOperationClient Client { get; }
    }

    internal static class VectorManager
    {
        public const int ContextStep = 2;
    }
}
