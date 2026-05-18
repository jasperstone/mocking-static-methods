using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.Core.PhishingDomainFeatures;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Core.Repositories.Implementations.Tests
{
    public class AzurePhishingDomainRepositoryTests
    {
        [Fact]
        public async Task UpdatePhishingDomainsAsync_LogsDebugMessage()
        {
            // Arrange
            var storageServiceMock = new Mock<IAzurePhishingDomainStorageService>();
            var cacheMock = new Mock<IDistributedCache>();
            var loggerMock = new Mock<ILogger<AzurePhishingDomainRepository>>();

            var repository = new AzurePhishingDomainRepository(
                storageServiceMock.Object,
                cacheMock.Object,
                loggerMock.Object);

            var domains = new List<string> { "example.com", "test.com" };
            var checksum = "checksum123";

            // Act
            await repository.UpdatePhishingDomainsAsync(domains, checksum);

            // Assert
            loggerMock.Verify(
                logger => logger.LogDebug(
                    It.Is<string>(s => s == "Updated phishing domains cache after update operation"),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
