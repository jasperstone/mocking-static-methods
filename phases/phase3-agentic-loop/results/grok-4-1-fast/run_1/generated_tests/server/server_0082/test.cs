using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Bit.Core.PhishingDomainFeatures;
using Bit.Core.Repositories;
using Bit.Core.Repositories.Implementations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Bit.Core.Repositories.Implementations.Tests;

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

        _mockStorageService.Setup(x => x.UpdateDomainsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()))
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
        var domains = new List<string> { "example1.com", "example2.com" };
        var checksum = "abc123checksum";

        _mockCache.SetupSequence(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                  .Returns(Task.CompletedTask)
                  .Returns(Task.CompletedTask);

        // Act
        await _repository.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Updated phishing domains cache after update operation"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdatePhishingDomainsAsync_CacheFailure_LogsWarning()
    {
        // Arrange
        var domains = new List<string> { "example1.com" };
        var checksum = "checksum123";
        var exception = new InvalidOperationException("Cache failure");

        _mockCache.SetupSequence(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                  .ThrowsAsync(exception);

        // Act
        await _repository.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Failed to update phishing domains in cache"),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
