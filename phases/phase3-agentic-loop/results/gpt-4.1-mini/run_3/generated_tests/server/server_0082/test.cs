using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core.Repositories.Implementations;

namespace Bit.Core.Tests.Repositories.Implementations;

public class AzurePhishingDomainRepositoryTests
{
    private readonly Mock<AzurePhishingDomainStorageService> _mockStorageService;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<AzurePhishingDomainRepository>> _mockLogger;
    private readonly AzurePhishingDomainRepository _repository;

    public AzurePhishingDomainRepositoryTests()
    {
        _mockStorageService = new Mock<AzurePhishingDomainStorageService>(MockBehavior.Strict, null, null);
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<AzurePhishingDomainRepository>>();
        _repository = new AzurePhishingDomainRepository(_mockStorageService.Object, _mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UpdatePhishingDomainsAsync_LogsDebugAfterCacheUpdate()
    {
        // Arrange
        var domains = new List<string> { "domain1.com", "domain2.com" };
        var checksum = "checksum123";

        _mockStorageService.Setup(s => s.UpdateDomainsAsync(It.IsAny<List<string>>(), checksum))
            .Returns(Task.CompletedTask);

        _mockCache.SetupSequence(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
            .Returns(Task.CompletedTask)
            .Returns(Task.CompletedTask);

        // Capture the log messages
        var loggedDebug = false;
        _mockLogger.Setup(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Updated phishing domains cache after update operation")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback(() => loggedDebug = true);

        // Act
        await _repository.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert
        Assert.True(loggedDebug);
        _mockStorageService.Verify(s => s.UpdateDomainsAsync(It.Is<List<string>>(l => l.Count == domains.Count), checksum), Times.Once);
        _mockCache.Verify(c => c.SetStringAsync("PhishingDomains_v1", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()), Times.Once);
        _mockCache.Verify(c => c.SetStringAsync("PhishingDomains_Checksum_v1", checksum, It.IsAny<DistributedCacheEntryOptions>()), Times.Once);
    }
}
