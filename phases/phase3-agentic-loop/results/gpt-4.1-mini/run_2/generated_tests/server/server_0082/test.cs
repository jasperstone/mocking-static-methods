using System;
using System.Collections.Generic;
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
        _mockStorageService = new Mock<AzurePhishingDomainStorageService>(MockBehavior.Strict);
        _mockCache = new Mock<IDistributedCache>(MockBehavior.Strict);
        _mockLogger = new Mock<ILogger<AzurePhishingDomainRepository>>(MockBehavior.Strict);
        _repository = new AzurePhishingDomainRepository(_mockStorageService.Object, _mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UpdatePhishingDomainsAsync_LogsDebugAfterCacheUpdate()
    {
        // Arrange
        var domains = new List<string> { "domain1.com", "domain2.com" };
        var checksum = "checksum123";

        _mockStorageService
            .Setup(s => s.UpdateDomainsAsync(It.IsAny<List<string>>(), checksum))
            .Returns(Task.CompletedTask);

        _mockCache
            .Setup(c => c.SetStringAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DistributedCacheEntryOptions>()))
            .Returns(Task.CompletedTask);

        // Setup logger to expect LogDebug call with the exact message
        _mockLogger
            .SetupLogDebug("Updated phishing domains cache after update operation");

        // Act
        await _repository.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert
        _mockStorageService.Verify(s => s.UpdateDomainsAsync(It.Is<List<string>>(l => l.Count == domains.Count), checksum), Times.Once);
        _mockCache.Verify(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()), Times.Exactly(2));
        _mockLogger.VerifyLogDebug("Updated phishing domains cache after update operation", Times.Once);
    }
}

public static class LoggerExtensionsForTests
{
    public static void SetupLogDebug<T>(this Mock<ILogger<T>> mockLogger, string expectedMessage)
    {
        mockLogger.Setup(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();
    }

    public static void VerifyLogDebug<T>(this Mock<ILogger<T>> mockLogger, string expectedMessage, Times times)
    {
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
