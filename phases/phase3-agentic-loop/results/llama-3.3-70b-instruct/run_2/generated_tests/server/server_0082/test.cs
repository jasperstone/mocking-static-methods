using Bit.Core.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Core.Repositories.Implementations.Tests
{
    public class AzurePhishingDomainRepositoryTests
    {
        private readonly Mock<IAzurePhishingDomainStorageService> _storageServiceMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<AzurePhishingDomainRepository>> _loggerMock;
        private readonly AzurePhishingDomainRepository _repository;

        public AzurePhishingDomainRepositoryTests()
        {
            _storageServiceMock = new Mock<IAzurePhishingDomainStorageService>();
            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<AzurePhishingDomainRepository>>();
            _repository = new AzurePhishingDomainRepository(_storageServiceMock.Object, _cacheMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_LogsDebugMessage()
        {
            // Arrange
            var domains = new List<string> { "domain1", "domain2" };
            var checksum = "checksum";

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(logger => logger.LogDebug("Updated phishing domains cache after update operation"), Times.Once);
        }

        [Fact]
        public async Task GetCurrentChecksumAsync_LogsDebugMessage()
        {
            // Arrange
            var checksum = "checksum";
            _cacheMock.Setup(cache => cache.GetStringAsync(It.IsAny<string>())).ReturnsAsync(checksum);

            // Act
            await _repository.GetCurrentChecksumAsync();

            // Assert
            _loggerMock.Verify(logger => logger.LogDebug("Retrieved phishing domain checksum from cache"), Times.Once);
        }

        [Fact]
        public async Task GetActivePhishingDomainsAsync_LogsDebugMessage()
        {
            // Arrange
            var domains = new List<string> { "domain1", "domain2" };
            var cachedDomains = JsonSerializer.Serialize(domains);
            _cacheMock.Setup(cache => cache.GetStringAsync(It.IsAny<string>())).ReturnsAsync(cachedDomains);

            // Act
            await _repository.GetActivePhishingDomainsAsync();

            // Assert
            _loggerMock.Verify(logger => logger.LogDebug("Retrieved phishing domains from cache"), Times.Once);
        }
    }
}
