using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core.PhishingDomainFeatures;
using Bit.Core.Repositories.Implementations;

namespace Bit.Core.Tests.Repositories.Implementations
{
    public class AzurePhishingDomainRepositoryTests
    {
        [Fact]
        public async Task UpdatePhishingDomainsAsync_LogsDebugMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<AzurePhishingDomainRepository>>();
            var mockStorageService = new Mock<IAzurePhishingDomainStorageService>();
            var mockCache = new Mock<IDistributedCache>();

            var repository = new AzurePhishingDomainRepository(
                mockStorageService.Object,
                mockCache.Object,
                mockLogger.Object);

            var domains = new List<string> { "example.com", "test.com" };
            var checksum = "checksum123";

            // Act
            await repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug(
                    It.Is<string>(s => s == "Updated phishing domains cache after update operation"),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
