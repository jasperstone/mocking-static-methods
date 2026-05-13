using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;
using System.Threading;
using Bit.Core.Repositories.Implementations;

namespace Bit.Tests
{
    public class AzurePhishingDomainRepositoryTests
    {
        private readonly Mock<AzurePhishingDomainStorageService> _storageServiceMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<AzurePhishingDomainRepository>> _loggerMock;
        private readonly AzurePhishingDomainRepository _repository;

        public AzurePhishingDomainRepositoryTests()
        {
            _storageServiceMock = new Mock<AzurePhishingDomainStorageService>();
            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<AzurePhishingDomainRepository>>();
            _repository = new AzurePhishingDomainRepository(
                _storageServiceMock.Object,
                _cacheMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetCurrentChecksumAsync_Should_LogDebug_When_CacheHit()
        {
            // Arrange
            var checksum = "abc123";
            _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(checksum);

            // Act
            var result = await _repository.GetCurrentChecksumAsync();

            // Assert
            Assert.Equal(checksum, result);
            _loggerMock.Verify(
                x => x.LogDebug("Retrieved phishing domain checksum from cache"),
                Times.Once);
        }
    }
}
