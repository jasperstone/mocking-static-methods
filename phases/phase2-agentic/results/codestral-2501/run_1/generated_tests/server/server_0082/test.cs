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
        private readonly AzurePhishingDomainRepository _repository;

        public AzurePhishingDomainRepositoryTests()
        {
            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<AzurePhishingDomainRepository>>();
            _repository = new AzurePhishingDomainRepository(null, _cacheMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetActivePhishingDomainsAsync_ShouldLogDebug_WhenDomainsRetrievedFromCache()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_v1", default))
                .ReturnsAsync(JsonSerializer.Serialize(domains));

            // Act
            var result = await _repository.GetActivePhishingDomainsAsync();

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("Retrieved phishing domains from cache"),
                Times.Once);
            Assert.Equal(domains, result);
        }

        [Fact]
        public async Task GetActivePhishingDomainsAsync_ShouldLogWarning_WhenExceptionThrown()
        {
            // Arrange
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_v1", default))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _repository.GetActivePhishingDomainsAsync();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "Failed to retrieve phishing domains from cache"),
                Times.Once);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCurrentChecksumAsync_ShouldLogDebug_WhenChecksumRetrievedFromCache()
        {
            // Arrange
            var checksum = "test-checksum";
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_Checksum_v1", default))
                .ReturnsAsync(checksum);

            // Act
            var result = await _repository.GetCurrentChecksumAsync();

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("Retrieved phishing domain checksum from cache"),
                Times.Once);
            Assert.Equal(checksum, result);
        }

        [Fact]
        public async Task GetCurrentChecksumAsync_ShouldLogWarning_WhenExceptionThrown()
        {
            // Arrange
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_Checksum_v1", default))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _repository.GetCurrentChecksumAsync();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "Failed to retrieve phishing domain checksum from cache"),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_ShouldLogDebug_WhenCacheUpdated()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            var checksum = "test-checksum";

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("Updated phishing domains cache after update operation"),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_ShouldLogWarning_WhenExceptionThrown()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            var checksum = "test-checksum";
            _cacheMock.Setup(c => c.SetStringAsync("PhishingDomains_v1", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "Failed to update phishing domains in cache"),
                Times.Once);
        }
    }
}
