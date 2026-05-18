using Bit.Core.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
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
        public async Task UpdatePhishingDomainsAsync_LogsDebugMessage_WhenCacheUpdateSucceeds()
        {
            // Arrange
            var domains = new List<string> { "domain1", "domain2" };
            var checksum = "checksum";
            _storageServiceMock.Setup(s => s.UpdateDomainsAsync(domains, checksum)).ReturnsAsync(Task.CompletedTask);
            _cacheMock.Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>())).ReturnsAsync(true);

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.Is<string>(s => s.Contains("Updated phishing domains cache after update operation"))), Times.Once);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_LogsWarningMessage_WhenCacheUpdateFails()
        {
            // Arrange
            var domains = new List<string> { "domain1", "domain2" };
            var checksum = "checksum";
            _storageServiceMock.Setup(s => s.UpdateDomainsAsync(domains, checksum)).ReturnsAsync(Task.CompletedTask);
            _cacheMock.Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>())).Throws(new Exception("Cache update failed"));

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Failed to update phishing domains in cache"))), Times.Once);
        }
    }
}
