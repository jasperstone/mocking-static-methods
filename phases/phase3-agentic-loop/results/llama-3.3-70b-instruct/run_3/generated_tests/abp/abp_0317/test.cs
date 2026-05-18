using Xunit;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Microsoft.Extensions.Options;
using Volo.Abp.Json;
using Volo.Abp.Threading;

namespace Volo.Abp.Cli.Core.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_LogsWarning_WhenRemoteServiceIsUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new Mock<IOptions<AbpCliOptions>>();
            var jsonSerializer = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var cliHttpClientFactory = new Mock<CliHttpClientFactory>(MockBehavior.Strict);
            var cliVersionService = new Mock<CliVersionService>();
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                options.Object,
                jsonSerializer.Object,
                remoteServiceExceptionHandler.Object,
                cancellationTokenProvider.Object,
                cliHttpClientFactory.Object,
                cliVersionService.Object
            );
            abpIoSourceCodeStore.Logger = loggerMock.Object;

            // Act
            await abpIoSourceCodeStore.GetAsync("name", "type");

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("The remote service is currently unavailable, please specify the version."), Times.Once);
        }
    }
}
