using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Volo.Abp.Http;
using Volo.Abp.Threading;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_UsesLocalTemplate_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = Options.Create(new AbpCliOptions());
            var jsonSerializer = new DefaultJsonSerializer(); // Assuming DefaultJsonSerializer is the correct implementation
            var remoteServiceExceptionHandler = new DefaultRemoteServiceExceptionHandler(); // Assuming DefaultRemoteServiceExceptionHandler is the correct implementation
            var cancellationTokenProvider = new DefaultCancellationTokenProvider(); // Assuming DefaultCancellationTokenProvider is the correct implementation
            var cliHttpClientFactory = new CliHttpClientFactory(); // Assuming CliHttpClientFactory is the correct implementation
            var cliVersionService = new CliVersionService(); // Assuming CliVersionService is the correct implementation

            var store = new AbpIoSourceCodeStore(
                options,
                jsonSerializer,
                remoteServiceExceptionHandler,
                cancellationTokenProvider,
                cliHttpClientFactory,
                cliVersionService)
            {
                Logger = loggerMock.Object
            };

            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";
            var templateSource = "local/path";

            // Act
            await store.GetAsync(name, type, version, templateSource);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s == "Using local Template: TestTemplate, version: 1.0.0")),
                Times.Once);
        }
    }
}
