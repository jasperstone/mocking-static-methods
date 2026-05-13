using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.Core.Repositories.Implementations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Core.Tests.Repositories.Implementations
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
        public async Task UpdatePhishingDomainsAsync_LogsDebug_WhenCacheUpdatedSuccessfully()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            var checksum = "checksum123";

            _cacheMock.Setup(cache => cache.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                      .Returns(Task.CompletedTask);

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(logger => logger.LogDebug("Updated phishing domains cache after update operation"), Times.Once);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_LogsWarning_WhenCacheUpdateFails()
        {
            // Arrange
            var domains = new List<string> { "domain1.com", "domain2.com" };
            var checksum = "checksum123";

            _cacheMock.Setup(cache => cache.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                      .ThrowsAsync(new Exception("Cache update failed"));

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning(It.IsAny<Exception>(), "Failed to update phishing domains in cache"), Times.Once);
        }
    }
}
