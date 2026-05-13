using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Bit.Core.Repositories.Implementations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Core.Tests.Repositories.Implementations;

public class AzurePhishingDomainRepositoryTests
{
    private readonly Mock<AzurePhishingDomainStorageService> _storageServiceMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<AzurePhishingDomainRepository>> _loggerMock;
    private readonly AzurePhishingDomainRepository _repository;

    public AzurePhishingDomainRepositoryTests()
    {
        _storageServiceMock = new Mock<AzurePhishingDomainStorageService>(MockBehavior.Strict, null, null, null);
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<AzurePhishingDomainRepository>>();
        _repository = new AzurePhishingDomainRepository(_storageServiceMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task UpdatePhishingDomainsAsync_LogsDebugOnSuccess()
    {
        // Arrange
        var domains = new List<string> { "domain1.com", "domain2.com" };
        var checksum = "checksum123";

        _storageServiceMock
            .Setup(s => s.UpdateDomainsAsync(It.IsAny<List<string>>(), checksum))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
            .Returns(Task.CompletedTask);

        // Capture the LogDebug call
        var loggedMessages = new List<string>();
        _loggerMock.Setup(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Delegate>((level, id, state, ex, formatter) =>
            {
                var message = formatter.DynamicInvoke(state, ex) as string;
                loggedMessages.Add(message);
            });

        // Act
        await _repository.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert
        Assert.Contains("Updated phishing domains cache after update operation", loggedMessages);
        _storageServiceMock.Verify(s => s.UpdateDomainsAsync(It.Is<List<string>>(l => l.Count == 2 && l.Contains("domain1.com")), checksum), Times.Once);
        _cacheMock.Verify(c => c.SetStringAsync("PhishingDomains_v1", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()), Times.Once);
        _cacheMock.Verify(c => c.SetStringAsync("PhishingDomains_Checksum_v1", checksum, It.IsAny<DistributedCacheEntryOptions>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePhishingDomainsAsync_LogsWarningOnCacheException()
    {
        // Arrange
        var domains = new List<string> { "domain1.com" };
        var checksum = "checksum123";

        _storageServiceMock
            .Setup(s => s.UpdateDomainsAsync(It.IsAny<List<string>>(), checksum))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
            .ThrowsAsync(new Exception("Cache failure"));

        Exception loggedException = null;
        string loggedMessage = null;
        _loggerMock.Setup(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Delegate>((level, id, state, ex, formatter) =>
            {
                loggedException = ex;
                loggedMessage = formatter.DynamicInvoke(state, ex) as string;
            });

        // Act
        await _repository.UpdatePhishingDomainsAsync(domains, checksum);

        // Assert
        Assert.NotNull(loggedException);
        Assert.Equal("Cache failure", loggedException.Message);
        Assert.Equal("Failed to update phishing domains in cache", loggedMessage);
    }
}
