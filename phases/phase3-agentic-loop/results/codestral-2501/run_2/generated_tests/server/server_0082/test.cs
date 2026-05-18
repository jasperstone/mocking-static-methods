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
        public async Task GetActivePhishingDomainsAsync_ShouldLogDebug_WhenCacheHit()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            var cachedDomains = JsonSerializer.Serialize(domains);
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_v1", default)).ReturnsAsync(cachedDomains);

            // Act
            var result = await _repository.GetActivePhishingDomainsAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrieved phishing domains from cache")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Equal(domains, result);
        }

        [Fact]
        public async Task GetActivePhishingDomainsAsync_ShouldLogWarning_WhenCacheMiss()
        {
            // Arrange
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_v1", default)).ReturnsAsync((string)null);

            // Act
            var result = await _repository.GetActivePhishingDomainsAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to retrieve phishing domains from cache")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task GetCurrentChecksumAsync_ShouldLogDebug_WhenCacheHit()
        {
            // Arrange
            var checksum = "checksum123";
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_Checksum_v1", default)).ReturnsAsync(checksum);

            // Act
            var result = await _repository.GetCurrentChecksumAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrieved phishing domain checksum from cache")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Equal(checksum, result);
        }

        [Fact]
        public async Task GetCurrentChecksumAsync_ShouldLogWarning_WhenCacheMiss()
        {
            // Arrange
            _cacheMock.Setup(c => c.GetStringAsync("PhishingDomains_Checksum_v1", default)).ReturnsAsync((string)null);

            // Act
            var result = await _repository.GetCurrentChecksumAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to retrieve phishing domain checksum from cache")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_ShouldLogDebug_WhenUpdateSucceeds()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            var checksum = "checksum123";

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Updated phishing domains cache after update operation")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_ShouldLogWarning_WhenUpdateFails()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            var checksum = "checksum123";
            _cacheMock.Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default))
                .ThrowsAsync(new Exception("Simulated cache update failure"));

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to update phishing domains in cache")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
