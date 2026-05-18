using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Bit.Core.PhishingDomainFeatures;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Core.Repositories.Implementations.Tests
{
    public class AzurePhishingDomainRepositoryTests
    {
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<AzurePhishingDomainRepository>> _loggerMock;
        private readonly Mock<AzurePhishingDomainStorageService> _storageServiceMock;
        private readonly AzurePhishingDomainRepository _repository;

        public AzurePhishingDomainRepositoryTests()
        {
            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<AzurePhishingDomainRepository>>();
            _storageServiceMock = new Mock<AzurePhishingDomainStorageService>();
            _repository = new AzurePhishingDomainRepository(_storageServiceMock.Object, _cacheMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetActivePhishingDomainsAsync_ShouldReturnCachedDomains()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            var cachedDomains = JsonSerializer.Serialize(domains);
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_v1", default)).ReturnsAsync(cachedDomains);

            // Act
            var result = await _repository.GetActivePhishingDomainsAsync();

            // Assert
            Assert.Equal(domains, result);
            _loggerMock.Verify(l => l.LogDebug("Retrieved phishing domains from cache"), Times.Once);
        }

        [Fact]
        public async Task GetActivePhishingDomainsAsync_ShouldReturnStoredDomains_WhenCacheIsEmpty()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_v1", default)).ReturnsAsync((string)null);
            _storageServiceMock.Setup(s => s.GetDomainsAsync()).ReturnsAsync(domains);
            _cacheMock.Setup(c => c.SetStringAsync("PhishingDomains_v1", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default)).Returns(Task.CompletedTask);

            // Act
            var result = await _repository.GetActivePhishingDomainsAsync();

            // Assert
            Assert.Equal(domains, result);
            _loggerMock.Verify(l => l.LogDebug("Stored {Count} phishing domains in cache", domains.Count), Times.Once);
        }

        [Fact]
        public async Task GetCurrentChecksumAsync_ShouldReturnCachedChecksum()
        {
            // Arrange
            var checksum = "checksum123";
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_Checksum_v1", default)).ReturnsAsync(checksum);

            // Act
            var result = await _repository.GetCurrentChecksumAsync();

            // Assert
            Assert.Equal(checksum, result);
            _loggerMock.Verify(l => l.LogDebug("Retrieved phishing domain checksum from cache"), Times.Once);
        }

        [Fact]
        public async Task GetCurrentChecksumAsync_ShouldReturnStoredChecksum_WhenCacheIsEmpty()
        {
            // Arrange
            var checksum = "checksum123";
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_Checksum_v1", default)).ReturnsAsync((string)null);
            _storageServiceMock.Setup(s => s.GetChecksumAsync()).ReturnsAsync(checksum);
            _cacheMock.Setup(c => c.SetStringAsync("PhishingDomains_Checksum_v1", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default)).Returns(Task.CompletedTask);

            // Act
            var result = await _repository.GetCurrentChecksumAsync();

            // Assert
            Assert.Equal(checksum, result);
            _loggerMock.Verify(l => l.LogDebug("Stored phishing domain checksum in cache"), Times.Once);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_ShouldLogDebug_WhenUpdateSucceeds()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            var checksum = "checksum123";
            _cacheMock.Setup(c => c.SetStringAsync("PhishingDomains_v1", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default)).Returns(Task.CompletedTask);
            _cacheMock.Setup(c => c.SetStringAsync("PhishingDomains_Checksum_v1", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default)).Returns(Task.CompletedTask);

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(l => l.LogDebug("Updated phishing domains cache after update operation"), Times.Once);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_ShouldLogWarning_WhenUpdateFails()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            var checksum = "checksum123";
            _cacheMock.Setup(c => c.SetStringAsync("PhishingDomains_v1", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default)).ThrowsAsync(new Exception("Test exception"));
            _cacheMock.Setup(c => c.SetStringAsync("PhishingDomains_Checksum_v1", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default)).ThrowsAsync(new Exception("Test exception"));

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), "Failed to update phishing domains in cache"), Times.Exactly(2));
        }
    }
}
