using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Bit.Core.Repositories.Implementations;

namespace Bit.Tests
{
    public class AzurePhishingDomainRepositoryTests
    {
        private readonly Mock<AzurePhishingDomainStorageService> _storageServiceMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<AzurePhishingDomainRepository>> _loggerMock;
        private readonly AzurePhishingDomainRepository _repository;

        public AzurePhishingDomainRepositoryTests()
        {
            _storageServiceMock = new Mock<AzurePhishingDomainStorageService>();
            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<AzurePhishingDomainRepository>>();
            _repository = new AzurePhishingDomainRepository(
                _storageServiceMock.Object,
                _cacheMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetCurrentChecksumAsync_ShouldLogDebug_WhenCacheHit()
        {
            // Arrange
            var checksum = "abc123";
            _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(checksum);

            // Act
            var result = await _repository.GetCurrentChecksumAsync();

            // Assert
            Assert.Equal(checksum, result);
            _loggerMock.Verify(
                x => x.LogDebug("Retrieved phishing domain checksum from cache"),
                Times.Once);
        }

        [Fact]
        public async Task GetCurrentChecksumAsync_ShouldLogWarning_WhenCacheThrows()
        {
            // Arrange
            _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            // Act
            await _repository.GetCurrentChecksumAsync();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "Failed to retrieve phishing domain checksum from cache"),
                Times.Once);
        }

        [Fact]
        public async Task GetCurrentChecksumAsync_ShouldLogDebug_WhenCacheMissAndStorageReturnsChecksum()
        {
            // Arrange
            _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync((string)null);
            var checksum = "xyz789";
            _storageServiceMock.Setup(s => s.GetChecksumAsync())
                .ReturnsAsync(checksum);
            _cacheMock.Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _repository.GetCurrentChecksumAsync();

            // Assert
            Assert.Equal(checksum, result);
            _loggerMock.Verify(
                x => x.LogDebug("Stored phishing domain checksum in cache"),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_ShouldLogDebug()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            var checksum = "checksum123";

            _cacheMock.Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("Updated phishing domains cache after update operation"),
                Times.Once);
        }
    }
}
