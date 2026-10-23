using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_Should_LogErrorAndReturnFalse_OnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<IClient>();
            var mockMigrateOperation = new Mock<IMigrateOperation>();
            var mockClientExec = mockClient.As<IClient>();
            mockClientExec.Setup(c => c.ExecuteForArrayAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            var migrateOperation = new[] { mockMigrateOperation.Object };
            var session = new MigrateSession
            {
                logger = mockLogger.Object,
                migrateOperation = migrateOperation,
                _targetNodeId = "node1",
                _namespaces = new System.Collections.Generic.List<ulong> { 0, 1, 2, 3 },
                _namespaceMap = null
            };

            // Act
            var result = await session.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Failed to reserve {count} Vector Set contexts on destination node {node}", It.IsAny<int>(), It.IsAny<ulong>()),
                Times.Once);
        }

        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_Should_ReturnTrue_When_Success()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<IClient>();
            mockClient.Setup(c => c.ExecuteForArrayAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new string[] { "10", "11" });
            var mockClientObj = mockClient.As<IClient>();

            var mockMigrateOperation = new Mock<IMigrateOperation>();
            var migrateOperation = new[] { mockMigrateOperation.Object };
            var session = new MigrateSession
            {
                logger = mockLogger.Object,
                migrateOperation = migrateOperation,
                _targetNodeId = "node1",
                _namespaces = new System.Collections.Generic.List<ulong> { 0, 1, 2, 3 },
                _namespaceMap = null
            };

            // Act
            var result = await session.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.True(result);
            Assert.NotNull(session._namespaceMap);
            mockClient.Verify(c => c.ExecuteForArrayAsync("CLUSTER", "RESERVE", "VECTOR_SET_CONTEXTS", "2"), Times.Once);
        }

        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_Should_LogError_When_ExceptionOccurs()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<IClient>();
            mockClient.Setup(c => c.ExecuteForArrayAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            var mockMigrateOperation = new Mock<IMigrateOperation>();
            var migrateOperation = new[] { mockMigrateOperation.Object };
            var session = new MigrateSession
            {
                logger = mockLogger.Object,
                migrateOperation = migrateOperation,
                _targetNodeId = "node1",
                _namespaces = new System.Collections.Generic.List<ulong> { 0, 1, 2, 3 },
                _namespaceMap = null
            };

            // Act
            var result = await session.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Failed to reserve {count} Vector Set contexts on destination node {node}", It.IsAny<int>(), It.IsAny<ulong>()),
                Times.Once);
        }
    }
}
