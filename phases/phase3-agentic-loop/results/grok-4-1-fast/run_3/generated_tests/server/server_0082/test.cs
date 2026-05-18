using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Bit.Core.Repositories.Tests.Implementations;

public class AzurePhishingDomainRepositoryTests
{
    private readonly Mock<AzurePhishingDomainStorageService> _mockStorageService;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<AzurePhishingDomainRepository>> _mockLogger;
    private readonly AzurePhishingDomainRepository _repository;

    public AzurePhishingDomainRepositoryTests()
    {
        _mockStorageService = new Mock<AzurePhishingDomainStorageService>();
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<AzurePhishingDomainRepository>>();

        _mockStorageService.Setup(x => x.UpdateDomainsAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
                          .Returns(Task.CompletedTask);
        _mockCache.Setup(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                  .Returns(Task.CompletedTask);

        _repository = new AzurePhishingDomainRepository(
            _mockStorageService.Object,
            _mockCache.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task UpdatePhishingDomainsAsync_SuccessfulCacheUpdate_LogsDebugMessage()
    {
        // Arrange
        var domains = new List<string> { "phishing1.com", "phishing2.com" };
        var checksum = "checksum123";

        // Act
        await _repository.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert - Verifies the LogDebug extension call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Updated phishing domains cache after update operation"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdatePhishingDomainsAsync_CacheFailure_LogsWarning()
    {
        // Arrange
        var domains = new List<string> { "phishing1.com" };
        var checksum = "checksum123";
        var cacheException = new InvalidOperationException("Cache failure");
        
        _mockCache.SetupSequence(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                  .Returns(Task.CompletedTask)
                  .ThrowsAsync(cacheException);

        // Act
        await _repository.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Failed to update phishing domains in cache"),
                cacheException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
