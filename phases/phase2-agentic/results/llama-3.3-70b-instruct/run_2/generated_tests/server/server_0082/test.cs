using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Core.Repositories.Tests
{
    public class AzurePhishingDomainRepositoryTests
    {
        private readonly Mock<ILogger<AzurePhishingDomainRepository>> _loggerMock;
        private readonly Mock<AzurePhishingDomainStorageService> _storageServiceMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly AzurePhishingDomainRepository _repository;

        public AzurePhishingDomainRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<AzurePhishingDomainRepository>>();
            _storageServiceMock = new Mock<AzurePhishingDomainStorageService>();
            _cacheMock = new Mock<IDistributedCache>();
            _repository = new AzurePhishingDomainRepository(_storageServiceMock.Object, _cacheMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_LogsDebugMessage_WhenCacheUpdateSucceeds()
        {
            // Arrange
            var domains = new List<string> { "example.com" };
            var checksum = "checksum";
            _storageServiceMock.Setup(s => s.UpdateDomainsAsync(domains, checksum)).ReturnsAsync(Task.CompletedTask);
            _cacheMock.Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>())).ReturnsAsync(true);

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(l => l.LogDebug("Updated phishing domains cache after update operation"), Times.Once);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_LogsWarningMessage_WhenCacheUpdateFails()
        {
            // Arrange
            var domains = new List<string> { "example.com" };
            var checksum = "checksum";
            _storageServiceMock.Setup(s => s.UpdateDomainsAsync(domains, checksum)).ReturnsAsync(Task.CompletedTask);
            _cacheMock.Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>())).Throws(new Exception("Cache update failed"));

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), "Failed to update phishing domains in cache"), Times.Once);
        }
    }
}
