using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bit.Core.Repositories.Implementations.Tests
{
    public class AzurePhishingDomainRepositoryTests
    {
        [Fact]
        public async Task UpdatePhishingDomainsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AzurePhishingDomainRepository>>();
            var storageServiceMock = new Mock<AzurePhishingDomainStorageService>();
            var cacheMock = new Mock<IDistributedCache>();
            var repository = new AzurePhishingDomainRepository(storageServiceMock.Object, cacheMock.Object, loggerMock.Object);

            // Act
            await repository.UpdatePhishingDomainsAsync(new List<string>(), "checksum");

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)((state, exception) => state.ToString())),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePhishingDomainsAsync_LogsWarningMessage_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AzurePhishingDomainRepository>>();
            var storageServiceMock = new Mock<AzurePhishingDomainStorageService>();
            var cacheMock = new Mock<IDistributedCache>();
            cacheMock.Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                .Throws(new Exception("Test exception"));
            var repository = new AzurePhishingDomainRepository(storageServiceMock.Object, cacheMock.Object, loggerMock.Object);

            // Act
            await repository.UpdatePhishingDomainsAsync(new List<string>(), "checksum");

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)((state, exception) => state.ToString())),
                Times.Once);
        }
    }
}
