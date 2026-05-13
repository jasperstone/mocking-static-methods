using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace VectorData.Chroma.Tests
{
    public class ChromaMemoryStoreTests
    {
        [Fact]
        public async Task CreateCollectionAsync_ValidCollectionName_CreatesCollection()
        {
            // Arrange
            var chromaClientMock = new Mock<IChromaClient>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<ChromaMemoryStore>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerFactoryMock.Object);

            // Act
            await chromaMemoryStore.CreateCollectionAsync("TestCollection");

            // Assert
            chromaClientMock.Verify(x => x.CreateCollectionAsync("TestCollection", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCollectionAsync_ValidCollectionName_DeletesCollection()
        {
            // Arrange
            var chromaClientMock = new Mock<IChromaClient>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<ChromaMemoryStore>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerFactoryMock.Object);

            // Act
            await chromaMemoryStore.DeleteCollectionAsync("TestCollection");

            // Assert
            chromaClientMock.Verify(x => x.DeleteCollectionAsync("TestCollection", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCollectionAsync_NonExistentCollection_ThrowsKernelException()
        {
            // Arrange
            var chromaClientMock = new Mock<IChromaClient>();
            chromaClientMock.Setup(x => x.DeleteCollectionAsync("TestCollection", It.IsAny<CancellationToken>())).Throws(new HttpOperationException("Test error"));
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<ChromaMemoryStore>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerFactoryMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<KernelException>(() => chromaMemoryStore.DeleteCollectionAsync("TestCollection"));
            loggerMock.Verify(x => x.LogError("Cannot delete non-existent collection {0}", "TestCollection"), Times.Once);
        }

        [Fact]
        public async Task DoesCollectionExistAsync_ValidCollectionName_ReturnsTrue()
        {
            // Arrange
            var chromaClientMock = new Mock<IChromaClient>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<ChromaMemoryStore>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerFactoryMock.Object);

            // Act
            var result = await chromaMemoryStore.DoesCollectionExistAsync("TestCollection");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetAsync_ValidCollectionNameAndKey_ReturnsMemoryRecord()
        {
            // Arrange
            var chromaClientMock = new Mock<IChromaClient>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<ChromaMemoryStore>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerFactoryMock.Object);

            // Act
            var result = await chromaMemoryStore.GetAsync("TestCollection", "TestKey");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetBatchAsync_ValidCollectionNameAndKeys_ReturnsMemoryRecords()
        {
            // Arrange
            var chromaClientMock = new Mock<IChromaClient>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<ChromaMemoryStore>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var chromaMemoryStore = new ChromaMemoryStore(chromaClientMock.Object, loggerFactoryMock.Object);

            // Act
            var result = await chromaMemoryStore.GetBatchAsync("TestCollection", new[] { "TestKey1", "TestKey2" }).ToListAsync();

            // Assert
            Assert.NotNull(result);
        }
    }
}
