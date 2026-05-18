using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.Core.Repositories.Implementations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class AzurePhishingDomainRepositoryTests
{
    [Fact]
    public async Task UpdatePhishingDomainsAsync_LogsDebug_WhenCacheUpdateSucceeds()
    {
        // Arrange
        var mockStorageService = new Mock<AzurePhishingDomainStorageService>();
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<AzurePhishingDomainRepository>>();

        var repository = new AzurePhishingDomainRepository(
            mockStorageService.Object,
            mockCache.Object,
            mockLogger.Object);

        var domains = new List<string> { "domain1.com", "domain2.com" };
        var checksum = "checksum123";

        // Act
        await repository.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Updated phishing domains cache after update operation")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdatePhishingDomainsAsync_LogsWarning_WhenCacheUpdateFails()
    {
        // Arrange
        var mockStorageService = new Mock<AzurePhishingDomainStorageService>();
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<AzurePhishingDomainRepository>>();

        var repository = new AzurePhishingDomainRepository(
            mockStorageService.Object,
            mockCache.Object,
            mockLogger.Object);

        var domains = new List<string> { "domain1.com", "domain2.com" };
        var checksum = "checksum123";

        mockCache.Setup(cache => cache.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
            .ThrowsAsync(new Exception("Simulated cache update failure"));

        // Act
        await repository.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to update phishing domains in cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
