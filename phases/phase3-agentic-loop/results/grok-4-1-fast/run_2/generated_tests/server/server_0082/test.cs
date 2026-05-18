using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bit.Core.Repositories;
using Bit.Core.Repositories.Implementations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bit.Core.Repositories.Implementations.Tests
{
    public class AzurePhishingDomainRepositoryTests
    {
        private readonly Mock<AzurePhishingDomainStorageService> _mockStorageService;
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly Mock<ILogger<AzurePhishingDomainRepository>> _mockLogger;
        private readonly AzurePhishingDomainRepository _repository;

        public AzurePhishingDomainRepositoryTests()
        {
            _mockStorageService = new();
            _mockCache = new();
            _mockLogger = new();

            _repository = new AzurePhishingDomainRepository(
                _mockStorageService.Object,
                _mockCache.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_LogsDebugMessageOnSuccess()
        {
            // Arrange
            var domains = new List<string> { "example.com" };
            var checksum = "abc123";
            _mockCache
                .Setup(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                .Returns(Task.CompletedTask);
            _mockStorageService
                .Setup(x => x.UpdateDomainsAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Updated phishing domains cache after update operation")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_LogsWarningOnCacheException()
        {
            // Arrange
            var domains = new List<string> { "example.com" };
            var checksum = "abc123";
            var exception = new InvalidOperationException("Cache failure");
            
            _mockCache
                .SetupSequence(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                .Returns(Task.CompletedTask)
                .ThrowsAsync(exception);
            _mockStorageService
                .Setup(x => x.UpdateDomainsAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            _mockLogger.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
