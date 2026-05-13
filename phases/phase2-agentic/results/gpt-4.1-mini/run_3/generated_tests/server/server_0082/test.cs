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
    [Fact]
    public async Task UpdatePhishingDomainsAsync_LogsDebugOnSuccess()
    {
        // Arrange
        var mockStorageService = new Mock<AzurePhishingDomainStorageService>(MockBehavior.Strict, null, null, null);
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<AzurePhishingDomainRepository>>();

        var domains = new List<string> { "domain1.com", "domain2.com" };
        var checksum = "checksum123";

        mockStorageService
            .Setup(s => s.UpdateDomainsAsync(It.IsAny<List<string>>(), checksum))
            .Returns(Task.CompletedTask);

        mockCache
            .Setup(c => c.SetStringAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                default))
            .Returns(Task.CompletedTask);

        // We want to verify that LogDebug is called with the expected message
        mockLogger
            .Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Updated phishing domains cache after update operation"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Verifiable();

        var repo = new AzurePhishingDomainRepository(
            mockStorageService.Object,
            mockCache.Object,
            mockLogger.Object);

        // Act
        await repo.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert
        mockLogger.Verify();
    }

    [Fact]
    public async Task UpdatePhishingDomainsAsync_LogsWarningOnCacheException()
    {
        // Arrange
        var mockStorageService = new Mock<AzurePhishingDomainStorageService>(MockBehavior.Strict, null, null, null);
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<AzurePhishingDomainRepository>>();

        var domains = new List<string> { "domain1.com" };
        var checksum = "checksum";

        mockStorageService
            .Setup(s => s.UpdateDomainsAsync(It.IsAny<List<string>>(), checksum))
            .Returns(Task.CompletedTask);

        mockCache
            .Setup(c => c.SetStringAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                default))
            .ThrowsAsync(new Exception("Cache failure"));

        mockLogger
            .Setup(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Failed to update phishing domains in cache"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Verifiable();

        var repo = new AzurePhishingDomainRepository(
            mockStorageService.Object,
            mockCache.Object,
            mockLogger.Object);

        // Act
        await repo.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert
        mockLogger.Verify();
    }
}
