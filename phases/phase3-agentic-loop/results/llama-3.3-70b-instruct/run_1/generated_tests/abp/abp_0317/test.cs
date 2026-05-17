using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_LogsWarning_WhenRemoteServiceIsUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                Mock.Of<Microsoft.Extensions.Options.IOptions<Volo.Abp.Cli.AbpCliOptions>>(),
                Mock.Of<Volo.Abp.Json.IJsonSerializer>(),
                Mock.Of<Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler>(),
                Mock.Of<Volo.Abp.Threading.ICancellationTokenProvider>(),
                Mock.Of<Volo.Abp.Cli.Http.CliHttpClientFactory>(),
                Mock.Of<Volo.Abp.Cli.Version.CliVersionService>()
            );
            abpIoSourceCodeStore.Logger = loggerMock.Object;

            // Act
            await Assert.ThrowsAsync<CliUsageException>(() => abpIoSourceCodeStore.GetAsync("name", "type"));

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("The remote service is currently unavailable, please specify the version."), Times.Once);
        }
    }
}
