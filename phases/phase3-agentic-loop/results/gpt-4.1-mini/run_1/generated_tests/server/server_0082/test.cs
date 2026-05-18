using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core.Repositories.Implementations;
using Bit.Core.PhishingDomainFeatures;
using Bit.Core.Settings;

namespace Bit.Core.Tests.Repositories.Implementations
{
    public class AzurePhishingDomainRepositoryTests
    {
        private readonly Mock<AzurePhishingDomainStorageService> _mockStorageService;
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly Mock<ILogger<AzurePhishingDomainRepository>> _mockLogger;
        private readonly AzurePhishingDomainRepository _repository;

        public AzurePhishingDomainRepositoryTests()
        {
            var mockGlobalSettings = new Mock<GlobalSettings>();
            var mockStorageLogger = new Mock<ILogger<AzurePhishingDomainStorageService>>();
            _mockStorageService = new Mock<AzurePhishingDomainStorageService>(mockGlobalSettings.Object, mockStorageLogger.Object);
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

            _mockCache.Setup(c => c.SetStringAsync(
                It.Is<string>(k => k == "PhishingDomains_v1"),
                It.IsAny<string>(),
                It.IsAny<DistributedCacheEntryOptions>()))
                .Returns(Task.CompletedTask);

            _mockCache.Setup(c => c.SetStringAsync(
                It.Is<string>(k => k == "PhishingDomains_Checksum_v1"),
                checksum,
                It.IsAny<DistributedCacheEntryOptions>()))
                .Returns(Task.CompletedTask);

            // Capture the LogDebug call
            var loggedMessages = new List<string>();
            _mockLogger.Setup(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Delegate>((level, eventId, state, ex, formatter) =>
                {
                    var message = formatter.DynamicInvoke(state, ex) as string;
                    loggedMessages.Add(message);
                });

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            Assert.Contains("Updated phishing domains cache after update operation", loggedMessages);
            _mockStorageService.Verify(s => s.UpdateDomainsAsync(It.IsAny<List<string>>(), checksum), Times.Once);
            _mockCache.Verify(c => c.SetStringAsync("PhishingDomains_v1", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()), Times.Once);
            _mockCache.Verify(c => c.SetStringAsync("PhishingDomains_Checksum_v1", checksum, It.IsAny<DistributedCacheEntryOptions>()), Times.Once);
            _mockLogger.VerifyAll();
        }
    }
}
